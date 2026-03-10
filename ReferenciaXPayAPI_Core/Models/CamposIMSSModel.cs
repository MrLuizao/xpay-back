using System;

namespace ReferenciaXPayAPI_Core.Models
{
    public class CamposIMSSModel
    {
        public string Referencia { get; set; } = string.Empty;
        public string RegPat { get; set; } = string.Empty;
        public string PerPag { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string FSUA { get; set; } = string.Empty;
        public DateTime FechVenc { get; set; }
        public decimal ImpIMSS { get; set; }
        public decimal ImpRCV { get; set; }
        public decimal ImpAPV { get; set; }
        public decimal ImpACV { get; set; }
        public decimal ImpTotal { get; set; }
    }

    public class ReferenciaRequest
    {
        public string Referencia { get; set; } = string.Empty;
    }

    public class ReferenciaResponse
    {
        public CamposIMSSModel Campos { get; set; } = new CamposIMSSModel();
        public string Ticket { get; set; } = string.Empty;
        public string RespCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
