using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class Producto : IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public decimal Stock { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }

        public List<VentaProducto> VentaProductos { get; set; } = new List<VentaProducto>();
        public List<ProductQuantityPrice> QuantityPrices { get; set; } = new List<ProductQuantityPrice>();

        public string? Marca { get; set; }

        public decimal Descuento { get; set; } = 0;

        public DateTime? fechaExpiracionDescuento { get; set; }

        public bool descuentoIndefinido { get; set; } = false;

        public bool Half { get; set; } = false;

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ID del tenant (empresa/cliente) al que pertenece este producto
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Navegación al tenant al que pertenece este producto
        /// </summary>
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }
    }
}
