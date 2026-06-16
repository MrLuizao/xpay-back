using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ReferenciaXPayAPI.Models;
using static ReferenciaXPayAPI.Utilidades;
using static ReferenciaXPayAPI.Models.generaReferenciaNumericaModel;
using Newtonsoft.Json;

namespace ReferenciaXPayAPI.Controllers
{
    public class generarReferenciaNumericaController : ApiController
    {
        // POST api/<controller>
        public HttpResponseMessage Post([FromBody] generaReferenciaNumericaModel model)
        {
            generaReferenciaNumericaResponse resp = new generaReferenciaNumericaResponse();
            generarReferenciaNumerica oProceso = new generarReferenciaNumerica();

            try
            {
                if (model == null)
                {
                    GrabaLog("Sin definicion", "err");

                    resp.respcode = "14";
                    resp.referenciaNumerica = "";
                    resp.vigencia = null;
                    resp.monto = null;
                    return Request.CreateResponse(HttpStatusCode.NoContent, resp);
                }

                var jsonReqMessage = JsonConvert.SerializeObject(model);
                GrabaLog(jsonReqMessage, "json");

                string cReferencia = model.referencia;
                string cRespcode = String.Empty;
                string cReferenciaNumerica = String.Empty;

                // Calcular importe antes de llamar al SP
                decimal? importeCalculado = model.Importe;
                DateTime? fechaVigenciaIMSS = null;
                string RegPat = "";
                string PerPag = "";
                string Origen = "";
                string FSUA = "";
                string FechVenc = "";
                string ImpImss = "";
                string ImpRCV = "";
                string ImpAPV = "";
                string ImpACV = "";
                int mRet = 0;

                // Si no se proporcionó importe, calcularlo de la referencia
                if (!importeCalculado.HasValue)
                {
                    mRet = oProceso.ValidaReferencia(cReferencia, ref cRespcode);
                    if (mRet == 0)
                    {
                        oProceso.ObtenerCampos(cReferencia, ref RegPat, ref PerPag, ref Origen, ref FSUA, ref FechVenc, ref ImpImss, ref ImpRCV, ref ImpAPV, ref ImpACV);
                        GrabaLog(FechVenc, "Fecha: ");
                        GrabaLog(ImpImss, "Importe: ");
                        fechaVigenciaIMSS = DateTime.ParseExact(FechVenc, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        importeCalculado = Convert.ToDecimal(ImpImss);
                    }
                }

                int status = oProceso.generarBD(cReferencia, ref cRespcode, ref cReferenciaNumerica, importeCalculado);

                if (status == 0)
                {
                    double? importeIMSS = importeCalculado.HasValue ? (double)importeCalculado.Value : null;

                    resp.respcode = "00";
                    resp.referenciaNumerica = cReferenciaNumerica;
                    resp.vigencia = fechaVigenciaIMSS;
                    resp.monto = importeIMSS;
                }
                else
                {
                    resp.respcode = "14";
                    resp.referenciaNumerica = String.Empty;
                    resp.vigencia = null;
                    resp.monto = null;
                }
            }
            catch (Exception ex)
            {
                GrabaLog(ex.Message, "err");
                resp.respcode = "30";
                resp.referenciaNumerica = String.Empty;
                resp.vigencia = null;
                resp.monto = null;
                return Request.CreateResponse(HttpStatusCode.BadRequest, resp);
            }

            return Request.CreateResponse(HttpStatusCode.OK, resp);
        }
    }
}
