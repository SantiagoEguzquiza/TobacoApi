using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Representa un recorrido programado para un vendedor-repartidor
    /// Define qué clientes debe visitar cada día de la semana
    /// </summary>
    public class RecorridoProgramado : IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int VendedorId { get; set; }

        [ForeignKey("VendedorId")]
        public User Vendedor { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; }

        [Required]
        public DiaSemana DiaSemana { get; set; }

        /// <summary>
        /// Orden de visita en el recorrido del día (para optimizar ruta)
        /// </summary>
        public int Orden { get; set; } = 0;

        /// <summary>
        /// Indica si el recorrido está activo
        /// </summary>
        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID del tenant (empresa/cliente) al que pertenece este recorrido programado
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Navegación al tenant al que pertenece este recorrido programado
        /// </summary>
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }
    }
}

