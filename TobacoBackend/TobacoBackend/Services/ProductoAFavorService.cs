using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class ProductoAFavorService : IProductoAFavorService
    {
        private readonly IProductoAFavorRepository _productoAFavorRepository;
        private readonly IMapper _mapper;

        public ProductoAFavorService(IProductoAFavorRepository productoAFavorRepository, IMapper mapper)
        {
            _productoAFavorRepository = productoAFavorRepository;
            _mapper = mapper;
        }

        public async Task<List<ProductoAFavorDTO>> GetAllProductosAFavor()
        {
            var productos = await _productoAFavorRepository.GetAllProductosAFavor();
            return _mapper.Map<List<ProductoAFavorDTO>>(productos);
        }

        public async Task<ProductoAFavorDTO> GetProductoAFavorById(int id)
        {
            var producto = await _productoAFavorRepository.GetProductoAFavorById(id);
            return _mapper.Map<ProductoAFavorDTO>(producto);
        }

        public async Task<List<ProductoAFavorDTO>> GetProductosAFavorByClienteId(int clienteId, bool? soloNoEntregados = null)
        {
            var productos = await _productoAFavorRepository.GetProductosAFavorByClienteId(clienteId, soloNoEntregados);
            return _mapper.Map<List<ProductoAFavorDTO>>(productos);
        }

        public async Task<List<ProductoAFavorDTO>> GetProductosAFavorByVentaId(int ventaId)
        {
            var productos = await _productoAFavorRepository.GetProductosAFavorByVentaId(ventaId);
            return _mapper.Map<List<ProductoAFavorDTO>>(productos);
        }

        public async Task AddProductoAFavor(ProductoAFavorDTO productoAFavorDto)
        {
            var productoAFavor = _mapper.Map<ProductoAFavor>(productoAFavorDto);
            productoAFavor.FechaRegistro = DateTime.Now;
            await _productoAFavorRepository.AddProductoAFavor(productoAFavor);
        }

        public async Task UpdateProductoAFavor(int id, ProductoAFavorDTO productoAFavorDto)
        {
            var productoAFavor = _mapper.Map<ProductoAFavor>(productoAFavorDto);
            productoAFavor.Id = id;
            await _productoAFavorRepository.UpdateProductoAFavor(productoAFavor);
        }

        public async Task<bool> DeleteProductoAFavor(int id)
        {
            return await _productoAFavorRepository.DeleteProductoAFavor(id);
        }

        public async Task MarcarComoEntregado(int id, int usuarioId)
        {
            var productoAFavor = await _productoAFavorRepository.GetProductoAFavorById(id);
            if (productoAFavor == null)
            {
                throw new Exception($"ProductoAFavor con ID {id} no encontrado.");
            }

            productoAFavor.Entregado = true;
            productoAFavor.FechaEntrega = DateTime.Now;
            productoAFavor.UsuarioEntregaId = usuarioId;

            await _productoAFavorRepository.UpdateProductoAFavor(productoAFavor);
        }
    }
}

