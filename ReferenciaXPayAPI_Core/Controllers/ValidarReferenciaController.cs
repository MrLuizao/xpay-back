using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Logic;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ValidarReferenciaController : ControllerBase
    {
        private readonly ReferenciaLogic _logic;

        public ValidarReferenciaController(ReferenciaLogic logic)
        {
            _logic = logic;
        }

        [HttpPost]
        public IActionResult Post([FromBody] ValidarReferenciaRequestModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Referencia))
            {
                return BadRequest(new ApiResponse<ValidarReferenciaResponseModel>
                {
                    Code = "400",
                    Message = "Referencia es obligatoria"
                });
            }

            var response = _logic.ValidarReferencia(model.Referencia);

            if (response.Code == "success")
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
    }
}
