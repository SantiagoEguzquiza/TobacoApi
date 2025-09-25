namespace TobacoBackend.DTOs
{
    public class PrecioEspecialDTO
    {
        public int? Id { get; set; }
        public int ClienteId { get; set; }
        public int ProductoId { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Propiedades de navegación para mostrar información adicional
        public string? ClienteNombre { get; set; }
        public string? ProductoNombre { get; set; }
        public decimal? PrecioEstandar { get; set; } // Precio estándar del producto
    }
}
