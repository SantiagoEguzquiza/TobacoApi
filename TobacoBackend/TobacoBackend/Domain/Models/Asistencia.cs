using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class Asistencia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public DateTime FechaHoraEntrada { get; set; }

        public DateTime? FechaHoraSalida { get; set; }

        [StringLength(200)]
        public string? UbicacionEntrada { get; set; }

        [StringLength(200)]
        public string? UbicacionSalida { get; set; }

        [StringLength(50)]
        public string? LatitudEntrada { get; set; }

        [StringLength(50)]
        public string? LongitudEntrada { get; set; }

        [StringLength(50)]
        public string? LatitudSalida { get; set; }

        [StringLength(50)]
        public string? LongitudSalida { get; set; }

        // Campo calculado para obtener las horas trabajadas
        [NotMapped]
        public TimeSpan? HorasTrabajadas
        {
            get
            {
                if (FechaHoraSalida.HasValue)
                {
                    return FechaHoraSalida.Value - FechaHoraEntrada;
                }
                return null;
            }
        }
    }
}

