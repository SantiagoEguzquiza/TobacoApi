using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface ICompraRepository
    {
        Task<Compra> CreateAsync(Compra compra);
        Task<Compra?> GetByIdAsync(int id);
        Task<List<Compra>> GetAllByTenantAsync(DateTime? desde = null, DateTime? hasta = null);
        Task DeleteAsync(Compra compra);
    }
}
