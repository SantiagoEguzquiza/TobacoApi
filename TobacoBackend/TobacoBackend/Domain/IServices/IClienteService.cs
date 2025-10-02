using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IClienteService
    {
        Task<List<ClienteDTO>> GetAllClientes();
        Task<ClienteDTO> GetClienteById(int id);
        Task<ClienteDTO> AddCliente(ClienteDTO cliente);
        Task UpdateCliente(int id, ClienteDTO cliente);
        Task<bool> DeleteCliente(int id);
        Task<IEnumerable<ClienteDTO>> BuscarClientesAsync(string query);
        Task<List<ClienteDTO>> GetClientesConDeuda();
        Task<object> GetClientesPaginados(int page, int pageSize);
        Task<object> GetClientesConDeudaPaginados(int page, int pageSize);
        
        // Métodos para manejo de deuda
        Task AgregarDeuda(int clienteId, decimal monto);
        Task ReducirDeuda(int clienteId, decimal monto);
        Task<bool> ValidarMontoAbono(int clienteId, decimal montoAbono);
        Task<object> GetDetalleDeuda(int clienteId);
    }
}
