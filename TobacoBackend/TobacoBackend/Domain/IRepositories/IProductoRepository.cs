using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IProductoRepository
    {
        Task<List<Producto>> GetAllProductos();
        Task<Producto> GetProductoById(int id);
        Task AddProducto(Producto producto);
        Task UpdateProducto(Producto producto);
        Task<bool> DeleteProducto(int id);
        Task<bool> SoftDeleteProducto(int id);
        Task<bool> ActivateProducto(int id);
        Task<ProductoPaginationResult> GetProductosPaginados(int page, int pageSize);
        Task UpdateProductoDiscount(int id, decimal descuento, DateTime? fechaExpiracionDescuento, bool descuentoIndefinido);
    }
}
