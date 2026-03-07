using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class CompraItem : IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CompraId { get; set; }

        [ForeignKey("CompraId")]
        public Compra Compra { get; set; } = null!;

        [Required]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; } = null!;

        [Required]
        public decimal Cantidad { get; set; }

        [Required]
        public decimal CostoUnitario { get; set; }

        [Required]
        public decimal Subtotal { get; set; }

        [Required]
        public int TenantId { get; set; }

        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;
    }
}
