using System.ComponentModel.DataAnnotations;
using TobacoBackend.Domain.Models;

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
    }

    public class LoginDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDTO User { get; set; }
    }

    public class CreateUserDTO
    {
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
        public string Role { get; set; }

        public TipoVendedor TipoVendedor { get; set; } = TipoVendedor.Repartidor;
        
        [StringLength(100)]
        public string? Zona { get; set; }
    }

    public class UpdateUserDTO
    {
        [StringLength(50)]
        public string? UserName { get; set; }

        [StringLength(255)]
        public string? Password { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Role { get; set; }

        public bool? IsActive { get; set; }

        public TipoVendedor? TipoVendedor { get; set; }
        
        [StringLength(100)]
        public string? Zona { get; set; }
    }
}
