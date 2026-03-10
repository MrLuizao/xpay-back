namespace ReferenciaXPayAPI_Core.Models
{
    public class HistorialRecibosModel
    {
        public long Id { get; set; }
        public string Servicio { get; set; } = string.Empty;
        public string ReferenciaNumerica { get; set; } = string.Empty;
        public double Importe { get; set; }
        public string Vigencia { get; set; } = string.Empty;
        public string Estatus { get; set; } = string.Empty;
        public string ReferenciaXPay { get; set; } = string.Empty;
    }
}
