namespace TobacoBackend.DTOs
{
    /// <summary>
    /// DTO para obtener permisos de un empleado
    /// </summary>
    public class PermisosEmpleadoDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // Permisos de Productos
        public bool Productos_Visualizar { get; set; }
        public bool Productos_Crear { get; set; }
        public bool Productos_Editar { get; set; }
        public bool Productos_Eliminar { get; set; }

        // Permisos de Clientes
        public bool Clientes_Visualizar { get; set; }
        public bool Clientes_Crear { get; set; }
        public bool Clientes_Editar { get; set; }
        public bool Clientes_Eliminar { get; set; }

        // Permisos de Ventas
        public bool Ventas_Visualizar { get; set; }
        public bool Ventas_Crear { get; set; }
        public bool Ventas_EditarBorrador { get; set; }
        public bool Ventas_Eliminar { get; set; }

        // Permisos de Cuenta Corriente
        public bool CuentaCorriente_Visualizar { get; set; }
        public bool CuentaCorriente_RegistrarAbonos { get; set; }

        // Permisos de Entregas
        public bool Entregas_Visualizar { get; set; }
        public bool Entregas_ActualizarEstado { get; set; }
    }

    /// <summary>
    /// DTO para actualizar permisos de un empleado
    /// </summary>
    public class UpdatePermisosEmpleadoDTO
    {
        // Permisos de Productos
        public bool? Productos_Visualizar { get; set; }
        public bool? Productos_Crear { get; set; }
        public bool? Productos_Editar { get; set; }
        public bool? Productos_Eliminar { get; set; }

        // Permisos de Clientes
        public bool? Clientes_Visualizar { get; set; }
        public bool? Clientes_Crear { get; set; }
        public bool? Clientes_Editar { get; set; }
        public bool? Clientes_Eliminar { get; set; }

        // Permisos de Ventas
        public bool? Ventas_Visualizar { get; set; }
        public bool? Ventas_Crear { get; set; }
        public bool? Ventas_EditarBorrador { get; set; }
        public bool? Ventas_Eliminar { get; set; }

        // Permisos de Cuenta Corriente
        public bool? CuentaCorriente_Visualizar { get; set; }
        public bool? CuentaCorriente_RegistrarAbonos { get; set; }

        // Permisos de Entregas
        public bool? Entregas_Visualizar { get; set; }
        public bool? Entregas_ActualizarEstado { get; set; }
    }
}

