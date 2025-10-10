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
        
        // Indica si este item fue entregado
        public bool Entregado { get; set; } = false;

        // Motivo cuando no se entrega (obligatorio si Entregado = false)
        public string? Motivo { get; set; }

        // Nota opcional sobre la entrega
        public string? Nota { get; set; }

        // Auditoría del chequeo
        public DateTime? FechaChequeo { get; set; }

        public int? UsuarioChequeoId { get; set; }

        [ForeignKey("UsuarioChequeoId")]
        public User? UsuarioChequeo { get; set; }
    }
}

