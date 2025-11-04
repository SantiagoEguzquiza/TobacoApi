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
    }
}
