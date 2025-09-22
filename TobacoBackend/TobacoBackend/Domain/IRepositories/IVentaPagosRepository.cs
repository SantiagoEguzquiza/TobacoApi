using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IVentaPagosRepository
    {
        Task<List<VentaPagos>> GetAllVentaPagos();
        Task<VentaPagos> GetVentaPagosById(int id);
        Task<List<VentaPagos>> GetVentaPagosByPedidoId(int pedidoId);
        Task<VentaPagos> AddVentaPagos(VentaPagos ventaPagos);
        Task<VentaPagos> UpdateVentaPagos(VentaPagos ventaPagos);
        Task<bool> DeleteVentaPagos(int id);
        Task<bool> DeleteVentaPagosByPedidoId(int pedidoId);
    }
}
