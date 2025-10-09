using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.Models;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AplicationDbContext _context;

    public CategoriaRepository(AplicationDbContext context)
    {
        this._context = context;
    }

    public async Task<List<Categoria>> GetAllAsync() =>
        await _context.Categorias.OrderBy(c => c.SortOrder).ToListAsync();

    public async Task<Categoria?> GetByIdAsync(int id) =>
        await _context.Categorias.FindAsync(id);

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
        var categoria = await _context.Categorias.FindAsync(id);
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
                var categoria = await _context.Categorias.FindAsync(id);
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
