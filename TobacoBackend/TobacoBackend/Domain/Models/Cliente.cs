using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Direccion { get; set; }

        [Required]
        public string Telefono { get; set; }

        public string Deuda { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DescuentoGlobal { get; set; } = 0;

        public List<Pedido> Pedidos { get; set; } = new List<Pedido>();

        public List<PrecioEspecial> PreciosEspeciales { get; set; } = new List<PrecioEspecial>();

        public List<Abonos> Abonos { get; set; } = new List<Abonos>();

        [NotMapped]
        public decimal DeudaDecimal => decimal.TryParse(Deuda, out var value) ? value : 0;
    }
}
