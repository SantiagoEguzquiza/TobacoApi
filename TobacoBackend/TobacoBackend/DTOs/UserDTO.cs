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
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ExpiresIn { get; set; } // Tiempo de expiración en segundos
        public UserDTO User { get; set; }
    }

    public class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "El refresh token es requerido")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Solo Development: body opcional para restablecer contraseña SuperAdmin.
    /// </summary>
    public class ResetSuperAdminPasswordRequest
    {
        public string? NewPassword { get; set; }
    }

    /// <summary>
    /// Solicitud de recuperación de contraseña (no revela si el usuario existe).
    /// </summary>
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Restablecer contraseña con el token recibido por correo.
    /// </summary>
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una minúscula y un número")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RefreshTokenResponseDTO
    {
        public string AccessToken { get; set; }
        public string? RefreshToken { get; set; } // Nuevo refresh token (opcional, solo si se rota)
        public int ExpiresIn { get; set; } // Tiempo de expiración en segundos
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
