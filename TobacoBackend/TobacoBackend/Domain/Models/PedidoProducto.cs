using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class PedidoProducto
    {
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        public decimal Cantidad { get; set; }
        
        // Precio final calculado después de aplicar todos los descuentos
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioFinalCalculado { get; set; }
    }
}
