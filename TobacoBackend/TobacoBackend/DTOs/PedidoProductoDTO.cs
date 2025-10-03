namespace TobacoBackend.DTOs
{
    public class PedidoProductoDTO
    {
        public int ProductoId { get; set; }
        public ProductoDTO Producto { get; set; } 
        public decimal Cantidad { get; set; }
        public decimal PrecioFinalCalculado { get; set; }
    }
}
