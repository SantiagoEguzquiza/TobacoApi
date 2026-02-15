namespace TobacoBackend.Domain.IServices
{
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo de recuperación de contraseña con el enlace para restablecer.
        /// </summary>
        /// <param name="toEmail">Correo del usuario</param>
        /// <param name="userName">Nombre de usuario (para el texto del correo)</param>
        /// <param name="resetLink">URL con el token para restablecer contraseña</param>
        /// <returns>True si se envió correctamente</returns>
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
    }
}
