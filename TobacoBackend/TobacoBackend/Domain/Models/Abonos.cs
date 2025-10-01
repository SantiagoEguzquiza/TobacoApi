using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class Abonos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        [Required]
        public string Monto { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public string Nota { get; set; }
    }
}
