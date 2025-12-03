using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class AsistenciaRepository : IAsistenciaRepository
    {
        private readonly AplicationDbContext _context;

        public AsistenciaRepository(AplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el TenantId actual del contexto para filtrar las consultas
        /// </summary>
        private IQueryable<Asistencia> FilterByTenant(IQueryable<Asistencia> query)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (tenantId.HasValue)
            {
                return query.Where(a => a.TenantId == tenantId.Value);
            }
            return query; // Si no hay TenantId (SuperAdmin), no filtrar
        }

        public async Task<Asistencia> RegistrarEntradaAsync(Asistencia asistencia)
        {
            _context.Asistencias.Add(asistencia);
            await _context.SaveChangesAsync();
            return asistencia;
        }

        public async Task<Asistencia?> RegistrarSalidaAsync(int asistenciaId, DateTime fechaHoraSalida, string? ubicacionSalida, string? latitudSalida, string? longitudSalida)
        {
            var asistencia = await FilterByTenant(_context.Asistencias)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == asistenciaId);

            if (asistencia == null)
                return null;

            asistencia.FechaHoraSalida = fechaHoraSalida;
            asistencia.UbicacionSalida = ubicacionSalida;
            asistencia.LatitudSalida = latitudSalida;
            asistencia.LongitudSalida = longitudSalida;

            _context.Asistencias.Update(asistencia);
            await _context.SaveChangesAsync();

            return asistencia;
        }

        public async Task<Asistencia?> GetByIdAsync(int id)
        {
            return await FilterByTenant(_context.Asistencias)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Asistencia?> GetAsistenciaActivaByUserIdAsync(int userId)
        {
            return await FilterByTenant(_context.Asistencias)
                .Include(a => a.User)
                .Where(a => a.UserId == userId && a.FechaHoraSalida == null)
                .OrderByDescending(a => a.FechaHoraEntrada)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Asistencia>> GetAsistenciasByUserIdAsync(int userId)
        {
            return await FilterByTenant(_context.Asistencias)
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.FechaHoraEntrada)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asistencia>> GetAsistenciasByUserIdAndDateRangeAsync(int userId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await FilterByTenant(_context.Asistencias)
                .Include(a => a.User)
                .Where(a => a.UserId == userId && a.FechaHoraEntrada >= fechaInicio && a.FechaHoraEntrada <= fechaFin)
                .OrderByDescending(a => a.FechaHoraEntrada)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asistencia>> GetAllAsistenciasAsync()
        {
            return await FilterByTenant(_context.Asistencias)
                .Include(a => a.User)
                .OrderByDescending(a => a.FechaHoraEntrada)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asistencia>> GetAsistenciasByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await FilterByTenant(_context.Asistencias)
                .Include(a => a.User)
                .Where(a => a.FechaHoraEntrada >= fechaInicio && a.FechaHoraEntrada <= fechaFin)
                .OrderByDescending(a => a.FechaHoraEntrada)
                .ToListAsync();
        }
    }
}

