using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class ProductoAFavorRepository : IProductoAFavorRepository
    {
        private readonly AplicationDbContext _context;

        public ProductoAFavorRepository(AplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductoAFavor>> GetAllProductosAFavor()
        {
            return await _context.ProductosAFavor
                .Include(p => p.Cliente)
                .Include(p => p.Producto)
                .Include(p => p.Venta)
                .Include(p => p.UsuarioRegistro)
                .Include(p => p.UsuarioEntrega)
                .ToListAsync();
        }

        public async Task<ProductoAFavor> GetProductoAFavorById(int id)
        {
            return await _context.ProductosAFavor
                .Include(p => p.Cliente)
                .Include(p => p.Producto)
                .Include(p => p.Venta)
                .Include(p => p.UsuarioRegistro)
                .Include(p => p.UsuarioEntrega)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<ProductoAFavor>> GetProductosAFavorByClienteId(int clienteId, bool? soloNoEntregados = null)
        {
            var query = _context.ProductosAFavor
                .Include(p => p.Cliente)
                .Include(p => p.Producto)
                .Include(p => p.Venta)
                .Include(p => p.UsuarioRegistro)
                .Include(p => p.UsuarioEntrega)
                .Where(p => p.ClienteId == clienteId);

            if (soloNoEntregados == true)
            {
                query = query.Where(p => !p.Entregado);
            }

            return await query
                .OrderByDescending(p => p.FechaRegistro)
                .ToListAsync();
        }

        public async Task<List<ProductoAFavor>> GetProductosAFavorByVentaId(int ventaId)
        {
            return await _context.ProductosAFavor
                .Include(p => p.Cliente)
                .Include(p => p.Producto)
                .Include(p => p.Venta)
                .Include(p => p.UsuarioRegistro)
                .Include(p => p.UsuarioEntrega)
                .Where(p => p.VentaId == ventaId)
                .OrderBy(p => p.ProductoId)
                .ToListAsync();
        }

        public async Task AddProductoAFavor(ProductoAFavor productoAFavor)
        {
            await _context.ProductosAFavor.AddAsync(productoAFavor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductoAFavor(ProductoAFavor productoAFavor)
        {
            _context.ProductosAFavor.Update(productoAFavor);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteProductoAFavor(int id)
        {
            var productoAFavor = await _context.ProductosAFavor.FindAsync(id);
            if (productoAFavor == null) return false;

            _context.ProductosAFavor.Remove(productoAFavor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductosAFavorByVentaId(int ventaId)
        {
            var productosAFavor = await _context.ProductosAFavor
                .Where(paf => paf.VentaId == ventaId)
                .ToListAsync();

            if (!productosAFavor.Any()) return false;

            _context.ProductosAFavor.RemoveRange(productosAFavor);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

