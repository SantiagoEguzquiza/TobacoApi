using TobacoBackend.Domain.Models;

namespace TobacoBackend.DTOs
{
    public class VentaPagoDTO
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public MetodoPagoEnum Metodo { get; set; }
        public decimal Monto { get; set; }
    }
}

