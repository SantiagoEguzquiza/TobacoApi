using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace TobacoBackend.Services
{
    public class AbonosService : IAbonosService
    {
        private readonly IAbonosRepository _abonosRepository;
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteService;
        private readonly AplicationDbContext _context;
        
        public AbonosService(IAbonosRepository abonosRepository, IMapper mapper, IClienteService clienteService, AplicationDbContext context)
        {
            _abonosRepository = abonosRepository;
            _mapper = mapper;
            _clienteService = clienteService;
            _context = context;
        }

        public async Task<AbonoDTO> AddAbono(AbonoDTO abonoDto)
        {
            // Validar que el monto del abono sea válido
            var montoAbono = decimal.TryParse(abonoDto.Monto, out var monto) ? monto : 0;
            
            if (montoAbono <= 0)
            {
                throw new Exception("El monto del abono debe ser mayor a cero");
            }

            // Validar que el cliente tenga suficiente deuda para el abono
            if (!await _clienteService.ValidarMontoAbono(abonoDto.ClienteId, montoAbono))
            {
                throw new Exception("El monto del abono no puede ser mayor a la deuda del cliente");
            }

            var abono = _mapper.Map<Abonos>(abonoDto);
            if (string.IsNullOrEmpty(abono.Nota))
                abono.Nota = "";

            // PostgreSQL timestamp with time zone requiere UTC
            if (abono.Fecha.Kind != DateTimeKind.Utc)
                abono.Fecha = abono.Fecha.Kind == DateTimeKind.Local
                    ? abono.Fecha.ToUniversalTime()
                    : DateTime.SpecifyKind(abono.Fecha, DateTimeKind.Utc);

            // Set TenantId from current context
            var tenantId = _context.GetCurrentTenantId();
            if (!tenantId.HasValue)
            {
                throw new InvalidOperationException("No se pudo determinar el TenantId del contexto actual.");
            }
            abono.TenantId = tenantId.Value;
            
            var abonoCreado = await _abonosRepository.AddAbono(abono);
            
            // Reducir la deuda del cliente
            await _clienteService.ReducirDeuda(abonoDto.ClienteId, montoAbono);
            
            return _mapper.Map<AbonoDTO>(abonoCreado);
        }

        public async Task<bool> DeleteAbono(int id)
        {
            // Primero obtener la información del abono antes de eliminarlo
            var abono = await _abonosRepository.GetAbonoById(id);
            if (abono == null)
            {
                return false;
            }

            // Obtener el monto del abono para restaurar la deuda
            var montoAbono = decimal.TryParse(abono.Monto, out var monto) ? monto : 0;
            
            // Eliminar el abono
            bool eliminado = await _abonosRepository.DeleteAbono(id);
            
            if (eliminado && montoAbono > 0)
            {
                // Restaurar la deuda del cliente (agregar el monto del abono eliminado)
                await _clienteService.AgregarDeuda(abono.ClienteId, montoAbono);
            }
            
            return eliminado;
        }

        public async Task<AbonoDTO> GetAbonoById(int id)
        {
            var abono = await _abonosRepository.GetAbonoById(id);
            return _mapper.Map<AbonoDTO>(abono);
        }

        public async Task<List<AbonoDTO>> GetAllAbonos()
        {
            var abonos = await _abonosRepository.GetAllAbonos();
            return _mapper.Map<List<AbonoDTO>>(abonos);
        }

        public async Task UpdateAbono(int id, AbonoDTO abonoDto)
        {
            var abono = _mapper.Map<Abonos>(abonoDto);
            abono.Id = id;
            if (abono.Fecha.Kind != DateTimeKind.Utc)
                abono.Fecha = abono.Fecha.Kind == DateTimeKind.Local
                    ? abono.Fecha.ToUniversalTime()
                    : DateTime.SpecifyKind(abono.Fecha, DateTimeKind.Utc);
            await _abonosRepository.UpdateAbono(abono);
        }

        public async Task<List<AbonoDTO>> GetAbonosByClienteId(int clienteId)
        {
            var abonos = await _abonosRepository.GetAbonosByClienteId(clienteId);
            return _mapper.Map<List<AbonoDTO>>(abonos);
        }
    }
}
