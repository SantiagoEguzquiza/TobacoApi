namespace TobacoBackend.DTOs
{
    public class SaldarDeudaDTO
    {
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Nota { get; set; }
    }
}
