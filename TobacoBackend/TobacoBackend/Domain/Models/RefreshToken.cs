using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// ID del tenant (empresa/cliente) al que pertenece este refresh token
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Navegación al tenant al que pertenece este refresh token
        /// </summary>
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }
    }
}
