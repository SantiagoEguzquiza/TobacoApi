using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> GetAllClientes();
        Task<Cliente> GetClienteById(int id);
        Task<Cliente> AddCliente(Cliente cliente);
        Task UpdateCliente(Cliente cliente);
        Task<bool> DeleteCliente(int id);
        Task<IEnumerable<Cliente>> BuscarClientesAsync(string query);
        Task<List<Cliente>> GetClientesConDeuda();
        Task<(List<Cliente> Clientes, int TotalCount)> GetClientesPaginados(int page, int pageSize);
        Task<(List<Cliente> Clientes, int TotalCount)> GetClientesConDeudaPaginados(int page, int pageSize);
        Task<Cliente?> BuscarConsumidorFinal(int tenantId);
    }
}
