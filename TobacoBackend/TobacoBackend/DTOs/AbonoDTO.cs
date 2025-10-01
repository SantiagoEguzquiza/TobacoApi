namespace TobacoBackend.DTOs
{
    public class AbonoDTO
    {
        public int? Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public string Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Nota { get; set; }
    }
}
