using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class VentaPagosRepository : IVentaPagosRepository
    {
        private readonly AplicationDbContext _context;

        public VentaPagosRepository(AplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VentaPagos>> GetAllVentaPagos()
        {
            return await _context.VentaPagos.ToListAsync();
        }

        public async Task<VentaPagos> GetVentaPagosById(int id)
        {
            return await _context.VentaPagos.FindAsync(id);
        }

        public async Task<List<VentaPagos>> GetVentaPagosByPedidoId(int pedidoId)
        {
            return await _context.VentaPagos
                .Where(vp => vp.PedidoId == pedidoId)
                .ToListAsync();
        }

        public async Task<VentaPagos> AddVentaPagos(VentaPagos ventaPagos)
        {
            _context.VentaPagos.Add(ventaPagos);
            await _context.SaveChangesAsync();
            return ventaPagos;
        }

        public async Task<VentaPagos> UpdateVentaPagos(VentaPagos ventaPagos)
        {
            _context.VentaPagos.Update(ventaPagos);
            await _context.SaveChangesAsync();
            return ventaPagos;
        }

        public async Task<bool> DeleteVentaPagos(int id)
        {
            var ventaPagos = await _context.VentaPagos.FindAsync(id);
            if (ventaPagos == null)
                return false;

            _context.VentaPagos.Remove(ventaPagos);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVentaPagosByPedidoId(int pedidoId)
        {
            var ventaPagos = await _context.VentaPagos
                .Where(vp => vp.PedidoId == pedidoId)
                .ToListAsync();

            if (!ventaPagos.Any())
                return false;

            _context.VentaPagos.RemoveRange(ventaPagos);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
