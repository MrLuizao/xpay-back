using Microsoft.AspNetCore.Mvc;
using ReferenciaXPayAPI_Core.Models;
using ReferenciaXPayAPI_Core.Logic;
using Newtonsoft.Json;
using System.IO;

namespace ReferenciaXPayAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CamposIMSSController : ControllerBase
    {
        private readonly CamposIMSSLogic _camposLogic;
        private readonly ReferenciaLogic _referenciaLogic;

        public CamposIMSSController(CamposIMSSLogic camposLogic, ReferenciaLogic referenciaLogic)
        {
            _camposLogic = camposLogic;
            _referenciaLogic = referenciaLogic;
        }

        /// <summary>
        /// Procesa una referencia del IMSS y devuelve los campos decodificados con ticket generado
        /// </summary>
        /// <param name="model">Modelo con la referencia a procesar</param>
        /// <returns>Campos decodificados y ticket formateado</returns>
        [HttpPost]
        public IActionResult Post([FromBody] ReferenciaRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Referencia))
                {
                    _referenciaLogic.GrabaLog("Referencia es obligatoria", "err");
                    return BadRequest(new ApiResponse<ReferenciaResponse>
                    {
                        Code = "400",
                        Message = "Referencia es obligatoria"
                    });
                }

                var jsonReqMessage = JsonConvert.SerializeObject(model);
                _referenciaLogic.GrabaLog(jsonReqMessage, "json");

                var response = _camposLogic.ProcesarReferencia(model.Referencia);

                if (response.RespCode == "00")
                {
                    return Ok(new ApiResponse<ReferenciaResponse>
                    {
                        Code = "success",
                        Message = response.Message,
                        Data = response
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<ReferenciaResponse>
                    {
                        Code = response.RespCode,
                        Message = response.Message,
                        Data = response
                    });
                }
            }
            catch (Exception ex)
            {
                _referenciaLogic.GrabaLog(ex.ToString(), "err_critico");
                return StatusCode(500, new ApiResponse<string>
                {
                    Code = "500",
                    Message = "Error Crítico .NET",
                    Data = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Procesa una referencia del IMSS desde un archivo QR y devuelve los campos decodificados
        /// </summary>
        /// <param name="documento">Archivo PDF o imagen con código QR</param>
        /// <returns>Campos decodificados y ticket formateado</returns>
        [HttpPost("Archivo")]
        public IActionResult PostArchivo([FromForm] IFormFile documento)
        {
            try
            {
                if (documento == null || documento.Length == 0)
                {
                    _referenciaLogic.GrabaLog("Sin archivo adjunto", "err");
                    return BadRequest(new ApiResponse<string>
                    {
                        Code = "400",
                        Message = "El documento es obligatorio"
                    });
                }

                // Leer QR del archivo usando el servicio existente
                string qrText = null;
                var ext = Path.GetExtension(documento.FileName).ToLower();

                using (var stream = documento.OpenReadStream())
                {
                    // Necesitamos inyectar QRReaderService aquí
                    // Por ahora, simulamos la lectura del QR
                    if (ext == ".pdf" || ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    {
                        // Aquí iría la lógica de QRReaderService
                        // Por ahora devolvemos error para implementar después
                        return BadRequest(new ApiResponse<string>
                        {
                            Code = "501",
                            Message = "Funcionalidad de QR en desarrollo para este endpoint"
                        });
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<string>
                        {
                            Code = "400",
                            Message = "Formato de archivo no soportado. Sólo PDF, PNG y JPG."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _referenciaLogic.GrabaLog(ex.ToString(), "err_critico_archivo");
                return StatusCode(500, new ApiResponse<string>
                {
                    Code = "500",
                    Message = "Error Crítico .NET en PostArchivo",
                    Data = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Obtiene solo los campos decodificados sin generar ticket
        /// </summary>
        /// <param name="referencia">Referencia del IMSS a decodificar</param>
        /// <returns>Campos decodificados</returns>
        [HttpGet("{referencia}")]
        public IActionResult Get(string referencia)
        {
            try
            {
                if (string.IsNullOrEmpty(referencia))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Code = "400",
                        Message = "Referencia es obligatoria"
                    });
                }

                _referenciaLogic.GrabaLog($"GET referencia: {referencia}", "info");

                var response = _camposLogic.ProcesarReferencia(referencia);

                if (response.RespCode == "00")
                {
                    return Ok(new ApiResponse<CamposIMSSModel>
                    {
                        Code = "success",
                        Message = response.Message,
                        Data = response.Campos
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<CamposIMSSModel>
                    {
                        Code = response.RespCode,
                        Message = response.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _referenciaLogic.GrabaLog(ex.ToString(), "err_critico_get");
                return StatusCode(500, new ApiResponse<string>
                {
                    Code = "500",
                    Message = "Error Crítico .NET",
                    Data = ex.ToString()
                });
            }
        }
    }
}
