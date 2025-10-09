using TobacoBackend.Domain.Models;

namespace TobacoBackend.DTOs
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public ClienteDTO Cliente { get; set; } 
        public List<VentaProductoDTO> VentaProductos { get; set; }
        public List<VentaPagoDTO> VentaPagos { get; set; }
        public decimal Total { get; set; }
        public DateTime Fecha { get; set; }
        public MetodoPagoEnum MetodoPago { get; set; }
        public int? UsuarioId { get; set; }
        public UserDTO? Usuario { get; set; }
 
    }
}

