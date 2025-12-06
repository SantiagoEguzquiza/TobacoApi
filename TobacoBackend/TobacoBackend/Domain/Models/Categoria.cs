using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class Categoria : IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        [StringLength(7)]
        public string ColorHex { get; set; } = "#9E9E9E"; // Default gray color

        public int SortOrder { get; set; } = 0; // Default sort order

        public List<Producto> Productos { get; set; } = new List<Producto>();

        /// <summary>
        /// ID del tenant (empresa/cliente) al que pertenece esta categoría
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Navegación al tenant al que pertenece esta categoría
        /// </summary>
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }
    }
}
