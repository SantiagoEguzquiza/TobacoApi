using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class Categoria
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
    }
}
