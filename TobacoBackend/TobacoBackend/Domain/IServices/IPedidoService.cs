using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IPedidoService
    {
        Task<List<PedidoDTO>> GetAllPedidos();
        Task<PedidoDTO> GetPedidoById(int id);
        Task AddPedido(PedidoDTO pedidoDto);
        Task UpdatePedido(int id, PedidoDTO pedidoDto);
        Task<bool> DeletePedido(int id);
        Task<object> GetPedidosPaginados(int page, int pageSize);
        Task<object> GetPedidosPorCliente(int clienteId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<object> GetPedidosConCuentaCorrienteByClienteId(int clienteId, int page = 1, int pageSize = 20);
    }
}