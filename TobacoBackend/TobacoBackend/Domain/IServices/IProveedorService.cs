using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IProveedorService
    {
        Task<List<ProveedorDTO>> GetAllAsync();
        Task<ProveedorDTO> CreateAsync(CreateProveedorDTO dto);
    }
}
