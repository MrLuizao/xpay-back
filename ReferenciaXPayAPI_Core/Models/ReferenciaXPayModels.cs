using System;

namespace ReferenciaXPayAPI_Core.Models
{
    public class GeneraReferenciaNumericaModel
    {
        public string Referencia { get; set; } = string.Empty;
        public string? UsuarioXPayId { get; set; }
    }

    public class GeneraReferenciaNumericaResponse
    {
        public string respcode { get; set; } = string.Empty;
        public string referenciaNumerica { get; set; } = string.Empty;
        public string? referenciaXPay { get; set; }
        public DateTime? vigencia { get; set; }
        public double? monto { get; set; }
    }

    public class UsuarioXPay
    {
        // Based on the original project structure, though not fully used in the controller seen
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UsuarioRegistroModel
    {
        public string UserId { get; set; } = string.Empty; // Mandatory
        public string Celular { get; set; } = string.Empty; // Mandatory
        public int RolXPayId { get; set; } // Mandatory
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class UsuarioModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public int RolXPayId { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public int? UsuarioXPayId { get; set; }
    }

    public class UsuarioUpdateModel
    {
        public string UserId { get; set; } = string.Empty; // Mandatory
        public int? RolXPayId { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Celular { get; set; }
        public string? Password { get; set; }
    }

    public class LoginRequestModel
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty; // e.g. "201", "200", "400"
        public string Message { get; set; } = string.Empty; // e.g. "Success", "Created"
        public T? Data { get; set; }
    }

    public class RegistrarPagoModel
    {
        public string NumeroTransaccion { get; set; } = string.Empty;
        public double Importe { get; set; }
        public string? UsuarioXPayId { get; set; }
    }

    public class RegistrarPagoData
    {
        public string ReferenciaNumerica { get; set; } = string.Empty;
        public string Estatus { get; set; } = string.Empty; // "Pagada"
    }
}
