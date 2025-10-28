using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IAsistenciaRepository
    {
        Task<Asistencia> RegistrarEntradaAsync(Asistencia asistencia);
        Task<Asistencia?> RegistrarSalidaAsync(int asistenciaId, DateTime fechaHoraSalida, string? ubicacionSalida, string? latitudSalida, string? longitudSalida);
        Task<Asistencia?> GetByIdAsync(int id);
        Task<Asistencia?> GetAsistenciaActivaByUserIdAsync(int userId);
        Task<IEnumerable<Asistencia>> GetAsistenciasByUserIdAsync(int userId);
        Task<IEnumerable<Asistencia>> GetAsistenciasByUserIdAndDateRangeAsync(int userId, DateTime fechaInicio, DateTime fechaFin);
        Task<IEnumerable<Asistencia>> GetAllAsistenciasAsync();
        Task<IEnumerable<Asistencia>> GetAsistenciasByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}

