using Microsoft.AspNetCore.Authorization;

namespace TobacoBackend.Authorization
{
    /// <summary>
    /// Constantes para políticas de autorización
    /// </summary>
    public static class AuthorizationPolicies
    {
        // Roles principales
        public const string SuperAdminOnly = "SuperAdminOnly";
        public const string AdminOnly = "AdminOnly";
        public const string EmployeeOnly = "EmployeeOnly";
        public const string AdminOrEmployee = "AdminOrEmployee";
        public const string AdminOrEmployeeOnly = "AdminOrEmployeeOnly"; // Admin o Employee, NO SuperAdmin
        public const string SuperAdminOrAdmin = "SuperAdminOrAdmin";

        // Tipos de vendedor (para Employees)
        public const string VendedorOnly = "VendedorOnly";
        public const string RepartidorOnly = "RepartidorOnly";
        public const string RepartidorVendedorOnly = "RepartidorVendedorOnly";

        // Combinaciones
        public const string AdminOrVendedor = "AdminOrVendedor";
        public const string AdminOrRepartidor = "AdminOrRepartidor";
        public const string AdminOrRepartidorVendedor = "AdminOrRepartidorVendedor";
        public const string VendedorOrRepartidorVendedor = "VendedorOrRepartidorVendedor";
        
        // Ver ventas: Admin, Vendedor o RepartidorVendedor (NO Repartidor)
        public const string CanViewVentas = "CanViewVentas";
    }
}

