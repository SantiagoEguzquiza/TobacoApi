using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class RecorridoProgramadoService : IRecorridoProgramadoService
    {
        private readonly IRecorridoProgramadoRepository _repository;

        public RecorridoProgramadoService(IRecorridoProgramadoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RecorridoProgramadoDTO>> GetRecorridosByVendedorAndDia(int vendedorId, int diaSemana)
        {
            // Validar que el valor del día sea válido
            if (!Enum.IsDefined(typeof(DiaSemana), diaSemana))
            {
                throw new ArgumentException($"Día de la semana inválido: {diaSemana}");
            }
            
            var diaSemanaEnum = (DiaSemana)diaSemana;
            var recorridos = await _repository.GetRecorridosByVendedorAndDia(vendedorId, diaSemanaEnum);
            return recorridos.Select(ConvertToDTO).ToList();
        }

        public async Task<List<RecorridoProgramadoDTO>> GetRecorridosByVendedor(int vendedorId)
        {
            var recorridos = await _repository.GetRecorridosByVendedor(vendedorId);
            return recorridos.Select(ConvertToDTO).ToList();
        }

        public async Task<RecorridoProgramadoDTO?> GetById(int id)
        {
            var recorrido = await _repository.GetById(id);
            return recorrido != null ? ConvertToDTO(recorrido) : null;
        }

        public async Task<RecorridoProgramadoDTO> Create(CreateRecorridoProgramadoDTO dto)
        {
            // Asegurar que el día de la semana sea válido
            if (!Enum.IsDefined(typeof(DiaSemana), dto.DiaSemana))
            {
                throw new ArgumentException($"Día de la semana inválido: {dto.DiaSemana}");
            }

            var recorrido = new RecorridoProgramado
            {
                VendedorId = dto.VendedorId,
                ClienteId = dto.ClienteId,
                DiaSemana = dto.DiaSemana,
                Orden = dto.Orden,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            await _repository.Add(recorrido);
            await _repository.SaveChangesAsync();

            var created = await _repository.GetById(recorrido.Id);
            return ConvertToDTO(created!);
        }

        public async Task<RecorridoProgramadoDTO?> Update(int id, UpdateRecorridoProgramadoDTO dto)
        {
            var recorrido = await _repository.GetById(id);
            if (recorrido == null) return null;

            if (dto.ClienteId.HasValue) recorrido.ClienteId = dto.ClienteId.Value;
            if (dto.DiaSemana.HasValue) recorrido.DiaSemana = dto.DiaSemana.Value;
            if (dto.Orden.HasValue) recorrido.Orden = dto.Orden.Value;
            if (dto.Activo.HasValue) recorrido.Activo = dto.Activo.Value;

            await _repository.Update(recorrido);
            await _repository.SaveChangesAsync();

            var updated = await _repository.GetById(id);
            return updated != null ? ConvertToDTO(updated) : null;
        }

        public async Task<bool> Delete(int id)
        {
            await _repository.Delete(id);
            await _repository.SaveChangesAsync();
            return true;
        }

        private RecorridoProgramadoDTO ConvertToDTO(RecorridoProgramado recorrido)
        {
            return new RecorridoProgramadoDTO
            {
                Id = recorrido.Id,
                VendedorId = recorrido.VendedorId,
                VendedorNombre = recorrido.Vendedor?.UserName,
                ClienteId = recorrido.ClienteId,
                ClienteNombre = recorrido.Cliente?.Nombre,
                ClienteDireccion = recorrido.Cliente?.Direccion,
                ClienteLatitud = recorrido.Cliente?.Latitud,
                ClienteLongitud = recorrido.Cliente?.Longitud,
                DiaSemana = recorrido.DiaSemana,
                Orden = recorrido.Orden,
                Activo = recorrido.Activo
            };
        }
    }
}

