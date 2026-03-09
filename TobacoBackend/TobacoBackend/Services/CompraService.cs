using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Persistence;

namespace TobacoBackend.Services
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly AplicationDbContext _context;
        private readonly IMapper _mapper;

        public CompraService(
            ICompraRepository compraRepository,
            IProveedorRepository proveedorRepository,
            IProductoRepository productoRepository,
            AplicationDbContext context,
            IMapper mapper)
        {
            _compraRepository = compraRepository;
            _proveedorRepository = proveedorRepository;
            _productoRepository = productoRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<CompraDTO> CreateAsync(CreateCompraDTO dto)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (!tenantId.HasValue)
                throw new InvalidOperationException("No se pudo determinar el tenant. Inicia sesión con un usuario de tenant.");

            if (dto.Items == null || dto.Items.Count == 0)
                throw new InvalidOperationException("La compra debe tener al menos un ítem.");

            foreach (var item in dto.Items)
            {
                if (item.Cantidad <= 0)
                    throw new InvalidOperationException($"La cantidad del producto {item.ProductoId} debe ser mayor a 0.");
                if (item.CostoUnitario < 0)
                    throw new InvalidOperationException($"El costo unitario del producto {item.ProductoId} no puede ser negativo.");
            }

            var proveedor = await _proveedorRepository.GetByIdAsync(dto.ProveedorId);
            if (proveedor == null)
                throw new InvalidOperationException("Proveedor no encontrado.");

            foreach (var item in dto.Items)
            {
                _ = await _productoRepository.GetProductoById(item.ProductoId);
            }

            decimal totalCalculado = 0;
            var itemsParaGuardar = new List<(CreateCompraItemDTO dto, decimal subtotal)>();
            foreach (var item in dto.Items)
            {
                var subtotal = item.Cantidad * item.CostoUnitario;
                totalCalculado += subtotal;
                itemsParaGuardar.Add((item, subtotal));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var compra = new Compra
                {
                    ProveedorId = dto.ProveedorId,
                    Fecha = dto.Fecha.Kind == DateTimeKind.Utc ? dto.Fecha : DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
                    NumeroComprobante = string.IsNullOrWhiteSpace(dto.NumeroComprobante) ? null : dto.NumeroComprobante.Trim(),
                    Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim(),
                    Total = totalCalculado,
                    TenantId = tenantId.Value
                };

                foreach (var (itemDto, subtotal) in itemsParaGuardar)
                {
                    compra.Items.Add(new CompraItem
                    {
                        ProductoId = itemDto.ProductoId,
                        Cantidad = itemDto.Cantidad,
                        CostoUnitario = itemDto.CostoUnitario,
                        Subtotal = subtotal,
                        TenantId = tenantId.Value
                    });
                }

                var compraCreada = await _compraRepository.CreateAsync(compra);

                foreach (var (itemDto, _) in itemsParaGuardar)
                {
                    var producto = await _productoRepository.GetProductoById(itemDto.ProductoId);
                    var stockActual = producto.Stock;
                    var nuevoStock = stockActual + itemDto.Cantidad;
                    producto.Stock = nuevoStock;
                    producto.UltimoCostoCompra = itemDto.CostoUnitario;

                    if (stockActual <= 0)
                    {
                        producto.CostoPromedio = itemDto.CostoUnitario;
                    }
                    else
                    {
                        var costoPromedioActual = producto.CostoPromedio ?? 0;
                        producto.CostoPromedio = ((stockActual * costoPromedioActual) + (itemDto.Cantidad * itemDto.CostoUnitario)) / nuevoStock;
                    }

                    await _productoRepository.UpdateProducto(producto);
                }

                await transaction.CommitAsync();

                var compraConDetalle = await _compraRepository.GetByIdAsync(compraCreada.Id);
                return _mapper.Map<CompraDTO>(compraConDetalle);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CompraDTO?> GetByIdAsync(int id)
        {
            var compra = await _compraRepository.GetByIdAsync(id);
            return compra != null ? _mapper.Map<CompraDTO>(compra) : null;
        }

        public async Task<List<CompraDTO>> GetAllAsync(DateTime? desde = null, DateTime? hasta = null)
        {
            var compras = await _compraRepository.GetAllByTenantAsync(desde, hasta);
            return _mapper.Map<List<CompraDTO>>(compras);
        }

        public async Task DeleteAsync(int id)
        {
            var compra = await _compraRepository.GetByIdAsync(id);
            if (compra == null)
                throw new InvalidOperationException("Compra no encontrada.");

            if (compra.Items == null || compra.Items.Count == 0)
            {
                await _compraRepository.DeleteAsync(compra);
                return;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in compra.Items)
                {
                    var producto = await _productoRepository.GetProductoById(item.ProductoId);
                    var stockActual = producto.Stock;
                    var cantidadARevertir = item.Cantidad;

                    if (stockActual < cantidadARevertir)
                        throw new InvalidOperationException(
                            $"No se puede eliminar la compra: el producto \"{producto.Nombre}\" tiene stock actual {stockActual} " +
                            $"y la compra agregó {cantidadARevertir} unidades. Revertir dejaría stock negativo.");

                    producto.Stock = stockActual - cantidadARevertir;
                    await _productoRepository.UpdateProducto(producto);
                }

                await _compraRepository.DeleteAsync(compra);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
