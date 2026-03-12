using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Logic;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Filters;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [RequireAuthentication]
    public class HistorialRecibosController : ControllerBase
    {
        private readonly ReferenciaLogic _logic;

        public HistorialRecibosController(ReferenciaLogic logic)
        {
            _logic = logic;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string usuarioXPayId = "")
        {
            var response = _logic.ObtenerHistorialRecibos(usuarioXPayId);

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
