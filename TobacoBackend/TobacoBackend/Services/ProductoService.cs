    using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Repositories;

namespace TobacoBackend.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;

        public ProductoService(IProductoRepository productoRepository, IMapper mapper)
        {
            _productoRepository = productoRepository;
            _mapper = mapper;
        }

        public async Task<ProductoDTO> AddProducto(ProductoDTO productoDto)
        {
            // Validar lógica de descuento
            ValidateDiscountLogic(productoDto);
            
            var producto = _mapper.Map<Producto>(productoDto);
            
            // Si es indefinido, limpiar la fecha
            if (producto.descuentoIndefinido)
            {
                producto.fechaExpiracionDescuento = null;
            }
            
            await _productoRepository.AddProducto(producto);
            
            // Retornar el producto creado mapeado a DTO
            return _mapper.Map<ProductoDTO>(producto);
        }

        private void ValidateDiscountLogic(ProductoDTO productoDto)
        {
            // Si el descuento es indefinido, ignorar la fecha
            if (productoDto.descuentoIndefinido)
            {
                productoDto.fechaExpiracionDescuento = null;
            }
            
            // Validar rango de descuento (0-100)
            if (productoDto.Descuento < 0 || productoDto.Descuento > 100)
            {
                throw new ArgumentException("El descuento debe estar entre 0 y 100.");
            }
        }

        public async Task<bool> DeleteProducto(int id)
        {
            try
            {
                return await _productoRepository.DeleteProducto(id);
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar la excepción para que el controlador la maneje
                throw;
            }
        }

        public async Task<bool> SoftDeleteProducto(int id)
        {
            return await _productoRepository.SoftDeleteProducto(id);
        }

        public async Task<bool> ActivateProducto(int id)
        {
            return await _productoRepository.ActivateProducto(id);
        }

        private async Task ApplyDiscountExpirationLogicAsync(Producto producto)
        {
            // Si el descuento es indefinido, no hacer nada
            if (producto.descuentoIndefinido)
            {
                return;
            }

            // Si el descuento no es indefinido y tiene fecha de expiración
            if (producto.fechaExpiracionDescuento.HasValue)
            {
                // Si la fecha ya venció, poner descuento en 0 y guardar
                if (producto.fechaExpiracionDescuento.Value < DateTime.UtcNow)
                {
                    // Guardar el cambio en la base de datos
                    await _productoRepository.UpdateProductoDiscount(producto.Id, 0, null, false);
                    // Actualizar el objeto en memoria
                    producto.Descuento = 0;
                    producto.fechaExpiracionDescuento = null;
                }
            }
        }

        public async Task<List<ProductoDTO>> GetAllProductos()
        {
            var productos = await _productoRepository.GetAllProductos();
            
            // Aplicar lógica de expiración de descuentos
            foreach (var producto in productos)
            {
                await ApplyDiscountExpirationLogicAsync(producto);
            }
            
            return _mapper.Map<List<ProductoDTO>>(productos);
        }

        public async Task<ProductoDTO> GetProductoById(int id)
        {
            var producto = await _productoRepository.GetProductoById(id);
            
            // Aplicar lógica de expiración de descuentos
            await ApplyDiscountExpirationLogicAsync(producto);
            
            return _mapper.Map<ProductoDTO>(producto);
        }

        public async Task UpdateProducto(int id, ProductoDTO productoDto)
        {
            // Validar lógica de descuento
            ValidateDiscountLogic(productoDto);
            
            var producto = _mapper.Map<Producto>(productoDto);
            producto.Id = id;
            
            // Si es indefinido, limpiar la fecha
            if (producto.descuentoIndefinido)
            {
                producto.fechaExpiracionDescuento = null;
            }
            
            await _productoRepository.UpdateProducto(producto);
        }

        public async Task<object> GetProductosPaginados(int page, int pageSize)
        {
            var result = await _productoRepository.GetProductosPaginados(page, pageSize);
            
            // Aplicar lógica de expiración de descuentos
            foreach (var producto in result.Productos)
            {
                await ApplyDiscountExpirationLogicAsync(producto);
            }
            
            var productos = _mapper.Map<List<ProductoDTO>>(result.Productos);
            
            return new
            {
                productos = productos,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages,
                currentPage = page,
                pageSize = pageSize,
                hasNextPage = page < result.TotalPages,
                hasPreviousPage = page > 1
            };
        }
    }
}
