namespace TobacoBackend.DTOs
{
    public class CreateVentaResponseDTO
    {
        public int VentaId { get; set; }
        public string Message { get; set; } = "Venta creada exitosamente";
        public bool Asignada { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
    }
}

