using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IMapper _mapper;

        public TenantService(ITenantRepository tenantRepository, IMapper mapper)
        {
            _tenantRepository = tenantRepository;
            _mapper = mapper;
        }

        public async Task<List<TenantDTO>> GetAllTenantsAsync()
        {
            var tenants = await _tenantRepository.GetAllTenantsAsync();
            return _mapper.Map<List<TenantDTO>>(tenants);
        }

        public async Task<TenantDTO?> GetTenantByIdAsync(int id)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(id);
            return tenant != null ? _mapper.Map<TenantDTO>(tenant) : null;
        }

        public async Task<TenantDTO> CreateTenantAsync(CreateTenantDTO createTenantDto)
        {
            // Validar que el nombre no exista
            var nameExists = await _tenantRepository.TenantNameExistsAsync(createTenantDto.Nombre);
            if (nameExists)
            {
                throw new InvalidOperationException($"Ya existe un tenant con el nombre '{createTenantDto.Nombre}'.");
            }

            var tenant = _mapper.Map<Tenant>(createTenantDto);
            tenant.CreatedAt = DateTime.UtcNow;
            tenant.IsActive = true;

            var tenantCreado = await _tenantRepository.CreateTenantAsync(tenant);
            return _mapper.Map<TenantDTO>(tenantCreado);
        }

        public async Task<TenantDTO> UpdateTenantAsync(int id, UpdateTenantDTO updateTenantDto)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(id);
            if (tenant == null)
            {
                throw new InvalidOperationException($"Tenant con ID {id} no encontrado.");
            }

            // Validar que el nombre no exista en otro tenant
            if (!string.IsNullOrEmpty(updateTenantDto.Nombre) && updateTenantDto.Nombre != tenant.Nombre)
            {
                var nameExists = await _tenantRepository.TenantNameExistsAsync(updateTenantDto.Nombre, id);
                if (nameExists)
                {
                    throw new InvalidOperationException($"Ya existe un tenant con el nombre '{updateTenantDto.Nombre}'.");
                }
            }

            // Actualizar propiedades
            if (!string.IsNullOrEmpty(updateTenantDto.Nombre))
                tenant.Nombre = updateTenantDto.Nombre;
            
            if (updateTenantDto.Descripcion != null)
                tenant.Descripcion = updateTenantDto.Descripcion;
            
            if (!string.IsNullOrEmpty(updateTenantDto.Email))
                tenant.Email = updateTenantDto.Email;
            
            if (!string.IsNullOrEmpty(updateTenantDto.Telefono))
                tenant.Telefono = updateTenantDto.Telefono;
            
            if (updateTenantDto.IsActive.HasValue)
                tenant.IsActive = updateTenantDto.IsActive.Value;

            tenant.UpdatedAt = DateTime.UtcNow;

            var tenantActualizado = await _tenantRepository.UpdateTenantAsync(tenant);
            return _mapper.Map<TenantDTO>(tenantActualizado);
        }

        public async Task<bool> DeleteTenantAsync(int id)
        {
            return await _tenantRepository.DeleteTenantAsync(id);
        }
    }
}
