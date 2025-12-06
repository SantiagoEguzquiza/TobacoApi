using System.ComponentModel.DataAnnotations;

namespace TobacoBackend.DTOs
{
    /// <summary>
    /// DTO para el proceso de onboarding de un nuevo cliente/tenant
    /// Crea el tenant y su primer usuario administrador
    /// </summary>
    public class OnboardingRequestDTO
    {
        // Datos del Tenant
        [Required(ErrorMessage = "El nombre de la empresa es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string EmpresaNombre { get; set; }

        [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
        public string? EmpresaDescripcion { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? EmpresaEmail { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        public string? EmpresaTelefono { get; set; }

        // Datos del Usuario Admin
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
        public string AdminUserName { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string AdminPassword { get; set; }

        [EmailAddress(ErrorMessage = "El email del administrador no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? AdminEmail { get; set; }
    }

    public class OnboardingResponseDTO
    {
        public TenantDTO Tenant { get; set; }
        public UserDTO AdminUser { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}

