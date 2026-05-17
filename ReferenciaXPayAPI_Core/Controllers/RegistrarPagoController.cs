using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Logic;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Filters;
using System;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireAuthentication]
    public class RegistrarPagoController : ControllerBase
    {
        private readonly ReferenciaLogic _logic;

        public RegistrarPagoController(ReferenciaLogic logic)
        {
            _logic = logic;
        }

        [HttpPost]
        public IActionResult Post([FromBody] RegistrarPagoModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.NumeroTransaccion))
                {
                    _logic.GrabaLog("RegistrarPago: Sin definicion de transaccion", "err");
                    return BadRequest(new { code = "400", message = "El Número de Transacción es obligatorio" });
                }

                _logic.GrabaLog($"RegistrarPago solicitado: Transaccion={model.NumeroTransaccion}, Importe={model.Importe}, UsuarioId={model.UsuarioXPayId}", "info");

                var response = _logic.RegistrarPago(model);

                if (response.Code == "success")
                {
                    return Ok(response);
                }
                else if (response.Code == "404" || response.Code == "14")
                {
                    // Fallo o no encontrado en el SP
                    return BadRequest(response); // Regresamos 400 Bad Request o según convenga
                }
                else
                {
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logic.GrabaLog(ex.ToString(), "err_critico_registrarpago");
                return StatusCode(500, new { code = "500", message = "Error interno del servidor", detail = ex.Message });
            }
        }
    }
}
