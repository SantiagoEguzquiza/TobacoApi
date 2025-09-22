using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class VentaPagosService : IVentaPagosService
    {
        private readonly IVentaPagosRepository _ventaPagosRepository;
        private readonly IMapper _mapper;

        public VentaPagosService(IVentaPagosRepository ventaPagosRepository, IMapper mapper)
        {
            _ventaPagosRepository = ventaPagosRepository;
            _mapper = mapper;
        }

        public async Task<List<VentaPagosDTO>> GetAllVentaPagos()
        {
            var ventaPagos = await _ventaPagosRepository.GetAllVentaPagos();
            return _mapper.Map<List<VentaPagosDTO>>(ventaPagos);
        }

        public async Task<VentaPagosDTO> GetVentaPagosById(int id)
        {
            var ventaPagos = await _ventaPagosRepository.GetVentaPagosById(id);
            return _mapper.Map<VentaPagosDTO>(ventaPagos);
        }

        public async Task<List<VentaPagosDTO>> GetVentaPagosByPedidoId(int pedidoId)
        {
            var ventaPagos = await _ventaPagosRepository.GetVentaPagosByPedidoId(pedidoId);
            return _mapper.Map<List<VentaPagosDTO>>(ventaPagos);
        }

        public async Task<VentaPagosDTO> AddVentaPagos(VentaPagosDTO ventaPagosDto)
        {
            var ventaPagos = _mapper.Map<Domain.Models.VentaPagos>(ventaPagosDto);
            var result = await _ventaPagosRepository.AddVentaPagos(ventaPagos);
            return _mapper.Map<VentaPagosDTO>(result);
        }

        public async Task<VentaPagosDTO> UpdateVentaPagos(VentaPagosDTO ventaPagosDto)
        {
            var ventaPagos = _mapper.Map<Domain.Models.VentaPagos>(ventaPagosDto);
            var result = await _ventaPagosRepository.UpdateVentaPagos(ventaPagos);
            return _mapper.Map<VentaPagosDTO>(result);
        }

        public async Task<bool> DeleteVentaPagos(int id)
        {
            return await _ventaPagosRepository.DeleteVentaPagos(id);
        }

        public async Task<bool> DeleteVentaPagosByPedidoId(int pedidoId)
        {
            return await _ventaPagosRepository.DeleteVentaPagosByPedidoId(pedidoId);
        }
    }
}
