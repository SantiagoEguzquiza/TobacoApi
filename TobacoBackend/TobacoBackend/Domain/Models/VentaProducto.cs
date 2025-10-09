using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class VentaProducto
    {
        public int VentaId { get; set; }
        public Venta Venta { get; set; }

        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        public decimal Cantidad { get; set; }
        
        // Precio final calculado después de aplicar todos los descuentos
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioFinalCalculado { get; set; }
    }
}

