namespace TobacoBackend.DTOs
{
    public class ProductoAFavorDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public ClienteDTO? Cliente { get; set; }
        public int ProductoId { get; set; }
        public ProductoDTO? Producto { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Motivo { get; set; }
        public string? Nota { get; set; }
        public int VentaId { get; set; }
        public int VentaProductoId { get; set; }
        public int? UsuarioRegistroId { get; set; }
        public UserDTO? UsuarioRegistro { get; set; }
        public bool Entregado { get; set; } = false;
        public DateTime? FechaEntrega { get; set; }
        public int? UsuarioEntregaId { get; set; }
        public UserDTO? UsuarioEntrega { get; set; }
    }
}

