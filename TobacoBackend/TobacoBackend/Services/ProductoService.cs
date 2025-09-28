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

        public async Task AddProducto(ProductoDTO productoDto)
        {
            var producto = _mapper.Map<Producto>(productoDto);
            await _productoRepository.AddProducto(producto);
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

        public async Task<List<ProductoDTO>> GetAllProductos()
        {
            var productos = await _productoRepository.GetAllProductos();
            return _mapper.Map<List<ProductoDTO>>(productos);
        }

        public async Task<ProductoDTO> GetProductoById(int id)
        {
            var producto = await _productoRepository.GetProductoById(id);
            return _mapper.Map<ProductoDTO>(producto);
        }

        public async Task UpdateProducto(int id, ProductoDTO productoDto)
        {
            var producto = _mapper.Map<Producto>(productoDto);
            producto.Id = id;
            await _productoRepository.UpdateProducto(producto);
        }

        public async Task<object> GetProductosPaginados(int page, int pageSize)
        {
            var result = await _productoRepository.GetProductosPaginados(page, pageSize);
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
