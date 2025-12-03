using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class PrecioEspecial : IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        // Navegación
        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; }

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; }

        // Fecha de creación para auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Fecha de última actualización
        public DateTime? FechaActualizacion { get; set; }

        /// <summary>
        /// ID del tenant (empresa/cliente) al que pertenece este precio especial
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Navegación al tenant al que pertenece este precio especial
        /// </summary>
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }
    }
}
