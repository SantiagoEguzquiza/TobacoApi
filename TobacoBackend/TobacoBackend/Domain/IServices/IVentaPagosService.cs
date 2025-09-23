using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IVentaPagosService
    {
        Task<List<VentaPagosDTO>> GetAllVentaPagos();
        Task<VentaPagosDTO> GetVentaPagosById(int id);
        Task<List<VentaPagosDTO>> GetVentaPagosByPedidoId(int pedidoId);
        Task<VentaPagosDTO> AddVentaPagos(VentaPagosDTO ventaPagosDto);
        Task<VentaPagosDTO> UpdateVentaPagos(VentaPagosDTO ventaPagosDto);
        Task<bool> DeleteVentaPagos(int id);
        Task<bool> DeleteVentaPagosByPedidoId(int pedidoId);
    }
}
