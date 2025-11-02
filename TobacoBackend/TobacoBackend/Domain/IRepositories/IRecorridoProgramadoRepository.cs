using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IRecorridoProgramadoRepository
    {
        Task<List<RecorridoProgramado>> GetRecorridosByVendedorAndDia(int vendedorId, DiaSemana diaSemana);
        Task<List<RecorridoProgramado>> GetRecorridosByVendedor(int vendedorId);
        Task<RecorridoProgramado?> GetById(int id);
        Task Add(RecorridoProgramado recorrido);
        Task Update(RecorridoProgramado recorrido);
        Task Delete(int id);
        Task SaveChangesAsync();
    }
}

