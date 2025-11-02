using TobacoBackend.Domain.Models;

namespace TobacoBackend.DTOs
{
    public class RecorridoProgramadoDTO
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public string? VendedorNombre { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ClienteDireccion { get; set; }
        public double? ClienteLatitud { get; set; }
        public double? ClienteLongitud { get; set; }
        public DiaSemana DiaSemana { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }

    public class CreateRecorridoProgramadoDTO
    {
        public int VendedorId { get; set; }
        public int ClienteId { get; set; }
        public DiaSemana DiaSemana { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class UpdateRecorridoProgramadoDTO
    {
        public int? ClienteId { get; set; }
        public DiaSemana? DiaSemana { get; set; }
        public int? Orden { get; set; }
        public bool? Activo { get; set; }
    }
}
