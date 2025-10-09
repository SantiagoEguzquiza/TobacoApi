using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IVentaPagoRepository
    {
        Task<List<VentaPago>> GetAllVentaPagos();
        Task<VentaPago> GetVentaPagoById(int id);
        Task<List<VentaPago>> GetVentaPagosByVentaId(int ventaId);
        Task<VentaPago> AddVentaPago(VentaPago ventaPago);
        Task<VentaPago> UpdateVentaPago(VentaPago ventaPago);
        Task<bool> DeleteVentaPago(int id);
        Task<bool> DeleteVentaPagosByVentaId(int ventaId);
    }
}

