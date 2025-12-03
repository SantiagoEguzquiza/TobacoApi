using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class AbonosRepository : IAbonosRepository
    {
        private readonly AplicationDbContext _context;
        public AbonosRepository(AplicationDbContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// Obtiene el TenantId actual del contexto para filtrar las consultas
        /// </summary>
        private IQueryable<Abonos> FilterByTenant(IQueryable<Abonos> query)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue)
            {
                return query.Where(a => a.TenantId == tenantId.Value);
            }
            return query; // Si no hay TenantId (SuperAdmin), no filtrar
        }
        public async  Task<Abonos> AddAbono(Abonos abono)
        {
            _context.Abonos.Add(abono);
            await _context.SaveChangesAsync();
            return abono; 
        }

        public async Task<bool> DeleteAbono(int id)
        {
            var abono = await FilterByTenant(_context.Abonos).Where(c => c.Id == id).FirstOrDefaultAsync();

            if (abono != null)
            {
                _context.Abonos.Remove(abono);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<Abonos> GetAbonoById(int id)
        {
            var abono = await FilterByTenant(_context.Abonos)
                .Include(a => a.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (abono == null)
            {
                throw new Exception($"El abono con id {id} no fue encontrado o no existe");
            }

            return abono;
        }

        public async Task<List<Abonos>> GetAllAbonos()
        {
            return await FilterByTenant(_context.Abonos)
                .Include(a => a.Cliente)
                .ToListAsync(); 
        }

        public async Task<List<Abonos>> GetAbonosByClienteId(int clienteId)
        {
            return await FilterByTenant(_context.Abonos)
                .Include(a => a.Cliente)
                .Where(a => a.ClienteId == clienteId)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }

        public async Task UpdateAbono(Abonos abono)
        {
            _context.Abonos.Update(abono);
            await _context.SaveChangesAsync();
        }
    }
}
