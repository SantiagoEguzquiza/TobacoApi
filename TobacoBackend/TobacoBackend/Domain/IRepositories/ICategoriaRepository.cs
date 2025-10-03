using TobacoBackend.Domain.Models;

public interface ICategoriaRepository
{
    Task<List<Categoria>> GetAllAsync();
    Task<Categoria?> GetByIdAsync(int id);
    Task AddAsync(Categoria categoria);
    Task UpdateAsync(Categoria categoria);
    Task DeleteAsync(int id);
    Task ReorderAsync(List<(int id, int sortOrder)> categoriaOrders);
}
