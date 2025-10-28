namespace TobacoBackend.DTOs
{
    public class AsistenciaDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime FechaHoraEntrada { get; set; }
        public DateTime? FechaHoraSalida { get; set; }
        public string? UbicacionEntrada { get; set; }
        public string? UbicacionSalida { get; set; }
        public string? LatitudEntrada { get; set; }
        public string? LongitudEntrada { get; set; }
        public string? LatitudSalida { get; set; }
        public string? LongitudSalida { get; set; }
        public TimeSpan? HorasTrabajadas { get; set; }
    }

    public class RegistrarEntradaDTO
    {
        public int UserId { get; set; }
        public string? UbicacionEntrada { get; set; }
        public string? LatitudEntrada { get; set; }
        public string? LongitudEntrada { get; set; }
    }

    public class RegistrarSalidaDTO
    {
        public int AsistenciaId { get; set; }
        public string? UbicacionSalida { get; set; }
        public string? LatitudSalida { get; set; }
        public string? LongitudSalida { get; set; }
    }
}

