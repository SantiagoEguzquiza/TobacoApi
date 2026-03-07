using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;
using TobacoBackend.Persistence;

namespace TobacoBackend.Repositories
{
    public class CompraRepository : ICompraRepository
    {
        private readonly AplicationDbContext _context;

        public CompraRepository(AplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Compra> FilterByTenant(IQueryable<Compra> query)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue)
                return query.Where(c => c.TenantId == tenantId.Value);
            return query;
        }

        public async Task<Compra> CreateAsync(Compra compra)
        {
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();
            return compra;
        }

        public async Task<Compra?> GetByIdAsync(int id)
        {
            return await FilterByTenant(_context.Compras)
                .Include(c => c.Proveedor)
                .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Compra>> GetAllByTenantAsync(DateTime? desde = null, DateTime? hasta = null)
        {
            IQueryable<Compra> query = FilterByTenant(_context.Compras)
                .Include(c => c.Proveedor);
            if (desde.HasValue)
                query = query.Where(c => c.Fecha.Date >= desde.Value.Date);
            if (hasta.HasValue)
                query = query.Where(c => c.Fecha.Date <= hasta.Value.Date);
            return await query
                .OrderByDescending(c => c.Fecha)
                .ThenByDescending(c => c.Id)
                .ToListAsync();
        }
    }
}
