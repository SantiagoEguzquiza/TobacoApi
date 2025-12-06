using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class VentaPagoService : IVentaPagoService
    {
        private readonly IVentaPagoRepository _ventaPagoRepository;
        private readonly IMapper _mapper;

        public VentaPagoService(IVentaPagoRepository ventaPagoRepository, IMapper mapper)
        {
            _ventaPagoRepository = ventaPagoRepository;
            _mapper = mapper;
        }

        public async Task<List<VentaPagoDTO>> GetAllVentaPagos()
        {
            var ventaPagos = await _ventaPagoRepository.GetAllVentaPagos();
            return _mapper.Map<List<VentaPagoDTO>>(ventaPagos);
        }

        public async Task<VentaPagoDTO> GetVentaPagoById(int id)
        {
            var ventaPago = await _ventaPagoRepository.GetVentaPagoById(id);
            return _mapper.Map<VentaPagoDTO>(ventaPago);
        }

        public async Task<List<VentaPagoDTO>> GetVentaPagosByVentaId(int ventaId)
        {
            var ventaPagos = await _ventaPagoRepository.GetVentaPagosByVentaId(ventaId);
            return _mapper.Map<List<VentaPagoDTO>>(ventaPagos);
        }

        public async Task<VentaPagoDTO> AddVentaPago(VentaPagoDTO ventaPagoDto)
        {
            var ventaPago = _mapper.Map<Domain.Models.VentaPago>(ventaPagoDto);
            
            // VentaPago no tiene TenantId directamente, se filtra a través de la relación con Venta
            // El TenantId se valida en el repositorio a través de la venta asociada
            
            var result = await _ventaPagoRepository.AddVentaPago(ventaPago);
            return _mapper.Map<VentaPagoDTO>(result);
        }

        public async Task<VentaPagoDTO> UpdateVentaPago(VentaPagoDTO ventaPagoDto)
        {
            var ventaPago = _mapper.Map<Domain.Models.VentaPago>(ventaPagoDto);
            var result = await _ventaPagoRepository.UpdateVentaPago(ventaPago);
            return _mapper.Map<VentaPagoDTO>(result);
        }

        public async Task<bool> DeleteVentaPago(int id)
        {
            return await _ventaPagoRepository.DeleteVentaPago(id);
        }

        public async Task<bool> DeleteVentaPagosByVentaId(int ventaId)
        {
            return await _ventaPagoRepository.DeleteVentaPagosByVentaId(ventaId);
        }
    }
}

