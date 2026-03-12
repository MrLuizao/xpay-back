using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Logic;
using ReferenciaXPayAPI_Core.Services;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ReferenciaLogic _logic;
        private readonly IJwtService _jwtService;

        public UsuariosController(ReferenciaLogic logic, IJwtService jwtService)
        {
            _logic = logic;
            _jwtService = jwtService;
        }

        [HttpPost]
        public IActionResult Post([FromBody] UsuarioRegistroModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Celular) || model.RolXPayId <= 0)
            {
                return BadRequest(new ApiResponse<UsuarioModel> 
                { 
                    Code = "400", 
                    Message = "UserId, Celular y RolXPayId son campos obligatorios." 
                });
            }

            ApiResponse<UsuarioModel> resp = _logic.RegistrarUsuario(model);

            return resp.Code switch
            {
                "success" or "201" or "00" or "OK" => StatusCode(201, resp),
                "409" => Conflict(resp),
                "500" => StatusCode(500, resp),
                _ => BadRequest(resp)
            };
        }

        [HttpPost("RegistroLocal")] // Cambiado de PUT a POST por solicitud para generar registro
        public IActionResult RegistroLocal([FromBody] UsuarioRegistroModel model)
        {
            if (model == null)
            {
                return BadRequest(new ApiResponse<UsuarioModel> { Code = "400", Message = "El modelo no puede ser nulo." });
            }

            if (string.IsNullOrEmpty(model.UserId))
            {
                model.UserId = Guid.NewGuid().ToString();
            }

            ApiResponse<UsuarioModel> resp = _logic.RegistrarUsuario(model);

            return resp.Code switch
            {
                "success" or "201" or "00" or "OK" => StatusCode(201, resp),
                "409" => Conflict(resp),
                "500" => StatusCode(500, resp),
                _ => BadRequest(resp)
            };
        }

        [HttpDelete("{userId}")]
        public IActionResult Delete(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ApiResponse<string> { Code = "400", Message = "UserId es obligatorio." });
            }

            ApiResponse<string> resp = _logic.EliminarUsuario(userId);

            return resp.Code switch
            {
                "success" or "200" => Ok(resp),
                "404" => NotFound(resp),
                "500" => StatusCode(500, resp),
                _ => BadRequest(resp)
            };
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequestModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new ApiResponse<UsuarioModel> { Code = "400", Message = "El Password es obligatorio." });
            }

            ApiResponse<UsuarioModel> resp = _logic.LoginUsuario(model);

            if (resp.Code == "success" && resp.Data != null)
            {
                // Generar token JWT
                var token = _jwtService.GenerateToken(resp.Data);
                
                // Retornar respuesta con token incluido
                return Ok(new 
                {
                    code = resp.Code,
                    message = resp.Message,
                    token = token,
                    tokenType = "Bearer",
                    expiresIn = 86400, // 24 horas
                    data = resp.Data
                });
            }

            return resp.Code switch
            {
                "401" => Unauthorized(resp),
                "403" => StatusCode(403, resp),
                "500" => StatusCode(500, resp),
                _ => BadRequest(resp)
            };
        }
    }
}
