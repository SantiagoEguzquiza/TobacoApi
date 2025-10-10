namespace TobacoBackend.DTOs
{
    public class VentaProductoDTO
    {
        public int ProductoId { get; set; }
        public ProductoDTO Producto { get; set; } 
        public decimal Cantidad { get; set; }
        public decimal PrecioFinalCalculado { get; set; }
        public bool Entregado { get; set; } = false;
        public string? Motivo { get; set; }
        public string? Nota { get; set; }
        public DateTime? FechaChequeo { get; set; }
        public int? UsuarioChequeoId { get; set; }
        public UserDTO? UsuarioChequeo { get; set; }
    }
}

