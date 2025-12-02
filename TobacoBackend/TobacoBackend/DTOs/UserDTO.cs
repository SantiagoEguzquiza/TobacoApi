using System.ComponentModel.DataAnnotations;
using TobacoBackend.Domain.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public TipoVendedor TipoVendedor { get; set; } = TipoVendedor.Repartidor;
        public string? Zona { get; set; }
        public PlanType Plan { get; set; } = PlanType.FREE;
    }

    public class LoginDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, guiones y guiones bajos")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDTO User { get; set; }
    }

    public class CreateUserDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, guiones y guiones bajos")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una minúscula y un número")]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [StringLength(20)]
        [RegularExpression(@"^(Admin|Employee)$", ErrorMessage = "El rol debe ser Admin o Employee")]
        public string Role { get; set; } = string.Empty;

        public TipoVendedor TipoVendedor { get; set; } = TipoVendedor.Repartidor;
        
        [StringLength(100)]
        public string? Zona { get; set; }

        /// <summary>
        /// Plan del usuario. Opcional en la creación - si no se especifica y el creador es admin, heredará el plan del admin.
        /// </summary>
        public PlanType? Plan { get; set; }
    }

    public class UpdateUserDTO
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, guiones y guiones bajos")]
        public string? UserName { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una minúscula y un número")]
        public string? Password { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string? Email { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^(Admin|Employee)$", ErrorMessage = "El rol debe ser Admin o Employee")]
        public string? Role { get; set; }

        public bool? IsActive { get; set; }

        public TipoVendedor? TipoVendedor { get; set; }
        
        [StringLength(100)]
        public string? Zona { get; set; }

        /// <summary>
        /// Plan del usuario. Solo puede ser modificado por desarrolladores.
        /// Si un admin cambia su plan, los sub-usuarios deberían actualizarse (lógica futura).
        /// </summary>
        public PlanType? Plan { get; set; }
    }
}
