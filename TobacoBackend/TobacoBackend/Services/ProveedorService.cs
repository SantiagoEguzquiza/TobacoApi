using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Persistence;

namespace TobacoBackend.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _proveedorRepository;
        private readonly AplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProveedorService(IProveedorRepository proveedorRepository, AplicationDbContext context, IMapper mapper)
        {
            _proveedorRepository = proveedorRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProveedorDTO>> GetAllAsync()
        {
            var list = await _proveedorRepository.GetAllByTenantAsync();
            return _mapper.Map<List<ProveedorDTO>>(list);
        }

        public async Task<ProveedorDTO> CreateAsync(CreateProveedorDTO dto)
        {
            var tenantId = _context.GetCurrentTenantId();
            if (!tenantId.HasValue)
                throw new InvalidOperationException("No se pudo determinar el tenant.");

            var nombre = dto.Nombre?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(nombre))
                throw new InvalidOperationException("El nombre del proveedor es requerido.");

            if (await _proveedorRepository.ExistsNombreAsync(nombre))
                throw new InvalidOperationException($"Ya existe un proveedor con el nombre '{nombre}'.");

            var proveedor = new Proveedor
            {
                Nombre = nombre,
                Contacto = string.IsNullOrWhiteSpace(dto.Contacto) ? null : dto.Contacto.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                TenantId = tenantId.Value
            };
            var created = await _proveedorRepository.CreateAsync(proveedor);
            return _mapper.Map<ProveedorDTO>(created);
        }
    }
}
