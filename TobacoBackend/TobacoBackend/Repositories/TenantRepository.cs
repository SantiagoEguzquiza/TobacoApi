using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly AplicationDbContext _context;

        public TenantRepository(AplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<Tenant?> GetTenantByIdAsync(int id)
        {
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        public async Task<Tenant> UpdateTenantAsync(Tenant tenant)
        {
            tenant.UpdatedAt = DateTime.UtcNow;
            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        public async Task<bool> DeleteTenantAsync(int id)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
                return false;

            // Verificar si tiene usuarios asociados
            var hasUsers = await _context.Users.AnyAsync(u => u.TenantId == id);
            if (hasUsers)
            {
                throw new InvalidOperationException("No se puede eliminar un tenant que tiene usuarios asociados.");
            }

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TenantExistsAsync(int id)
        {
            return await _context.Tenants.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> TenantNameExistsAsync(string nombre, int? excludeId = null)
        {
            var query = _context.Tenants.Where(t => t.Nombre == nombre);
            
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}

