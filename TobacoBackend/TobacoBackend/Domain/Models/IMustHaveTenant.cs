namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Interfaz que deben implementar todas las entidades que requieren filtrado por TenantId
    /// </summary>
    public interface IMustHaveTenant
    {
        int TenantId { get; set; }
    }
}

