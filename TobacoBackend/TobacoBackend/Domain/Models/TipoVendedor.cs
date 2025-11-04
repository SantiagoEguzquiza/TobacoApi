namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Define los tipos de vendedores/distribuidores
    /// </summary>
    public enum TipoVendedor
    {
        /// <summary>
        /// Vendedor: visita sucursales, revisa inventario y asigna entregas a repartidores
        /// </summary>
        Vendedor = 0,
        
        /// <summary>
        /// Repartidor: solo realiza entregas, no genera ventas
        /// </summary>
        Repartidor = 1,
        
        /// <summary>
        /// Repartidor-Vendedor: visita sucursales, genera ventas y entrega en el mismo acto
        /// </summary>
        RepartidorVendedor = 2
    }
}

