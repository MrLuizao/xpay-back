using System;

namespace ReferenciaXPayAPI_Core.Models
{
    public class GeneraReferenciaNumericaModel
    {
        public string Referencia { get; set; } = string.Empty;
    }

    public class GeneraReferenciaNumericaResponse
    {
        public string Respcode { get; set; } = string.Empty;
        public string ReferenciaNumerica { get; set; } = string.Empty;
        public DateTime? Vigencia { get; set; }
        public double? Monto { get; set; }
    }

    public class UsuarioXPay
    {
        // Based on the original project structure, though not fully used in the controller seen
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
