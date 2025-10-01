using System.ComponentModel.DataAnnotations;

namespace TobacoBackend.DTOs
{
    public class ProductQuantityPriceDTO
    {
        public int Id { get; set; }
        
        // ProductId no es requerido para crear precios con un producto nuevo
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(2, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 2")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "El precio total es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio total debe ser mayor a 0")]
        public decimal TotalPrice { get; set; }
        
        // Propiedad calculada para el precio unitario (solo para consulta)
        public decimal UnitPrice => Quantity > 0 ? TotalPrice / Quantity : 0;
    }
}
