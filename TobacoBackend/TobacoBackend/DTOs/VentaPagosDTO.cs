using TobacoBackend.Domain.Models;

namespace TobacoBackend.DTOs
{
    public class VentaPagosDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public MetodoPagoEnum Metodo { get; set; }
        public decimal Monto { get; set; }
    }
}
