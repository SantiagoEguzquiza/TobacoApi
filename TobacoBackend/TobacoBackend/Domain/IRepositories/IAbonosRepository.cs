using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IAbonosRepository
    {
        Task<List<Abonos>> GetAllAbonos();
        Task<Abonos> GetAbonoById(int id);
        Task<Abonos> AddAbono(Abonos abono);
        Task UpdateAbono(Abonos abono);
        Task<bool> DeleteAbono(int id);
        Task<List<Abonos>> GetAbonosByClienteId(int clienteId);
    }
}
