using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IAbonosService
    {
        Task<List<AbonoDTO>> GetAllAbonos();
        Task<AbonoDTO> GetAbonoById(int id);
        Task<AbonoDTO> AddAbono(AbonoDTO abonoDto);
        Task UpdateAbono(int id, AbonoDTO abonoDto);
        Task<bool> DeleteAbono(int id);
        Task<List<AbonoDTO>> GetAbonosByClienteId(int clienteId);
    }
}
