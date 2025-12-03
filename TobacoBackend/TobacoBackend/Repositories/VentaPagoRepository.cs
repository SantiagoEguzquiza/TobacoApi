using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class VentaPagoRepository : IVentaPagoRepository
    {
        private readonly AplicationDbContext _context;

        public VentaPagoRepository(AplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Filtra VentaPago por TenantId a través de la relación con Venta
        /// </summary>
        private IQueryable<VentaPago> FilterByTenant(IQueryable<VentaPago> query)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue)
            {
                return query.Where(vp => vp.Venta.TenantId == tenantId.Value);
            }
            return query; // Si no hay TenantId (SuperAdmin), no filtrar
        }

        public async Task<List<VentaPago>> GetAllVentaPagos()
        {
            return await FilterByTenant(_context.VentaPagos
                .Include(vp => vp.Venta))
                .ToListAsync();
        }

        public async Task<VentaPago> GetVentaPagoById(int id)
        {
            return await FilterByTenant(_context.VentaPagos
                .Include(vp => vp.Venta))
                .FirstOrDefaultAsync(vp => vp.Id == id);
        }

        public async Task<List<VentaPago>> GetVentaPagosByVentaId(int ventaId)
        {
            // Primero verificar que la venta pertenece al tenant actual
            var venta = await _context.Ventas.FirstOrDefaultAsync(v => v.Id == ventaId);
            if (venta == null)
                return new List<VentaPago>();

            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue && venta.TenantId != tenantId.Value)
                return new List<VentaPago>();

            return await _context.VentaPagos
                .Where(vp => vp.VentaId == ventaId)
                .ToListAsync();
        }

        public async Task<VentaPago> AddVentaPago(VentaPago ventaPago)
        {
            _context.VentaPagos.Add(ventaPago);
            await _context.SaveChangesAsync();
            return ventaPago;
        }

        public async Task<VentaPago> UpdateVentaPago(VentaPago ventaPago)
        {
            _context.VentaPagos.Update(ventaPago);
            await _context.SaveChangesAsync();
            return ventaPago;
        }

        public async Task<bool> DeleteVentaPago(int id)
        {
            var ventaPago = await FilterByTenant(_context.VentaPagos
                .Include(vp => vp.Venta))
                .FirstOrDefaultAsync(vp => vp.Id == id);
            if (ventaPago == null)
                return false;

            _context.VentaPagos.Remove(ventaPago);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVentaPagosByVentaId(int ventaId)
        {
            // Primero verificar que la venta pertenece al tenant actual
            var venta = await _context.Ventas.FirstOrDefaultAsync(v => v.Id == ventaId);
            if (venta == null)
                return false;

            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue && venta.TenantId != tenantId.Value)
                return false;

            var ventaPagos = await _context.VentaPagos
                .Where(vp => vp.VentaId == ventaId)
                .ToListAsync();

            if (!ventaPagos.Any())
                return false;

            _context.VentaPagos.RemoveRange(ventaPagos);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

