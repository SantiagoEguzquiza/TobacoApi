using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.Models;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AplicationDbContext _context;

    public CategoriaRepository(AplicationDbContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Obtiene el TenantId actual del contexto para filtrar las consultas
    /// </summary>
    private IQueryable<Categoria> FilterByTenant(IQueryable<Categoria> query)
    {
        var tenantId = _context.GetCurrentTenantId();
        if (tenantId.HasValue)
        {
            return query.Where(c => c.TenantId == tenantId.Value);
        }
        return query; // Si no hay TenantId (SuperAdmin), no filtrar
    }

    public async Task<List<Categoria>> GetAllAsync() =>
        await FilterByTenant(_context.Categorias).OrderBy(c => c.SortOrder).ToListAsync();

    public async Task<Categoria?> GetByIdAsync(int id) =>
        await FilterByTenant(_context.Categorias).FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var categoria = await FilterByTenant(_context.Categorias).FirstOrDefaultAsync(c => c.Id == id);
        if (categoria != null)
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderAsync(List<(int id, int sortOrder)> categoriaOrders)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var (id, sortOrder) in categoriaOrders)
            {
                var categoria = await FilterByTenant(_context.Categorias).FirstOrDefaultAsync(c => c.Id == id);
                if (categoria != null)
                {
                    categoria.SortOrder = sortOrder;
                }
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
