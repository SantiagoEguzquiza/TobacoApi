using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;
using TobacoBackend.Persistence;

namespace TobacoBackend.Repositories
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly AplicationDbContext _context;

        public ProveedorRepository(AplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Proveedor> FilterByTenant(IQueryable<Proveedor> query)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue)
                return query.Where(p => p.TenantId == tenantId.Value);
            return query;
        }

        public async Task<List<Proveedor>> GetAllByTenantAsync()
        {
            return await FilterByTenant(_context.Proveedores)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Proveedor?> GetByIdAsync(int id)
        {
            return await FilterByTenant(_context.Proveedores)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Proveedor> CreateAsync(Proveedor proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
            return proveedor;
        }

        public async Task<bool> ExistsNombreAsync(string nombre, int? excludeId = null)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (!tenantId.HasValue) return false;
            var query = _context.Proveedores
                .Where(p => p.TenantId == tenantId.Value && p.Nombre == nombre);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}
