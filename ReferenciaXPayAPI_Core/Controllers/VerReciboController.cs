using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Logic;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Filters;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [RequireAuthentication]
    public class VerReciboController : ControllerBase
    {
        private readonly ReferenciaLogic _logic;

        public VerReciboController(ReferenciaLogic logic)
        {
            _logic = logic;
        }

        /// <summary>
        /// Obtiene el cuerpo del ticket/recibo almacenado en BD a partir del folio de transacción.
        /// </summary>
        /// <param name="folioTransaccion">Folio de transacción (@IDS_NUM_STANDIN)</param>
        /// <returns>Ticket formateado desde la BD</returns>
        [HttpGet]
        public IActionResult Get([FromQuery] int folioTransaccion)
        {
            if (folioTransaccion <= 0)
            {
                return BadRequest(new ApiResponse<VerReciboModel>
                {
                    Code = "400",
                    Message = "folioTransaccion es obligatorio"
                });
            }

            var response = _logic.ObtenerCuerpoTicket(folioTransaccion);

            if (response.Code == "success")
            {
                return Ok(response);
            }
            else if (response.Code == "404")
            {
                return NotFound(response);
            }
            else
            {
                return StatusCode(500, response);
            }
        }
    }
}
