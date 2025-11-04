using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IRecorridoProgramadoService
    {
        Task<List<RecorridoProgramadoDTO>> GetRecorridosByVendedorAndDia(int vendedorId, int diaSemana);
        Task<List<RecorridoProgramadoDTO>> GetRecorridosByVendedor(int vendedorId);
        Task<RecorridoProgramadoDTO?> GetById(int id);
        Task<RecorridoProgramadoDTO> Create(CreateRecorridoProgramadoDTO dto);
        Task<RecorridoProgramadoDTO?> Update(int id, UpdateRecorridoProgramadoDTO dto);
        Task<bool> Delete(int id);
    }
}

