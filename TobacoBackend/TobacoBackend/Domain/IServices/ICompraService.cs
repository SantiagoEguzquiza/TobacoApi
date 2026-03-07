using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface ICompraService
    {
        Task<CompraDTO> CreateAsync(CreateCompraDTO dto);
        Task<CompraDTO?> GetByIdAsync(int id);
        Task<List<CompraDTO>> GetAllAsync(DateTime? desde = null, DateTime? hasta = null);
    }
}
