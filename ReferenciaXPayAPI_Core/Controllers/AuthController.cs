using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Services;
using ReferenciaXPayAPI_Core.Logic;
using System.Security.Claims;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ReferenciaLogic _logic;
        private readonly IJwtService _jwtService;

        public AuthController(ReferenciaLogic logic, IJwtService jwtService)
        {
            _logic = logic;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Autentica usuario y genera token JWT
        /// </summary>
        /// <param name="model">Credenciales de login</param>
        /// <returns>Token JWT y datos del usuario</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestModel model)
        {
            try
            {
                if (model == null || (string.IsNullOrEmpty(model.UserId) && string.IsNullOrEmpty(model.Email)))
                {
                    return BadRequest(new { code = "400", message = "UserId o Email son obligatorios" });
                }

                var loginResponse = _logic.LoginUsuario(model);

                if (loginResponse.Code == "success" && loginResponse.Data != null)
                {
                    // Generar token JWT
                    var token = _jwtService.GenerateToken(loginResponse.Data);

                    // Retornar respuesta con token
                    return Ok(new
                    {
                        code = "success",
                        message = "Login exitoso",
                        token = token,
                        tokenType = "Bearer",
                        expiresIn = 86400, // 24 horas en segundos
                        user = new
                        {
                            userId = loginResponse.Data.UserId,
                            nombre = loginResponse.Data.Nombre,
                            apellido = loginResponse.Data.Apellido,
                            email = loginResponse.Data.Email,
                            celular = loginResponse.Data.Celular,
                            rolXPayId = loginResponse.Data.RolXPayId,
                            usuarioXPayId = loginResponse.Data.UsuarioXPayId
                        }
                    });
                }
                else if (loginResponse.Code == "403")
                {
                    return StatusCode(403, new { code = "403", message = loginResponse.Message });
                }
                else
                {
                    return Unauthorized(new { code = "401", message = loginResponse.Message });
                }
            }
            catch (Exception ex)
            {
                _logic.GrabaLog("Error en AuthController.Login: " + ex.Message, "Auth_Error");
                return StatusCode(500, new { code = "500", message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Valida un token JWT y devuelve la información del usuario
        /// </summary>
        /// <returns>Información del token validado</returns>
        [HttpPost("validate")]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Token))
                {
                    return BadRequest(new { code = "400", message = "Token es obligatorio" });
                }

                var principal = _jwtService.ValidateToken(model.Token);
                if (principal == null)
                {
                    return Unauthorized(new { code = "401", message = "Token inválido o expirado" });
                }

                // Extraer claims del token
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var nombre = principal.FindFirst(ClaimTypes.Name)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var usuarioXPayId = principal.FindFirst("UsuarioXPayId")?.Value;
                var rolXPayId = principal.FindFirst("RolXPayId")?.Value;

                return Ok(new
                {
                    code = "success",
                    message = "Token válido",
                    valid = true,
                    user = new
                    {
                        userId,
                        nombre,
                        email,
                        usuarioXPayId = string.IsNullOrEmpty(usuarioXPayId) ? (int?)null : int.Parse(usuarioXPayId),
                        rolXPayId = string.IsNullOrEmpty(rolXPayId) ? (int?)null : int.Parse(rolXPayId)
                    }
                });
            }
            catch (Exception ex)
            {
                _logic.GrabaLog("Error en AuthController.ValidateToken: " + ex.Message, "Auth_Error");
                return StatusCode(500, new { code = "500", message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Refresca un token JWT existente
        /// </summary>
        /// <returns>Nuevo token JWT</returns>
        [HttpPost("refresh")]
        public IActionResult RefreshToken()
        {
            try
            {
                // Obtener token del header Authorization
                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { code = "400", message = "Token Bearer es obligatorio" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var principal = _jwtService.ValidateToken(token);
                
                if (principal == null)
                {
                    return Unauthorized(new { code = "401", message = "Token inválido o expirado" });
                }

                // Crear nuevo token con la misma información
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var nombre = principal.FindFirst(ClaimTypes.Name)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var usuarioXPayId = principal.FindFirst("UsuarioXPayId")?.Value;
                var rolXPayId = principal.FindFirst("RolXPayId")?.Value;

                // Reconstruir modelo de usuario para generar nuevo token
                var user = new UsuarioModel
                {
                    UserId = userId ?? "",
                    Nombre = nombre?.Split(' ')[0] ?? "",
                    Apellido = nombre?.Split(' ').Length > 1 ? string.Join(" ", nombre.Split(' ').Skip(1)) : "",
                    Email = email,
                    UsuarioXPayId = string.IsNullOrEmpty(usuarioXPayId) ? null : int.Parse(usuarioXPayId),
                    RolXPayId = string.IsNullOrEmpty(rolXPayId) ? 1 : int.Parse(rolXPayId)
                };

                var newToken = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    code = "success",
                    message = "Token refrescado exitosamente",
                    token = newToken,
                    tokenType = "Bearer",
                    expiresIn = 7200
                });
            }
            catch (Exception ex)
            {
                _logic.GrabaLog("Error en AuthController.RefreshToken: " + ex.Message, "Auth_Error");
                return StatusCode(500, new { code = "500", message = "Error interno del servidor" });
            }
        }
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
