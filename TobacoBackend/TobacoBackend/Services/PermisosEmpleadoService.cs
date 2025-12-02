using AutoMapper;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Services
{
    public class PermisosEmpleadoService : IPermisosEmpleadoService
    {
        private readonly IPermisosEmpleadoRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public PermisosEmpleadoService(
            IPermisosEmpleadoRepository repository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _repository = repository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PermisosEmpleadoDTO?> GetPermisosByUserIdAsync(int userId)
        {
            var permisos = await _repository.GetByUserIdAsync(userId);
            if (permisos == null)
            {
                // Si no existen permisos, crear unos por defecto (todos desactivados)
                permisos = await CreateDefaultPermisosForUser(userId);
            }
            return _mapper.Map<PermisosEmpleadoDTO>(permisos);
        }

        public async Task<PermisosEmpleadoDTO> UpdatePermisosAsync(int userId, UpdatePermisosEmpleadoDTO updateDto)
        {
            var permisos = await _repository.GetByUserIdAsync(userId);
            
            if (permisos == null)
            {
                permisos = await CreateDefaultPermisosForUser(userId);
            }

            // Actualizar solo los permisos que vienen en el DTO
            if (updateDto.Productos_Visualizar.HasValue)
                permisos.Productos_Visualizar = updateDto.Productos_Visualizar.Value;
            if (updateDto.Productos_Crear.HasValue)
                permisos.Productos_Crear = updateDto.Productos_Crear.Value;
            if (updateDto.Productos_Editar.HasValue)
                permisos.Productos_Editar = updateDto.Productos_Editar.Value;
            if (updateDto.Productos_Eliminar.HasValue)
                permisos.Productos_Eliminar = updateDto.Productos_Eliminar.Value;

            if (updateDto.Clientes_Visualizar.HasValue)
                permisos.Clientes_Visualizar = updateDto.Clientes_Visualizar.Value;
            if (updateDto.Clientes_Crear.HasValue)
                permisos.Clientes_Crear = updateDto.Clientes_Crear.Value;
            if (updateDto.Clientes_Editar.HasValue)
                permisos.Clientes_Editar = updateDto.Clientes_Editar.Value;
            if (updateDto.Clientes_Eliminar.HasValue)
                permisos.Clientes_Eliminar = updateDto.Clientes_Eliminar.Value;

            if (updateDto.Ventas_Visualizar.HasValue)
                permisos.Ventas_Visualizar = updateDto.Ventas_Visualizar.Value;
            if (updateDto.Ventas_Crear.HasValue)
                permisos.Ventas_Crear = updateDto.Ventas_Crear.Value;
            if (updateDto.Ventas_EditarBorrador.HasValue)
                permisos.Ventas_EditarBorrador = updateDto.Ventas_EditarBorrador.Value;
            if (updateDto.Ventas_Eliminar.HasValue)
                permisos.Ventas_Eliminar = updateDto.Ventas_Eliminar.Value;

            if (updateDto.CuentaCorriente_Visualizar.HasValue)
                permisos.CuentaCorriente_Visualizar = updateDto.CuentaCorriente_Visualizar.Value;
            if (updateDto.CuentaCorriente_RegistrarAbonos.HasValue)
                permisos.CuentaCorriente_RegistrarAbonos = updateDto.CuentaCorriente_RegistrarAbonos.Value;

            if (updateDto.Entregas_Visualizar.HasValue)
                permisos.Entregas_Visualizar = updateDto.Entregas_Visualizar.Value;
            if (updateDto.Entregas_ActualizarEstado.HasValue)
                permisos.Entregas_ActualizarEstado = updateDto.Entregas_ActualizarEstado.Value;

            var updated = await _repository.UpdateAsync(permisos);
            return _mapper.Map<PermisosEmpleadoDTO>(updated);
        }

        public async Task<PermisosEmpleadoDTO> CreateDefaultPermisosAsync(int userId)
        {
            var permisos = await CreateDefaultPermisosForUser(userId);
            return _mapper.Map<PermisosEmpleadoDTO>(permisos);
        }

        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            // Los admins siempre tienen todos los permisos
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Role == "Admin")
                return true;

            var permisos = await _repository.GetByUserIdAsync(userId);
            if (permisos == null)
                return false; // Sin permisos = sin acceso

            // Mapear el string del permiso a la propiedad correspondiente
            return permission switch
            {
                "Productos_Visualizar" => permisos.Productos_Visualizar,
                "Productos_Crear" => permisos.Productos_Crear,
                "Productos_Editar" => permisos.Productos_Editar,
                "Productos_Eliminar" => permisos.Productos_Eliminar,
                "Clientes_Visualizar" => permisos.Clientes_Visualizar,
                "Clientes_Crear" => permisos.Clientes_Crear,
                "Clientes_Editar" => permisos.Clientes_Editar,
                "Clientes_Eliminar" => permisos.Clientes_Eliminar,
                "Ventas_Visualizar" => permisos.Ventas_Visualizar,
                "Ventas_Crear" => permisos.Ventas_Crear,
                "Ventas_EditarBorrador" => permisos.Ventas_EditarBorrador,
                "Ventas_Eliminar" => permisos.Ventas_Eliminar,
                "CuentaCorriente_Visualizar" => permisos.CuentaCorriente_Visualizar,
                "CuentaCorriente_RegistrarAbonos" => permisos.CuentaCorriente_RegistrarAbonos,
                "Entregas_Visualizar" => permisos.Entregas_Visualizar,
                "Entregas_ActualizarEstado" => permisos.Entregas_ActualizarEstado,
                _ => false
            };
        }

        private async Task<PermisosEmpleado> CreateDefaultPermisosForUser(int userId)
        {
            var permisos = new PermisosEmpleado
            {
                UserId = userId,
                // Todos los permisos comienzan en false por defecto
            };
            return await _repository.CreateAsync(permisos);
        }
    }
}

