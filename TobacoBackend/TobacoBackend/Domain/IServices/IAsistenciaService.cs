using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IAsistenciaService
    {
        Task<AsistenciaDTO> RegistrarEntradaAsync(RegistrarEntradaDTO registrarEntradaDto);
        Task<AsistenciaDTO?> RegistrarSalidaAsync(RegistrarSalidaDTO registrarSalidaDto);
        Task<AsistenciaDTO?> GetAsistenciaActivaByUserIdAsync(int userId);
        Task<IEnumerable<AsistenciaDTO>> GetAsistenciasByUserIdAsync(int userId);
        Task<IEnumerable<AsistenciaDTO>> GetAsistenciasByUserIdAndDateRangeAsync(int userId, DateTime fechaInicio, DateTime fechaFin);
        Task<IEnumerable<AsistenciaDTO>> GetAllAsistenciasAsync();
        Task<IEnumerable<AsistenciaDTO>> GetAsistenciasByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}

