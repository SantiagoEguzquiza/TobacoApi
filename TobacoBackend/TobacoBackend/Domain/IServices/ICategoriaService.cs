using TobacoBackend.DTOs;

public interface ICategoriaService
{
    Task<List<CategoriaDTO>> GetAllAsync();
    Task<CategoriaDTO?> GetByIdAsync(int id);
    Task AddAsync(CategoriaDTO categoriaDto);
    Task<CategoriaDTO?> UpdateAsync(int id, CategoriaDTO categoriaDto);
    Task DeleteAsync(int id);
    Task ReorderAsync(List<(int id, int sortOrder)> categoriaOrders);
}
