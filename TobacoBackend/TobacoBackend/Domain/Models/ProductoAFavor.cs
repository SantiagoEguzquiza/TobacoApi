using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Representa un producto que quedó pendiente de entrega y está a favor del cliente
    /// </summary>
    public class ProductoAFavor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; }

        [Required]
        public decimal Cantidad { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        public string Motivo { get; set; }

        public string? Nota { get; set; }

        // Referencia a la venta original donde se generó el faltante
        [Required]
        public int VentaId { get; set; }

        [ForeignKey("VentaId")]
        public Venta Venta { get; set; }

        // Referencia al VentaProducto específico
        public int VentaProductoId { get; set; }

        // Usuario que registró el faltante
        public int? UsuarioRegistroId { get; set; }

        [ForeignKey("UsuarioRegistroId")]
        public User? UsuarioRegistro { get; set; }

        // Estado del producto a favor
        public bool Entregado { get; set; } = false;

        // Fecha en que se entregó el producto (si aplica)
        public DateTime? FechaEntrega { get; set; }

        // Usuario que marcó como entregado
        public int? UsuarioEntregaId { get; set; }

        [ForeignKey("UsuarioEntregaId")]
        public User? UsuarioEntrega { get; set; }
    }
}

