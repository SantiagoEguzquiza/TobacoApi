using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Employee"; // Admin or Employee

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Tipo de vendedor/distribuidor. Solo aplica para empleados (Role = "Employee")
        /// Por defecto es Repartidor
        /// </summary>
        public TipoVendedor TipoVendedor { get; set; } = TipoVendedor.Repartidor;

        /// <summary>
        /// Zona asignada al repartidor/vendedor
        /// </summary>
        [StringLength(100)]
        public string? Zona { get; set; }

        /// <summary>
        /// Plan contratado por el usuario. Los sub-usuarios heredan el plan de su admin.
        /// </summary>
        public PlanType Plan { get; set; } = PlanType.FREE;

        /// <summary>
        /// ID del usuario administrador que creó este usuario (si es un sub-usuario).
        /// Null si el usuario fue creado directamente por un desarrollador.
        /// </summary>
        public int? CreatedById { get; set; }

        /// <summary>
        /// Navegación al usuario administrador que creó este usuario
        /// </summary>
        [ForeignKey("CreatedById")]
        public User? CreatedBy { get; set; }
    }
}
