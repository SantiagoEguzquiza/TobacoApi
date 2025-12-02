using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class PermisosEmpleadoRepository : IPermisosEmpleadoRepository
    {
        private readonly AplicationDbContext _context;

        public PermisosEmpleadoRepository(AplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PermisosEmpleado?> GetByUserIdAsync(int userId)
        {
            return await _context.PermisosEmpleados
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<PermisosEmpleado> CreateAsync(PermisosEmpleado permisos)
        {
            _context.PermisosEmpleados.Add(permisos);
            await _context.SaveChangesAsync();
            return permisos;
        }

        public async Task<PermisosEmpleado> UpdateAsync(PermisosEmpleado permisos)
        {
            permisos.UpdatedAt = DateTime.UtcNow;
            _context.PermisosEmpleados.Update(permisos);
            await _context.SaveChangesAsync();
            return permisos;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var permisos = await _context.PermisosEmpleados.FindAsync(id);
            if (permisos == null)
                return false;

            _context.PermisosEmpleados.Remove(permisos);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

