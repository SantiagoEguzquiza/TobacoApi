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

        public async Task<List<VentaPago>> GetAllVentaPagos()
        {
            return await _context.VentaPagos.ToListAsync();
        }

        public async Task<VentaPago> GetVentaPagoById(int id)
        {
            return await _context.VentaPagos.FindAsync(id);
        }

        public async Task<List<VentaPago>> GetVentaPagosByVentaId(int ventaId)
        {
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
            var ventaPago = await _context.VentaPagos.FindAsync(id);
            if (ventaPago == null)
                return false;

            _context.VentaPagos.Remove(ventaPago);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVentaPagosByVentaId(int ventaId)
        {
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

