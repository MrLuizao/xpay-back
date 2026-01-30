using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Logic;
using Newtonsoft.Json;
using System.Net;
using System.Globalization;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerarReferenciaNumericaController : ControllerBase
    {
        private readonly ReferenciaLogic _logic;

        public GenerarReferenciaNumericaController(ReferenciaLogic logic)
        {
            _logic = logic;
        }

        [HttpPost]
        public IActionResult Post([FromBody] GeneraReferenciaNumericaModel model)
        {
            GeneraReferenciaNumericaResponse resp = new GeneraReferenciaNumericaResponse();

            try
            {
                if (model == null)
                {
                    _logic.GrabaLog("Sin definicion", "err");
                    resp.Respcode = "14";
                    return NoContent();
                }

                var jsonReqMessage = JsonConvert.SerializeObject(model);
                _logic.GrabaLog(jsonReqMessage, "json");

                string cReferencia = model.Referencia;
                string cRespcode;
                string cReferenciaNumerica;

                int status = _logic.GenerarBD(cReferencia, out cRespcode, out cReferenciaNumerica);

                if (status == 0)
                {
                    DateTime? fechaVigenciaIMSS = null;
                    double? importeIMSS = null;

                    string regPat, perPag, origen, fsua, fechVenc, impImss, impRcv, impApv, impAcv;
                    
                    int mRet = _logic.ValidaReferencia(cReferencia, out cRespcode);

                    if (mRet == 0)
                    {
                        _logic.ObtenerCampos(cReferencia, out regPat, out perPag, out origen, out fsua, out fechVenc, out impImss, out impRcv, out impApv, out impAcv);
                        _logic.GrabaLog(fechVenc, "Fecha: ");
                        _logic.GrabaLog(impImss, "Importe: ");

                        if (DateTime.TryParseExact(fechVenc, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            fechaVigenciaIMSS = parsedDate;
                        }

                        if (double.TryParse(impImss, out double parsedImporte))
                        {
                            importeIMSS = parsedImporte;
                        }
                    }

                    resp.Respcode = "00";
                    resp.ReferenciaNumerica = cReferenciaNumerica;
                    resp.Vigencia = fechaVigenciaIMSS;
                    resp.Monto = importeIMSS;
                }
                else
                {
                    resp.Respcode = "14";
                    resp.ReferenciaNumerica = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logic.GrabaLog(ex.Message, "err");
                resp.Respcode = "30";
                resp.ReferenciaNumerica = string.Empty;
                return BadRequest(resp);
            }

            return Ok(resp);
        }
    }
}
