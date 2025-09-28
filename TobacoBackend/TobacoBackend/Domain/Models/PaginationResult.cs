namespace TobacoBackend.Domain.Models
{
    public class PaginationResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    // Specific pagination results for different entities
    public class ProductoPaginationResult
    {
        public List<Producto> Productos { get; set; } = new List<Producto>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class PedidoPaginationResult
    {
        public List<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class ClientePaginationResult
    {
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
        public int TotalCount { get; set; }
    }
}
