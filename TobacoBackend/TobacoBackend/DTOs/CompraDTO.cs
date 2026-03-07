using System.ComponentModel.DataAnnotations;

namespace TobacoBackend.DTOs
{
    public class ProveedorDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public string? Email { get; set; }
    }

    public class CreateProveedorDTO
    {
        [Required(ErrorMessage = "El nombre del proveedor es requerido")]
        [StringLength(200, MinimumLength = 1)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Contacto { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
    }

    public class CompraItemDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CompraDTO
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public ProveedorDTO? Proveedor { get; set; }
        public DateTime Fecha { get; set; }
        public string? NumeroComprobante { get; set; }
        public string? Observaciones { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CompraItemDTO> Items { get; set; } = new List<CompraItemDTO>();
    }

    /// <summary>Item para crear una compra (producto, cantidad, costo unitario). Subtotal se calcula en backend.</summary>
    public class CreateCompraItemDTO
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [Required(ErrorMessage = "El costo unitario es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El costo unitario no puede ser negativo")]
        public decimal CostoUnitario { get; set; }
    }

    /// <summary>Cabecera + ítems para registrar una compra. Total debe coincidir con suma de ítems.</summary>
    public class CreateCompraDTO
    {
        [Required(ErrorMessage = "El proveedor es requerido")]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [StringLength(100)]
        public string? NumeroComprobante { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un ítem")]
        [MinLength(1, ErrorMessage = "La compra debe tener al menos un producto")]
        public List<CreateCompraItemDTO> Items { get; set; } = new List<CreateCompraItemDTO>();
    }
}
