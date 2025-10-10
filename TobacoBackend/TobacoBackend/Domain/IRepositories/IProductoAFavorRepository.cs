using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IProductoAFavorRepository
    {
        Task<List<ProductoAFavor>> GetAllProductosAFavor();
        Task<ProductoAFavor> GetProductoAFavorById(int id);
        Task<List<ProductoAFavor>> GetProductosAFavorByClienteId(int clienteId, bool? soloNoEntregados = null);
        Task<List<ProductoAFavor>> GetProductosAFavorByVentaId(int ventaId);
        Task AddProductoAFavor(ProductoAFavor productoAFavor);
        Task UpdateProductoAFavor(ProductoAFavor productoAFavor);
        Task<bool> DeleteProductoAFavor(int id);
        Task<bool> DeleteProductosAFavorByVentaId(int ventaId);
    }
}

