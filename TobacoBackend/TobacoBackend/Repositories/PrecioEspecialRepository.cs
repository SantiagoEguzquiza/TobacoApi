using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class PrecioEspecialRepository : IPrecioEspecialRepository
    {
        private readonly AplicationDbContext _context;

        public PrecioEspecialRepository(AplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PrecioEspecial>> GetAllPreciosEspecialesAsync()
        {
            return await _context.PreciosEspeciales
                .Include(pe => pe.Cliente)
                .Include(pe => pe.Producto)
                .ToListAsync();
        }

        public async Task<PrecioEspecial?> GetPrecioEspecialByIdAsync(int id)
        {
            return await _context.PreciosEspeciales
                .Include(pe => pe.Cliente)
                .Include(pe => pe.Producto)
                .FirstOrDefaultAsync(pe => pe.Id == id);
        }

        public async Task<List<PrecioEspecial>> GetPreciosEspecialesByClienteIdAsync(int clienteId)
        {
            return await _context.PreciosEspeciales
                .Include(pe => pe.Producto)
                .ThenInclude(p => p.Categoria)
                .Where(pe => pe.ClienteId == clienteId)
                .OrderBy(pe => pe.Producto.Nombre)
                .ToListAsync();
        }

        public async Task<PrecioEspecial?> GetPrecioEspecialByClienteAndProductoAsync(int clienteId, int productoId)
        {
            return await _context.PreciosEspeciales
                .Include(pe => pe.Cliente)
                .Include(pe => pe.Producto)
                .FirstOrDefaultAsync(pe => pe.ClienteId == clienteId && pe.ProductoId == productoId);
        }

        public async Task<PrecioEspecial> AddPrecioEspecialAsync(PrecioEspecial precioEspecial)
        {
            _context.PreciosEspeciales.Add(precioEspecial);
            await _context.SaveChangesAsync();
            return precioEspecial;
        }

        public async Task<PrecioEspecial> UpdatePrecioEspecialAsync(PrecioEspecial precioEspecial)
        {
            precioEspecial.FechaActualizacion = DateTime.UtcNow;
            _context.PreciosEspeciales.Update(precioEspecial);
            await _context.SaveChangesAsync();
            return precioEspecial;
        }

        public async Task<bool> DeletePrecioEspecialAsync(int id)
        {
            var precioEspecial = await _context.PreciosEspeciales.FindAsync(id);
            if (precioEspecial == null)
                return false;

            _context.PreciosEspeciales.Remove(precioEspecial);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePrecioEspecialByClienteAndProductoAsync(int clienteId, int productoId)
        {
            var precioEspecial = await _context.PreciosEspeciales
                .FirstOrDefaultAsync(pe => pe.ClienteId == clienteId && pe.ProductoId == productoId);
            
            if (precioEspecial == null)
                return false;

            _context.PreciosEspeciales.Remove(precioEspecial);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsPrecioEspecialAsync(int clienteId, int productoId)
        {
            return await _context.PreciosEspeciales
                .AnyAsync(pe => pe.ClienteId == clienteId && pe.ProductoId == productoId);
        }

        public async Task<List<PrecioEspecial>> GetPreciosEspecialesByProductoIdAsync(int productoId)
        {
            return await _context.PreciosEspeciales
                .Include(pe => pe.Cliente)
                .Where(pe => pe.ProductoId == productoId)
                .ToListAsync();
        }
    }
}
