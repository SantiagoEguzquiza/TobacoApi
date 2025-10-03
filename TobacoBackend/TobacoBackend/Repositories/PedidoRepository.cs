using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AplicationDbContext _context;

        public PedidoRepository(AplicationDbContext context)
        {
            this._context = context;
        }

        public async Task AddPedido(Pedido pedido)
        {
            await _context.Pedidos.AddAsync(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task AddPedidoProducto(PedidoProducto pedidoProducto)
        {
            await _context.PedidosProductos.AddAsync(pedidoProducto);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdatePedidoProducto(PedidoProducto pedidoProducto)
        {
            var pedidoProductoExistente = await _context.PedidosProductos
                .FirstOrDefaultAsync(pp => pp.PedidoId == pedidoProducto.PedidoId && pp.ProductoId == pedidoProducto.ProductoId);

            if (pedidoProductoExistente == null)
            {
                await _context.PedidosProductos.AddAsync(pedidoProducto);
            }
            else
            {
                pedidoProductoExistente.Cantidad = pedidoProducto.Cantidad;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(c => c.Id == id);

            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<Pedido>> GetAllPedidos()
        {
            return await _context.Pedidos
                .AsNoTracking()                     // lecturas más rápidas
                .OrderByDescending(p => p.Id) // o p.Id si es identity creciente
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.PedidoProductos)
                    .ThenInclude(pp => pp.Producto)
                .AsSplitQuery()                     // evita explosiones cartesianas
                .ToListAsync();
        }



        public async Task<Pedido> GetPedidoById(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.PedidoProductos)
                    .ThenInclude(pp => pp.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (pedido == null)
            {
                throw new Exception($"El pedido con id {id} no fue encontrado o no existe");
            }

            return pedido;
        }

        public async Task UpdatePedido(Pedido pedido)
        {
            var pedidoExistente = await _context.Pedidos
                .Include(p => p.PedidoProductos)
                .FirstOrDefaultAsync(p => p.Id == pedido.Id);

            if (pedidoExistente == null)
                throw new Exception("Pedido no encontrado");

            pedidoExistente.ClienteId = pedido.ClienteId;
            pedidoExistente.Fecha = DateTime.Now;

            _context.PedidosProductos.RemoveRange(pedidoExistente.PedidoProductos);

            pedidoExistente.PedidoProductos = new List<PedidoProducto>();
            decimal total = 0;

            foreach (var pp in pedido.PedidoProductos)
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == pp.ProductoId);
                if (producto == null)
                    throw new Exception($"Producto con ID {pp.ProductoId} no encontrado.");

                total += producto.Precio * pp.Cantidad;

                pedidoExistente.PedidoProductos.Add(new PedidoProducto
                {
                    ProductoId = pp.ProductoId,
                    Cantidad = pp.Cantidad
                });
            }

            // Aplicar descuento global del cliente si existe
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == pedido.ClienteId);
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                var descuento = total * (cliente.DescuentoGlobal / 100);
                total = total - descuento;
            }

            pedidoExistente.Total = total;

            await _context.SaveChangesAsync();
        }

        public async Task<PedidoPaginationResult> GetPedidosPaginados(int page, int pageSize)
        {
            var totalItems = await _context.Pedidos.CountAsync();

            var pedidos = await _context.Pedidos
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.PedidoProductos)
                    .ThenInclude(pp => pp.Producto)
                .AsSplitQuery()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PedidoPaginationResult
            {
                Pedidos = pedidos,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<PedidoPaginationResult> GetPedidosPorCliente(int clienteId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = _context.Pedidos
                .AsNoTracking()
                .Where(p => p.ClienteId == clienteId);

            // Apply date filters if provided
            if (dateFrom.HasValue)
            {
                query = query.Where(p => p.Fecha >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(p => p.Fecha <= dateTo.Value);
            }

            var totalItems = await query.CountAsync();

            var pedidos = await query
                .OrderByDescending(p => p.Fecha)
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.PedidoProductos)
                    .ThenInclude(pp => pp.Producto)
                .AsSplitQuery()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PedidoPaginationResult
            {
                Pedidos = pedidos,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

    }
}
