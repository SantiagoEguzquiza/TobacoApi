using System.ComponentModel.DataAnnotations;

namespace TobacoBackend.DTOs
{
    public class ClienteDTO
    {
        public int? Id { get; set; }
        
        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 200 caracteres")]
        public string Direccion { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [RegularExpression(@"^[0-9+\-\s()]+$", ErrorMessage = "El teléfono contiene caracteres inválidos")]
        public string? Telefono { get; set; }
        
        [StringLength(50, ErrorMessage = "La deuda no puede exceder 50 caracteres")]
        [RegularExpression(@"^$|^0$|^\d+(\.\d{1,2})?$", ErrorMessage = "La deuda debe ser un número válido o estar vacía")]
        public string? Deuda { get; set; }
        
        [Range(0, 100, ErrorMessage = "El descuento global debe estar entre 0 y 100")]
        public decimal DescuentoGlobal { get; set; } = 0;
        
        public bool Visible { get; set; } = true;
        
        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90")]
        public double? Latitud { get; set; }
        
        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180")]
        public double? Longitud { get; set; }
    }
}
