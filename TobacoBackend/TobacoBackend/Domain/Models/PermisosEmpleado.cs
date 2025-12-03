using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Modelo para gestionar permisos específicos de cada empleado
    /// </summary>
    public class PermisosEmpleado : IMustHaveTenant
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Permisos de Productos
        public bool Productos_Visualizar { get; set; } = false;
        public bool Productos_Crear { get; set; } = false;
        public bool Productos_Editar { get; set; } = false;
        public bool Productos_Eliminar { get; set; } = false;

        // Permisos de Clientes
        public bool Clientes_Visualizar { get; set; } = false;
        public bool Clientes_Crear { get; set; } = false;
        public bool Clientes_Editar { get; set; } = false;
        public bool Clientes_Eliminar { get; set; } = false;

        // Permisos de Ventas
        public bool Ventas_Visualizar { get; set; } = false;
        public bool Ventas_Crear { get; set; } = false;
        public bool Ventas_EditarBorrador { get; set; } = false;
        public bool Ventas_Eliminar { get; set; } = false;

        // Permisos de Cuenta Corriente
        public bool CuentaCorriente_Visualizar { get; set; } = false;
        public bool CuentaCorriente_RegistrarAbonos { get; set; } = false;

        // Permisos de Entregas
        public bool Entregas_Visualizar { get; set; } = false;
        public bool Entregas_ActualizarEstado { get; set; } = false;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// ID del tenant (empresa/cliente) al que pertenecen estos permisos
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Navegación al tenant al que pertenecen estos permisos
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }
    }
}

