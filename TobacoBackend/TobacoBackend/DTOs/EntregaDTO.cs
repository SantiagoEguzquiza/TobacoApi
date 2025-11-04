namespace TobacoBackend.DTOs
{
    public class EntregaDTO
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = "";
        public string ClienteDireccion { get; set; } = "";
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public int Estado { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public int RepartidorId { get; set; }
        public int Orden { get; set; }
        public string Notas { get; set; } = "";
        public double DistanciaDesdeUbicacionActual { get; set; }
    }
}
