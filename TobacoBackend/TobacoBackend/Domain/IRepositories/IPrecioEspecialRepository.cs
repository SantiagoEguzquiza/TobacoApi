using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IPrecioEspecialRepository
    {
        Task<List<PrecioEspecial>> GetAllPreciosEspecialesAsync();
        Task<PrecioEspecial?> GetPrecioEspecialByIdAsync(int id);
        Task<List<PrecioEspecial>> GetPreciosEspecialesByClienteIdAsync(int clienteId);
        Task<PrecioEspecial?> GetPrecioEspecialByClienteAndProductoAsync(int clienteId, int productoId);
        Task<PrecioEspecial> AddPrecioEspecialAsync(PrecioEspecial precioEspecial);
        Task<PrecioEspecial> UpdatePrecioEspecialAsync(PrecioEspecial precioEspecial);
        Task<bool> DeletePrecioEspecialAsync(int id);
        Task<bool> DeletePrecioEspecialByClienteAndProductoAsync(int clienteId, int productoId);
        Task<bool> ExistsPrecioEspecialAsync(int clienteId, int productoId);
        Task<List<PrecioEspecial>> GetPreciosEspecialesByProductoIdAsync(int productoId);
    }
}
