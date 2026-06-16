using System;
using System.Text;
using System.Globalization;
using ReferenciaXPayAPI_Core.Models;

namespace ReferenciaXPayAPI_Core.Logic
{
    public class CamposIMSSLogic
    {
        private readonly ReferenciaLogic _referenciaLogic;

        public CamposIMSSLogic(ReferenciaLogic referenciaLogic)
        {
            _referenciaLogic = referenciaLogic;
        }

        public ReferenciaResponse ProcesarReferencia(string referencia, int? comercioId = null, int? sucursalId = null)
        {
            var response = new ReferenciaResponse
            {
                RespCode = "00",
                Message = "Procesamiento exitoso"
            };

            try
            {
                // Validar referencia
                string respCode = "";
                int validationResult = _referenciaLogic.ValidaReferencia(referencia, ref respCode);
                
                if (validationResult != 0)
                {
                    response.RespCode = respCode;
                    response.Message = "Referencia inválida";
                    return response;
                }

                // Obtener campos
                var campos = ObtenerCampos(referencia);
                response.Campos = campos;

                // Obtener encabezado de ticket si se proporcionaron comercioId y sucursalId
                if (comercioId.HasValue && sucursalId.HasValue)
                {
                    response.Encabezado = _referenciaLogic.ObtenerEncabezadoTicket(comercioId.Value, sucursalId.Value);
                }

                // Generar ticket
                response.Ticket = GenerarTicket(campos);

                return response;
            }
            catch (Exception ex)
            {
                response.RespCode = "99";
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        private CamposIMSSModel ObtenerCampos(string referencia)
        {
            var campos = new CamposIMSSModel
            {
                Referencia = referencia,
                RegPat = referencia.Substring(0, 1) + Base36aBase10(referencia.Substring(1, 7)).ToString(),
                PerPag = Base36aBase10(referencia.Substring(8, 4)).ToString(),
                Origen = Base36aBase10(referencia.Substring(12, 1)).ToString(),
                FSUA = Base36aBase10(referencia.Substring(13, 4)).ToString(),
                FechVenc = DateTime.ParseExact(
                    FechaVenc(referencia.Substring(17, 4)),
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture
                ),
                ImpIMSS = Convert.ToDecimal(Base36aBase10(referencia.Substring(21, 7))) / 100m,
                ImpRCV = Convert.ToDecimal(Base36aBase10(referencia.Substring(28, 7))) / 100m,
                ImpAPV = Convert.ToDecimal(Base36aBase10(referencia.Substring(35, 7))) / 100m,
                ImpACV = Convert.ToDecimal(Base36aBase10(referencia.Substring(42, 7))) / 100m
            };

            campos.ImpTotal = campos.ImpIMSS + campos.ImpRCV + campos.ImpAPV + campos.ImpACV;

            return campos;
        }

        private static int CodifBase36(char car)
        {
            const string guarismosB36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return guarismosB36.IndexOf(car);
        }

        public static long Base36aBase10(string base36)
        {
            long valor = 0;

            for (int i = 0; i < base36.Length; i++)
            {
                char car = base36[i];
                valor = valor * 36 + CodifBase36(car);
            }

            return valor;
        }

        private static string FechaVenc(string fechaCodificada)
        {
            int año = 0;
            int mes = 0;
            int dia = 0;

            int fechaEntero = int.Parse(fechaCodificada);

            año = 2009 + (fechaEntero / 372);
            fechaEntero = fechaEntero % 372;

            mes = (fechaEntero / 31) + 1;
            dia = (fechaEntero % 31) + 1;

            return año.ToString() + mes.ToString("00") + dia.ToString("00");
        }

        private static string GenerarTicket(CamposIMSSModel campos)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("COMPROBANTE DE PAGO DE CUOTAS, APORTA-");
            sb.AppendLine("CIONES Y AMORTIZACIONES DE CREDITO");
            sb.AppendLine("SIPARE IMSS-INFONAVIT");
            sb.AppendLine();

            sb.AppendLine("Linea de captura " + campos.Referencia.Substring(0, 23));
            sb.AppendLine(campos.Referencia.Substring(23));
            sb.AppendLine("REGISTRO PATRONAL          " + campos.RegPat);
            sb.AppendLine("PERIODO DE PAGO            " + campos.PerPag);
            sb.AppendLine("FOLIO SUA                  " + campos.FSUA);
            sb.AppendLine("FECHA DE VENCIMIENTO       " + campos.FechVenc.ToString("dd/MM/yyyy"));
            sb.AppendLine("ORIGEN LINEA DE CAPTURA    " + campos.Origen);
            sb.AppendLine();

            string fIMSS = campos.ImpIMSS.ToString("###,##0.00");
            string fRCV = campos.ImpRCV.ToString("###,##0.00");
            string fAPV = campos.ImpAPV.ToString("###,##0.00");
            string fACV = campos.ImpACV.ToString("###,##0.00");
            string fTOT = campos.ImpTotal.ToString("###,##0.00");

            sb.AppendLine("IMPORTE IMSS" + new string(' ', 26 - fIMSS.Length) + fIMSS);
            sb.AppendLine("IMPORTE RCV" + new string(' ', 27 - fRCV.Length) + fRCV);
            sb.AppendLine("IMPORTE VIVIENDA" + new string(' ', 22 - fAPV.Length) + fAPV);
            sb.AppendLine("IMPORTE ACV" + new string(' ', 27 - fACV.Length) + fACV);
            sb.AppendLine("IMPORTE TOTAL MXN" + new string(' ', 21 - fTOT.Length) + fTOT);
            sb.AppendLine();

            sb.AppendLine("FECHA DE APLICACION        " + DateTime.Now.ToString("dd/MM/yyyy"));
            sb.AppendLine("HORA DE APLICACION         " + DateTime.Now.ToString("HH:mm:ss"));
            sb.AppendLine("PAGO REALIZADO EN EFECTIVO");
            sb.AppendLine("OPERADO POR XCD DESARROLLADORA SA");
            sb.AppendLine("FOLIO DE TRANSACCION 7122883");
            sb.AppendLine();
            sb.AppendLine("FAVOR DE GUARDAR ESTE COMPROBANTE");
            sb.AppendLine("PARA ACLARACIONES MARQUE 81-2474-6901");
            sb.AppendLine();
            sb.AppendLine("ESTE COMPROBANTE DE PAGO ES VALIDO ANTE");
            sb.AppendLine("EL IMSS E INFONAVIT");

            return sb.ToString();
        }
    }
}
