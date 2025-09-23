namespace TobacoBackend.DTOs
{
    public class CategoriaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ColorHex { get; set; } = "#9E9E9E"; // Default gray color
    }
}
