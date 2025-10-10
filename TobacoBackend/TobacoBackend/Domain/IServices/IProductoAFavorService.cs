using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IProductoAFavorService
    {
        Task<List<ProductoAFavorDTO>> GetAllProductosAFavor();
        Task<ProductoAFavorDTO> GetProductoAFavorById(int id);
        Task<List<ProductoAFavorDTO>> GetProductosAFavorByClienteId(int clienteId, bool? soloNoEntregados = null);
        Task<List<ProductoAFavorDTO>> GetProductosAFavorByVentaId(int ventaId);
        Task AddProductoAFavor(ProductoAFavorDTO productoAFavorDto);
        Task UpdateProductoAFavor(int id, ProductoAFavorDTO productoAFavorDto);
        Task<bool> DeleteProductoAFavor(int id);
        Task MarcarComoEntregado(int id, int usuarioId);
    }
}

