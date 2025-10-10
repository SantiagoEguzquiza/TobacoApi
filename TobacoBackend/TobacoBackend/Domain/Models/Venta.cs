using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class Venta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; }

        public List<VentaProducto> VentaProductos { get; set; } = new List<VentaProducto>();
        public List<VentaPago> VentaPagos { get; set; } = new List<VentaPago>();

        [Required]
        public decimal Total { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public MetodoPagoEnum MetodoPago { get; set; }

        // Usuario que creó la venta
        public int? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public User? Usuario { get; set; }
        
        // Estado de entrega de la venta
        [Required]
        public EstadoEntrega EstadoEntrega { get; set; } = EstadoEntrega.NO_ENTREGADA;
    }
}

