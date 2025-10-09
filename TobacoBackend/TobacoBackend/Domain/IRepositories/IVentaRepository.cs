using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IVentaRepository
    {
        Task<List<Venta>> GetAllVentas();
        Task<Venta> GetVentaById(int id);
        Task AddVenta(Venta venta);
        Task AddVentaProducto(VentaProducto ventaProducto);
        Task AddOrUpdateVentaProducto(VentaProducto ventaProducto);
        Task UpdateVenta(Venta venta);
        Task<bool> DeleteVenta(int id);
        Task<VentaPaginationResult> GetVentasPaginadas(int page, int pageSize);
        Task<VentaPaginationResult> GetVentasPorCliente(int clienteId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<VentaPaginationResult> GetVentasConCuentaCorrienteByClienteId(int clienteId, int page, int pageSize);
    }
}

