using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class Compra : IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProveedorId { get; set; }

        [ForeignKey("ProveedorId")]
        public Proveedor Proveedor { get; set; } = null!;

        [Required]
        public DateTime Fecha { get; set; }

        [StringLength(100)]
        public string? NumeroComprobante { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        public int TenantId { get; set; }

        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public List<CompraItem> Items { get; set; } = new List<CompraItem>();
    }
}
