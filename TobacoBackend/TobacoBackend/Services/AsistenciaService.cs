using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class AsistenciaService : IAsistenciaService
    {
        private readonly IAsistenciaRepository _asistenciaRepository;
        private readonly IMapper _mapper;

        public AsistenciaService(IAsistenciaRepository asistenciaRepository, IMapper mapper)
        {
            _asistenciaRepository = asistenciaRepository;
            _mapper = mapper;
        }

        public async Task<AsistenciaDTO> RegistrarEntradaAsync(RegistrarEntradaDTO registrarEntradaDto)
        {
            // Verificar si el usuario ya tiene una asistencia activa (sin salida)
            var asistenciaActiva = await _asistenciaRepository.GetAsistenciaActivaByUserIdAsync(registrarEntradaDto.UserId);
            if (asistenciaActiva != null)
            {
                throw new InvalidOperationException("El usuario ya tiene una asistencia activa. Debe registrar la salida primero.");
            }

            var asistencia = new Asistencia
            {
                UserId = registrarEntradaDto.UserId,
                FechaHoraEntrada = DateTime.UtcNow,
                UbicacionEntrada = registrarEntradaDto.UbicacionEntrada,
                LatitudEntrada = registrarEntradaDto.LatitudEntrada,
                LongitudEntrada = registrarEntradaDto.LongitudEntrada
            };

            var asistenciaCreada = await _asistenciaRepository.RegistrarEntradaAsync(asistencia);
            
            // Cargar el usuario para el mapeo
            var asistenciaConUsuario = await _asistenciaRepository.GetByIdAsync(asistenciaCreada.Id);
            
            return _mapper.Map<AsistenciaDTO>(asistenciaConUsuario);
        }

        public async Task<AsistenciaDTO?> RegistrarSalidaAsync(RegistrarSalidaDTO registrarSalidaDto)
        {
            var asistencia = await _asistenciaRepository.RegistrarSalidaAsync(
                registrarSalidaDto.AsistenciaId,
                DateTime.UtcNow,
                registrarSalidaDto.UbicacionSalida,
                registrarSalidaDto.LatitudSalida,
                registrarSalidaDto.LongitudSalida
            );

            if (asistencia == null)
                return null;

            return _mapper.Map<AsistenciaDTO>(asistencia);
        }

        public async Task<AsistenciaDTO?> GetAsistenciaActivaByUserIdAsync(int userId)
        {
            var asistencia = await _asistenciaRepository.GetAsistenciaActivaByUserIdAsync(userId);
            return asistencia != null ? _mapper.Map<AsistenciaDTO>(asistencia) : null;
        }

        public async Task<IEnumerable<AsistenciaDTO>> GetAsistenciasByUserIdAsync(int userId)
        {
            var asistencias = await _asistenciaRepository.GetAsistenciasByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<AsistenciaDTO>>(asistencias);
        }

        public async Task<IEnumerable<AsistenciaDTO>> GetAsistenciasByUserIdAndDateRangeAsync(int userId, DateTime fechaInicio, DateTime fechaFin)
        {
            var asistencias = await _asistenciaRepository.GetAsistenciasByUserIdAndDateRangeAsync(userId, fechaInicio, fechaFin);
            return _mapper.Map<IEnumerable<AsistenciaDTO>>(asistencias);
        }

        public async Task<IEnumerable<AsistenciaDTO>> GetAllAsistenciasAsync()
        {
            var asistencias = await _asistenciaRepository.GetAllAsistenciasAsync();
            return _mapper.Map<IEnumerable<AsistenciaDTO>>(asistencias);
        }

        public async Task<IEnumerable<AsistenciaDTO>> GetAsistenciasByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var asistencias = await _asistenciaRepository.GetAsistenciasByDateRangeAsync(fechaInicio, fechaFin);
            return _mapper.Map<IEnumerable<AsistenciaDTO>>(asistencias);
        }
    }
}

