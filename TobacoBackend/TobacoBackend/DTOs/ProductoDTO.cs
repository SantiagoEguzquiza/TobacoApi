using System.ComponentModel.DataAnnotations;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.DTOs
{
    public class ProductoDTO
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }
        
        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        public decimal Cantidad { get; set; }
        
        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }
        
        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }         
        
        public string CategoriaNombre { get; set; }   
        
        public bool Half { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
