using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AplicationDbContext _context;

        public UserRepository(AplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el TenantId actual del contexto para filtrar las consultas
        /// </summary>
        private IQueryable<User> FilterByTenant(IQueryable<User> query)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue)
            {
                return query.Where(u => u.TenantId == tenantId.Value);
            }
            return query; // Si no hay TenantId (SuperAdmin), no filtrar
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await FilterByTenant(_context.Users)
                .FirstOrDefaultAsync(u => u.UserName == userName && u.IsActive);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await FilterByTenant(_context.Users)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            // Verificar si la entidad ya está siendo rastreada
            var trackedEntity = _context.ChangeTracker.Entries<User>()
                .FirstOrDefault(e => e.Entity.Id == user.Id);

            if (trackedEntity != null)
            {
                // Si ya está siendo rastreada, actualizar los valores de la entidad rastreada
                trackedEntity.CurrentValues.SetValues(user);
            }
            else
            {
                // Si no está siendo rastreada, usar Update
                _context.Users.Update(user);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string userName)
        {
            return await FilterByTenant(_context.Users)
                .AnyAsync(u => u.UserName == userName);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await FilterByTenant(_context.Users)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await FilterByTenant(_context.Users)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetSubUsersByCreatedByIdAsync(int createdById)
        {
            return await FilterByTenant(_context.Users)
                .Where(u => u.CreatedById == createdById)
                .ToListAsync();
        }

        public async Task UpdateSubUsersPlanAsync(int createdById, PlanType newPlan)
        {
            var subUsers = await FilterByTenant(_context.Users)
                .Where(u => u.CreatedById == createdById)
                .ToListAsync();

            foreach (var subUser in subUsers)
            {
                subUser.Plan = newPlan;
            }

            if (subUsers.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
