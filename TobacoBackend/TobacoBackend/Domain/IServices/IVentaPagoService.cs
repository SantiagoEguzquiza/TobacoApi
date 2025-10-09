using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IVentaPagoService
    {
        Task<List<VentaPagoDTO>> GetAllVentaPagos();
        Task<VentaPagoDTO> GetVentaPagoById(int id);
        Task<List<VentaPagoDTO>> GetVentaPagosByVentaId(int ventaId);
        Task<VentaPagoDTO> AddVentaPago(VentaPagoDTO ventaPagoDto);
        Task<VentaPagoDTO> UpdateVentaPago(VentaPagoDTO ventaPagoDto);
        Task<bool> DeleteVentaPago(int id);
        Task<bool> DeleteVentaPagosByVentaId(int ventaId);
    }
}

