using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Repositories
{
    public class VentaRepository : IVentaRepository
    {
        private readonly AplicationDbContext _context;

        public VentaRepository(AplicationDbContext context)
        {
            this._context = context;
        }

        public async Task AddVenta(Venta venta)
        {
            await _context.Ventas.AddAsync(venta);
            await _context.SaveChangesAsync();
        }

        public async Task AddVentaProducto(VentaProducto ventaProducto)
        {
            await _context.VentasProductos.AddAsync(ventaProducto);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateVentaProducto(VentaProducto ventaProducto)
        {
            var ventaProductoExistente = await _context.VentasProductos
                .FirstOrDefaultAsync(vp => vp.VentaId == ventaProducto.VentaId && vp.ProductoId == ventaProducto.ProductoId);

            if (ventaProductoExistente == null)
            {
                await _context.VentasProductos.AddAsync(ventaProducto);
            }
            else
            {
                ventaProductoExistente.Cantidad = ventaProducto.Cantidad;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteVenta(int id)
        {
            var venta = await _context.Ventas.FirstOrDefaultAsync(v => v.Id == id);

            if (venta != null)
            {
                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<Venta>> GetAllVentas()
        {
            return await _context.Ventas
                .AsNoTracking()
                .OrderByDescending(v => v.Id)
                .Include(v => v.Cliente)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioAsignado)
                .Include(v => v.VentaProductos)
                    .ThenInclude(vp => vp.Producto)
                .AsSplitQuery()
                .ToListAsync();
        }



        public async Task<Venta> GetVentaById(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioAsignado)
                .Include(v => v.VentaProductos)
                    .ThenInclude(vp => vp.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (venta == null)
            {
                throw new Exception($"La venta con id {id} no fue encontrada o no existe");
            }

            return venta;
        }

        public async Task UpdateVenta(Venta venta)
        {
            var ventaExistente = await _context.Ventas
                .Include(v => v.VentaProductos)
                .FirstOrDefaultAsync(v => v.Id == venta.Id);

            if (ventaExistente == null)
                throw new Exception("Venta no encontrada");

            ventaExistente.ClienteId = venta.ClienteId;
            ventaExistente.Fecha = DateTime.Now;

            _context.VentasProductos.RemoveRange(ventaExistente.VentaProductos);

            ventaExistente.VentaProductos = new List<VentaProducto>();
            decimal total = 0;

            foreach (var vp in venta.VentaProductos)
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == vp.ProductoId);
                if (producto == null)
                    throw new Exception($"Producto con ID {vp.ProductoId} no encontrado.");

                total += producto.Precio * vp.Cantidad;

                ventaExistente.VentaProductos.Add(new VentaProducto
                {
                    ProductoId = vp.ProductoId,
                    Cantidad = vp.Cantidad
                });
            }

            // Aplicar descuento global del cliente si existe
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == venta.ClienteId);
            if (cliente != null && cliente.DescuentoGlobal > 0)
            {
                var descuento = total * (cliente.DescuentoGlobal / 100);
                total = total - descuento;
            }

            ventaExistente.Total = total;

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<VentaPaginationResult> GetVentasPaginadas(int page, int pageSize)
        {
            var totalItems = await _context.Ventas.CountAsync();

            var ventas = await _context.Ventas
                .AsNoTracking()
                .OrderByDescending(v => v.Id)
                .Include(v => v.Cliente)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioAsignado)
                .Include(v => v.VentaProductos)
                    .ThenInclude(vp => vp.Producto)
                .AsSplitQuery()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new VentaPaginationResult
            {
                Ventas = ventas,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<VentaPaginationResult> GetVentasPorCliente(int clienteId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = _context.Ventas
                .AsNoTracking()
                .Where(v => v.ClienteId == clienteId);

            // Apply date filters if provided
            if (dateFrom.HasValue)
            {
                query = query.Where(v => v.Fecha >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(v => v.Fecha <= dateTo.Value);
            }

            var totalItems = await query.CountAsync();

            var ventas = await query
                .OrderByDescending(v => v.Fecha)
                .Include(v => v.Cliente)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioAsignado)
                .Include(v => v.VentaProductos)
                    .ThenInclude(vp => vp.Producto)
                .Include(v => v.VentaPagos)
                .AsSplitQuery()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new VentaPaginationResult
            {
                Ventas = ventas,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<VentaPaginationResult> GetVentasConCuentaCorrienteByClienteId(int clienteId, int page, int pageSize)
        {
            var query = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioAsignado)
                .Include(v => v.VentaProductos)
                    .ThenInclude(vp => vp.Producto)
                .AsSplitQuery()
                .Include(v => v.VentaPagos)
                .Where(v => v.ClienteId == clienteId && v.MetodoPago == MetodoPagoEnum.CuentaCorriente)
                .OrderByDescending(v => v.Fecha);

            var totalItems = await query.CountAsync();

            var ventas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new VentaPaginationResult
            {
                Ventas = ventas,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<bool> AsignarVentaAUsuario(int ventaId, int usuarioId)
        {
            var venta = await _context.Ventas.FirstOrDefaultAsync(v => v.Id == ventaId);
            if (venta == null)
            {
                return false;
            }

            venta.UsuarioIdAsignado = usuarioId;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

