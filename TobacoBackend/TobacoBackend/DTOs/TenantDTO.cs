using System.ComponentModel.DataAnnotations;

namespace TobacoBackend.DTOs
{
    public class TenantDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? Telefono { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTenantDTO
    {
        [Required(ErrorMessage = "El nombre del tenant es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
        public string? Descripcion { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        public string? Telefono { get; set; }
    }

    public class UpdateTenantDTO
    {
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string? Nombre { get; set; }

        [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
        public string? Descripcion { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        public string? Telefono { get; set; }

        public bool? IsActive { get; set; }
    }
}

