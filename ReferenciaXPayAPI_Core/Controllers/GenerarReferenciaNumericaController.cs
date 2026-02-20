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
                if (model == null || string.IsNullOrEmpty(model.Referencia))
                {
                    _logic.GrabaLog("Sin definicion", "err");
                    resp.respcode = "400";
                    return BadRequest(new { code = "400", message = "Referencia es obligatoria" });
                }

                var jsonReqMessage = JsonConvert.SerializeObject(model);
                _logic.GrabaLog(jsonReqMessage, "json");

                string cReferencia = model.Referencia;
                string cRespcode = string.Empty;
                string cReferenciaNumerica = string.Empty;

                int status = _logic.GenerarBD(cReferencia, ref cRespcode, ref cReferenciaNumerica);

                if (status == 0)
                {
                    DateTime? fechaVigenciaIMSS = null;
                    double? importeIMSS = null;

                    string regPat = string.Empty;
                    string perPag = string.Empty;
                    string origen = string.Empty;
                    string fsua = string.Empty;
                    string fechVenc = string.Empty;
                    string impImss = string.Empty;
                    string impRcv = string.Empty; 
                    string impApv = string.Empty;
                    string impAcv = string.Empty;
                    
                    int mRet = _logic.ValidaReferencia(cReferencia, ref cRespcode);

                    if (mRet == 0)
                    {
                        _logic.ObtenerCampos(cReferencia, ref regPat, ref perPag, ref origen, ref fsua, ref fechVenc, ref impImss, ref impRcv, ref impApv, ref impAcv);
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

                        resp.respcode = "00";
                        resp.referenciaNumerica = cReferenciaNumerica;
                        resp.vigencia = fechaVigenciaIMSS;
                        resp.monto = importeIMSS;
                        return Ok(resp);
                    }
                    else
                    {
                        // La validación local falló, pero la base de datos SÍ generó la referencia numérica.
                        // Entregamos la referencia pero omitimos los campos calculados (Monto/Vigencia).
                        _logic.GrabaLog($"Validacion local falló ({cRespcode}), pero se entrega referencia de DB.", "warn");
                        resp.respcode = "00"; // Forzamos éxito porque la DB lo aceptó
                        resp.referenciaNumerica = cReferenciaNumerica;
                        return Ok(resp);
                    }
                }
                else
                {
                    resp.respcode = "14";
                    resp.referenciaNumerica = string.Empty;
                    return StatusCode(500, new { code = "500", message = "Error en el procesamiento de la base de datos", detail = cRespcode });
                }
            }
            catch (Exception ex)
            {
                _logic.GrabaLog(ex.ToString(), "err_critico");
                // ENVÍO EL ERROR CRUDO Y COMPLETO PARA PODER VER QUÉ SE ROMPE EN C#
                return StatusCode(500, new { code = "500", message = "Error Crítico .NET", detail = ex.ToString() });
            }
        }
    }
}
