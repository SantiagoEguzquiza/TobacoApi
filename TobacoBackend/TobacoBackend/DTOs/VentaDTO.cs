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
        public int? UsuarioIdCreador { get; set; }
        public UserDTO? UsuarioCreador { get; set; }
        public int? UsuarioIdAsignado { get; set; }
        public UserDTO? UsuarioAsignado { get; set; }
        public EstadoEntrega EstadoEntrega { get; set; } = EstadoEntrega.NO_ENTREGADA;
    }

    public class AsignarVentaDTO
    {
        public int VentaId { get; set; }
        public int UsuarioId { get; set; }
    }

    public class AsignarVentaAutomaticaResponseDTO
    {
        public bool Asignada { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

