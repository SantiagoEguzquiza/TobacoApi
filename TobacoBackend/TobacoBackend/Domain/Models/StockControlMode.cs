namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Define si un producto hereda la configuración global de stock
    /// del tenant o fuerza su propio comportamiento.
    /// </summary>
    public enum StockControlMode
    {
        InheritTenant = 0,
        ForceEnabled = 1,
        ForceDisabled = 2
    }
}
