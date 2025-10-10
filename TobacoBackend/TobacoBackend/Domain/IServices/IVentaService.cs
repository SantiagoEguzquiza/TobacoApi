using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IVentaService
    {
        Task<List<VentaDTO>> GetAllVentas();
        Task<VentaDTO> GetVentaById(int id);
        Task AddVenta(VentaDTO ventaDto);
        Task UpdateVenta(int id, VentaDTO ventaDto);
        Task<bool> DeleteVenta(int id);
        Task<object> GetVentasPaginadas(int page, int pageSize);
        Task<object> GetVentasPorCliente(int clienteId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<object> GetVentasConCuentaCorrienteByClienteId(int clienteId, int page = 1, int pageSize = 20);
        Task UpdateEstadoEntregaItems(int ventaId, List<VentaProductoDTO> items);
    }
}

