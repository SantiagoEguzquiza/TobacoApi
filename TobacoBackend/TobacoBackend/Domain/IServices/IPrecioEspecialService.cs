using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IPrecioEspecialService
    {
        Task<List<PrecioEspecialDTO>> GetAllPreciosEspecialesAsync();
        Task<PrecioEspecialDTO?> GetPrecioEspecialByIdAsync(int id);
        Task<List<PrecioEspecialDTO>> GetPreciosEspecialesByClienteIdAsync(int clienteId);
        Task<PrecioEspecialDTO?> GetPrecioEspecialByClienteAndProductoAsync(int clienteId, int productoId);
        Task<PrecioEspecialDTO> AddPrecioEspecialAsync(PrecioEspecialDTO precioEspecialDto);
        Task<PrecioEspecialDTO> UpdatePrecioEspecialAsync(PrecioEspecialDTO precioEspecialDto);
        Task<bool> DeletePrecioEspecialAsync(int id);
        Task<bool> DeletePrecioEspecialByClienteAndProductoAsync(int clienteId, int productoId);
        Task<bool> ExistsPrecioEspecialAsync(int clienteId, int productoId);
        Task<List<PrecioEspecialDTO>> GetPreciosEspecialesByProductoIdAsync(int productoId);
        
        // Métodos específicos para la lógica de negocio
        Task<decimal> GetPrecioFinalProductoAsync(int clienteId, int productoId);
        Task<bool> UpsertPrecioEspecialAsync(int clienteId, int productoId, decimal precio);
    }
}
