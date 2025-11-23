using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AplicationDbContext _context;
        public ProductoRepository(AplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddProducto(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteProducto(int id)
        {
            var producto = await _context.Productos.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (producto != null)
            {
                // Verificar si el producto tiene ventas vinculadas
                var tieneVentas = await _context.VentasProductos
                    .AnyAsync(vp => vp.ProductoId == id);

                if (tieneVentas)
                {
                    throw new InvalidOperationException("No se puede eliminar el producto porque tiene ventas vinculadas.");
                }

                // Verificar si el producto tiene precios especiales vinculados
                var tienePreciosEspeciales = await _context.PreciosEspeciales
                    .AnyAsync(pe => pe.ProductoId == id);

                if (tienePreciosEspeciales)
                {
                    throw new InvalidOperationException("No se puede eliminar el producto porque tiene precios especiales configurados para clientes.");
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> SoftDeleteProducto(int id)
        {
            var producto = await _context.Productos.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (producto != null)
            {
                producto.IsActive = false;
                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> ActivateProducto(int id)
        {
            var producto = await _context.Productos.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (producto != null)
            {
                producto.IsActive = true;
                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<Producto>> GetAllProductos()
        {
            return await _context.Productos
                .Where(p => p.IsActive)
                .Include(p => p.Categoria)
                .Include(p => p.QuantityPrices)
                .ToListAsync();
        }


        public async Task<Producto> GetProductoById(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.QuantityPrices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                throw new Exception($"El producto con id {id} no fue encontrado.");

            return producto;
        }


        public async Task UpdateProducto(Producto producto)
        {
            // Get the existing product with quantity prices
            var existingProduct = await _context.Productos
                .Include(p => p.QuantityPrices)
                .FirstOrDefaultAsync(p => p.Id == producto.Id);

            if (existingProduct == null)
                throw new Exception($"El producto con id {producto.Id} no fue encontrado.");

            // Update basic product properties
            existingProduct.Nombre = producto.Nombre;
            existingProduct.Stock = producto.Stock;
            existingProduct.Precio = producto.Precio;
            existingProduct.CategoriaId = producto.CategoriaId;
            existingProduct.Half = producto.Half;
            existingProduct.IsActive = producto.IsActive;
            existingProduct.Marca = producto.Marca;

            // Remove existing quantity prices
            _context.ProductQuantityPrices.RemoveRange(existingProduct.QuantityPrices);

            // Add new quantity prices
            foreach (var qp in producto.QuantityPrices)
            {
                qp.Id = 0; // Reset ID for new entities
                qp.ProductId = producto.Id;
                _context.ProductQuantityPrices.Add(qp);
            }

            _context.Productos.Update(existingProduct);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductoPaginationResult> GetProductosPaginados(int page, int pageSize)
        {
            var totalItems = await _context.Productos
                .Where(p => p.IsActive)
                .CountAsync();

            var productos = await _context.Productos
                .Where(p => p.IsActive)
                .Include(p => p.Categoria)
                .Include(p => p.QuantityPrices)
                .OrderBy(p => p.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new ProductoPaginationResult
            {
                Productos = productos,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }
    }
}
