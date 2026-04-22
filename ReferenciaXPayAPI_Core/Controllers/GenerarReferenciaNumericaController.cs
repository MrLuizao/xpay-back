using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Logic;
using Newtonsoft.Json;
using ReferenciaXPayAPI_Core.Filters;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    
    public class GenerarReferenciaNumericaController : ControllerBase
    {
        private readonly ReferenciaLogic _logic;
        private readonly QRReaderService _qrService;

        public GenerarReferenciaNumericaController(ReferenciaLogic logic, QRReaderService qrService)
        {
            _logic = logic;
            _qrService = qrService;
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

                int status = _logic.GenerarBD(cReferencia, ref cRespcode, ref cReferenciaNumerica, model.UsuarioXPayId ?? "");

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
                        _logic.GrabaLog($"{impImss} | {impRcv} | {impApv} | {impAcv}", "Importes Extraídos: ");

                        if (DateTime.TryParseExact(fechVenc, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            fechaVigenciaIMSS = parsedDate;
                        }

                        // Sumamos todos los componentes y dividimos entre 100 para corregir decimales
                        double.TryParse(impImss, NumberStyles.Any, CultureInfo.InvariantCulture, out double dImss);
                        double.TryParse(impRcv, NumberStyles.Any, CultureInfo.InvariantCulture, out double dRcv);
                        double.TryParse(impApv, NumberStyles.Any, CultureInfo.InvariantCulture, out double dApv);
                        double.TryParse(impAcv, NumberStyles.Any, CultureInfo.InvariantCulture, out double dAcv);
                        
                        importeIMSS = (dImss + dRcv + dApv + dAcv);

                        resp.respcode = "00";
                        resp.referenciaNumerica = cReferenciaNumerica;
                        resp.referenciaXPay = cReferencia;
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
                        resp.referenciaXPay = cReferencia;
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

        /// <summary>
        /// Genera una referencia numérica a partir de un archivo QR o código de barras
        /// </summary>
        /// <param name="documento">Archivo PDF o imagen con código QR</param>
        /// <param name="usuarioXPayId">ID del usuario que genera la referencia</param>
        /// <returns>Referencia numérica generada</returns>
        [HttpPost("Archivo")]
        public IActionResult PostArchivo([FromForm] IFormFile documento, [FromQuery] string usuarioXPayId = "")
        {
            GeneraReferenciaNumericaResponse resp = new GeneraReferenciaNumericaResponse();

            try
            {
                if (documento == null || documento.Length == 0)
                {
                    _logic.GrabaLog("Sin archivo adjunto", "err");
                    resp.respcode = "400";
                    return BadRequest(new { code = "400", message = "El documento es obligatorio" });
                }

                string qrText = null;
                var ext = Path.GetExtension(documento.FileName).ToLower();

                using (var stream = documento.OpenReadStream())
                {
                    if (ext == ".pdf")
                    {
                        qrText = _qrService.ReadCodeFromPdf(stream);
                    }
                    else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    {
                        qrText = _qrService.ReadCodeFromImage(stream);
                    }
                    else
                    {
                        resp.respcode = "400";
                        return BadRequest(new { code = "400", message = "Formato de archivo no soportado. Sólo PDF, PNG y JPG." });
                    }
                }

                if (string.IsNullOrEmpty(qrText))
                {
                    resp.respcode = "400";
                    return BadRequest(new { code = "400", message = "No se pudo detectar un código QR o de barras válido en el documento." });
                }

                _logic.GrabaLog($"Código detectado desde archivo: {qrText}", "info");

                // Reuse logic to generate reference
                string cReferencia = qrText;
                string cRespcode = string.Empty;
                string cReferenciaNumerica = string.Empty;

                int status = _logic.GenerarBD(cReferencia, ref cRespcode, ref cReferenciaNumerica, usuarioXPayId);

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

                        if (DateTime.TryParseExact(fechVenc, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            fechaVigenciaIMSS = parsedDate;
                        }

                        // Sumamos todos los componentes
                        double.TryParse(impImss, NumberStyles.Any, CultureInfo.InvariantCulture, out double dImss);
                        double.TryParse(impRcv, NumberStyles.Any, CultureInfo.InvariantCulture, out double dRcv);
                        double.TryParse(impApv, NumberStyles.Any, CultureInfo.InvariantCulture, out double dApv);
                        double.TryParse(impAcv, NumberStyles.Any, CultureInfo.InvariantCulture, out double dAcv);
                        
                        importeIMSS = (dImss + dRcv + dApv + dAcv);

                        resp.respcode = "00";
                        resp.referenciaNumerica = cReferenciaNumerica;
                        resp.referenciaXPay = qrText;
                        resp.vigencia = fechaVigenciaIMSS;
                        resp.monto = importeIMSS;
                        return Ok(resp);
                    }
                    else
                    {
                        _logic.GrabaLog($"Validacion local falló ({cRespcode}), pero se entrega referencia de DB desde archivo.", "warn");
                        resp.respcode = "00"; 
                        resp.referenciaNumerica = cReferenciaNumerica;
                        resp.referenciaXPay = qrText;
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
                _logic.GrabaLog(ex.ToString(), "err_critico_archivo");
                return StatusCode(500, new { code = "500", message = "Error Crítico .NET en PostArchivo", detail = ex.ToString() });
            }
        }
    }
}
