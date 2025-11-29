namespace TobacoBackend.Domain.Models
{
    /// <summary>
    /// Define los tipos de planes disponibles para los usuarios
    /// </summary>
    public enum PlanType
    {
        /// <summary>
        /// Plan gratuito con funcionalidades limitadas
        /// </summary>
        FREE = 0,
        
        /// <summary>
        /// Plan básico con funcionalidades estándar
        /// </summary>
        BASIC = 1,
        
        /// <summary>
        /// Plan premium con todas las funcionalidades
        /// </summary>
        PREMIUM = 2
    }
}

