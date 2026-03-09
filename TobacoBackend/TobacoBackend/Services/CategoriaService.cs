using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _repository;
    private readonly IMapper _mapper;
    private readonly AplicationDbContext _context;

    public CategoriaService(ICategoriaRepository repository, IMapper mapper, AplicationDbContext context)
    {
        _repository = repository;
        _mapper = mapper;
        _context = context;
    }

    public async Task<List<CategoriaDTO>> GetAllAsync()
    {
        var categorias = await _repository.GetAllAsync();
        return _mapper.Map<List<CategoriaDTO>>(categorias);
    }

    public async Task<CategoriaDTO?> GetByIdAsync(int id)
    {
        var categoria = await _repository.GetByIdAsync(id);
        return categoria == null ? null : _mapper.Map<CategoriaDTO>(categoria);
    }

    public async Task AddAsync(CategoriaDTO categoriaDto)
    {
        var categoria = _mapper.Map<Categoria>(categoriaDto);
        
        // Set TenantId from current context
        var tenantId = _context.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            throw new InvalidOperationException("No se pudo determinar el TenantId del contexto actual.");
        }
        categoria.TenantId = tenantId.Value;
        
        // Set SortOrder to the next available value (highest + 1)
        var allCategorias = await _repository.GetAllAsync();
        categoria.SortOrder = allCategorias.Any() ? allCategorias.Max(c => c.SortOrder) + 1 : 0;
        
        await _repository.AddAsync(categoria);
    }

    public async Task<CategoriaDTO?> UpdateAsync(int id, CategoriaDTO categoriaDto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return null;

        var nombreTrimmed = categoriaDto.Nombre.Trim();
        var allCategorias = await _repository.GetAllAsync();
        if (allCategorias.Any(c => c.Id != id && string.Equals(c.Nombre.Trim(), nombreTrimmed, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");

        existing.Nombre = nombreTrimmed;
        existing.ColorHex = categoriaDto.ColorHex;
        await _repository.UpdateAsync(existing);

        return _mapper.Map<CategoriaDTO>(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var tenantId = _context.GetCurrentTenantId();
        if (!tenantId.HasValue)
            throw new InvalidOperationException("No se pudo determinar el TenantId.");

        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);
        if (categoria == null)
            throw new InvalidOperationException("No se encontró la categoría o no tienes permiso para eliminarla.");

        // Productos en esta categoría (del mismo tenant)
        var productosEnCategoria = await _context.Productos
            .Where(p => p.CategoriaId == id && p.TenantId == tenantId.Value)
            .ToListAsync();

        var productosActivos = productosEnCategoria.Where(p => p.IsActive).ToList();
        if (productosActivos.Any())
            throw new InvalidOperationException(
                $"No se puede eliminar la categoría porque tiene {productosActivos.Count} producto(s) activo(s). Desactiva los productos primero.");

        // Solo hay productos desactivados (o ninguno): reasignar a otra categoría si hay productos
        if (productosEnCategoria.Any())
        {
            var otraCategoria = await _context.Categorias
                .Where(c => c.TenantId == tenantId.Value && c.Id != id)
                .OrderBy(c => c.SortOrder)
                .FirstOrDefaultAsync();

            if (otraCategoria == null)
                throw new InvalidOperationException(
                    "No se puede eliminar la única categoría si tiene productos asignados (aunque estén desactivados).");

            foreach (var p in productosEnCategoria)
                p.CategoriaId = otraCategoria.Id;
            await _context.SaveChangesAsync();
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task ReorderAsync(List<(int id, int sortOrder)> categoriaOrders)
    {
        await _repository.ReorderAsync(categoriaOrders);
    }

}
