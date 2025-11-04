using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class RecorridoProgramadoRepository : IRecorridoProgramadoRepository
    {
        private readonly AplicationDbContext _context;

        public RecorridoProgramadoRepository(AplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecorridoProgramado>> GetRecorridosByVendedorAndDia(int vendedorId, DiaSemana diaSemana)
        {
            return await _context.RecorridosProgramados
                .Include(r => r.Cliente)
                .Include(r => r.Vendedor)
                .Where(r => r.VendedorId == vendedorId 
                    && r.DiaSemana == diaSemana 
                    && r.Activo)
                .OrderBy(r => r.Orden)
                .ToListAsync();
        }

        public async Task<List<RecorridoProgramado>> GetRecorridosByVendedor(int vendedorId)
        {
            return await _context.RecorridosProgramados
                .Include(r => r.Cliente)
                .Include(r => r.Vendedor)
                .Where(r => r.VendedorId == vendedorId && r.Activo)
                .OrderBy(r => r.DiaSemana)
                .ThenBy(r => r.Orden)
                .ToListAsync();
        }

        public async Task<RecorridoProgramado?> GetById(int id)
        {
            return await _context.RecorridosProgramados
                .Include(r => r.Cliente)
                .Include(r => r.Vendedor)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task Add(RecorridoProgramado recorrido)
        {
            await _context.RecorridosProgramados.AddAsync(recorrido);
        }

        public Task Update(RecorridoProgramado recorrido)
        {
            _context.RecorridosProgramados.Update(recorrido);
            return Task.CompletedTask;
        }

        public async Task Delete(int id)
        {
            var recorrido = await GetById(id);
            if (recorrido != null)
            {
                _context.RecorridosProgramados.Remove(recorrido);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

