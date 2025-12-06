using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.DTOs;
using TobacoBackend.Authorization;
using TobacoBackend.Helpers;
using TobacoBackend.Services;
using TobacoBackend.Domain.Models;
using AutoMapper;
using System.Security.Claims;

namespace TobacoBackend.Controllers
{
    /// <summary>
    /// Controlador exclusivo para SuperAdmin
    /// Solo gestiona tenants y administradores, NO accede a datos de clientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdminOnly)]
    public class SuperAdminController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly AuditService _auditService;

        public SuperAdminController(
            ITenantService tenantService,
            IUserService userService,
            ITenantRepository tenantRepository,
            IUserRepository userRepository,
            TokenService tokenService,
            IMapper mapper,
            AuditService auditService)
        {
            _tenantService = tenantService;
            _userService = userService;
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _auditService = auditService;
        }

        #region Tenants Management

        /// <summary>
        /// Obtiene todos los tenants
        /// </summary>
        [HttpGet("tenants")]
        public async Task<ActionResult<List<TenantDTO>>> GetAllTenants()
        {
            try
            {
                var tenants = await _tenantService.GetAllTenantsAsync();
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener los tenants: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene un tenant por ID
        /// </summary>
        [HttpGet("tenants/{id}")]
        public async Task<ActionResult<TenantDTO>> GetTenantById(int id)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByIdAsync(id);

                if (tenant == null)
                    return NotFound(new { message = "Tenant no encontrado." });

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener el tenant: {ex.Message}" });
            }
        }

        /// <summary>
        /// Crea un nuevo tenant
        /// </summary>
        [HttpPost("tenants")]
        public async Task<ActionResult<TenantDTO>> CreateTenant([FromBody] CreateTenantDTO createTenantDto)
        {
            try
            {
                if (createTenantDto == null)
                    return BadRequest(new { message = "Los datos del tenant no pueden ser nulos." });

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos del tenant inválidos.", errors = ModelState });
                }

                createTenantDto.Nombre = InputSanitizer.SanitizeString(createTenantDto.Nombre);
                if (!string.IsNullOrEmpty(createTenantDto.Descripcion))
                    createTenantDto.Descripcion = InputSanitizer.SanitizeString(createTenantDto.Descripcion);

                if (InputSanitizer.ContainsSqlInjection(createTenantDto.Nombre) ||
                    InputSanitizer.ContainsXss(createTenantDto.Nombre))
                {
                    return BadRequest(new { message = "Entrada inválida detectada." });
                }

                var tenant = await _tenantService.CreateTenantAsync(createTenantDto);

                _auditService.LogCreate("Tenant", tenant.Id, User,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, tenant);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al crear el tenant: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza un tenant
        /// </summary>
        [HttpPut("tenants/{id}")]
        public async Task<ActionResult<TenantDTO>> UpdateTenant(int id, [FromBody] UpdateTenantDTO updateTenantDto)
        {
            try
            {
                if (updateTenantDto == null)
                    return BadRequest(new { message = "Los datos de actualización no pueden ser nulos." });

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de actualización inválidos.", errors = ModelState });
                }

                if (!string.IsNullOrEmpty(updateTenantDto.Nombre))
                {
                    updateTenantDto.Nombre = InputSanitizer.SanitizeString(updateTenantDto.Nombre);
                    if (InputSanitizer.ContainsSqlInjection(updateTenantDto.Nombre) ||
                        InputSanitizer.ContainsXss(updateTenantDto.Nombre))
                    {
                        return BadRequest(new { message = "Entrada inválida detectada." });
                    }
                }

                var tenant = await _tenantService.UpdateTenantAsync(id, updateTenantDto);

                _auditService.LogUpdate("Tenant", id, User, null,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return Ok(tenant);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al actualizar el tenant: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina un tenant (solo si no tiene usuarios asociados)
        /// </summary>
        [HttpDelete("tenants/{id}")]
        public async Task<ActionResult> DeleteTenant(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "ID de tenant inválido." });
                }

                var success = await _tenantService.DeleteTenantAsync(id);

                if (!success)
                    return NotFound(new { message = "Tenant no encontrado." });

                _auditService.LogDelete("Tenant", id, User,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return Ok(new { message = "Tenant eliminado exitosamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al eliminar el tenant: {ex.Message}" });
            }
        }

        #endregion

        #region Admin Users Management

        /// <summary>
        /// Obtiene todos los administradores de un tenant específico
        /// </summary>
        [HttpGet("tenants/{tenantId}/admins")]
        public async Task<ActionResult<List<UserDTO>>> GetTenantAdmins(int tenantId)
        {
            try
            {
                // Verificar que el tenant existe
                var tenantExists = await _tenantRepository.TenantExistsAsync(tenantId);
                if (!tenantExists)
                {
                    return NotFound(new { message = "Tenant no encontrado." });
                }

                // Para SuperAdmin, obtener usuarios directamente del tenant específico
                // sin usar el filtro automático del contexto
                var users = await _userRepository.GetUsersByTenantIdAsync(tenantId);
                var allUsers = users.ToList();
                var admins = allUsers
                    .Where(u => u.Role == "Admin")
                    .ToList();

                // Log para debugging
                Console.WriteLine($"SuperAdminController: TenantId={tenantId}, Total usuarios={allUsers.Count}, Admins={admins.Count}");

                return Ok(_mapper.Map<List<UserDTO>>(admins));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SuperAdminController: Error al obtener administradores: {ex.Message}");
                Console.WriteLine($"SuperAdminController: Stack trace: {ex.StackTrace}");
                return BadRequest(new { message = $"Error al obtener los administradores: {ex.Message}" });
            }
        }

        /// <summary>
        /// Crea un administrador para un tenant específico
        /// </summary>
        [HttpPost("tenants/{tenantId}/admins")]
        public async Task<ActionResult<UserDTO>> CreateTenantAdmin(int tenantId, [FromBody] CreateUserDTO createUserDto)
        {
            try
            {
                // Verificar que el tenant existe
                var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId);
                if (tenant == null)
                {
                    return NotFound(new { message = "Tenant no encontrado." });
                }

                if (createUserDto == null)
                    return BadRequest(new { message = "Los datos del usuario no pueden ser nulos." });

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos del usuario inválidos.", errors = ModelState });
                }

                // Forzar que sea Admin y asignar el tenant
                createUserDto.Role = "Admin";
                createUserDto.UserName = InputSanitizer.SanitizeUserName(createUserDto.UserName);

                if (InputSanitizer.ContainsSqlInjection(createUserDto.UserName) ||
                    InputSanitizer.ContainsXss(createUserDto.UserName))
                {
                    return BadRequest(new { message = "Entrada inválida detectada." });
                }

                // Verificar que el nombre de usuario no exista en ningún tenant
                var userNameExists = await _userRepository.ExistsAsync(createUserDto.UserName);
                if (userNameExists)
                {
                    return BadRequest(new { message = $"El nombre de usuario '{createUserDto.UserName}' ya está en uso." });
                }

                // Crear el usuario directamente con el TenantId
                var user = _mapper.Map<User>(createUserDto);
                user.TenantId = tenantId;
                user.Password = UserService.HashPasswordForStorage(createUserDto.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
                user.Plan = PlanType.FREE;

                await _userRepository.AddAsync(user);

                var userDto = _mapper.Map<UserDTO>(user);

                _auditService.LogCreate("Admin (SuperAdmin)", user.Id, User,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return CreatedAtAction(nameof(GetTenantAdmins), new { tenantId = tenantId }, userDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al crear el administrador: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza un administrador de un tenant
        /// </summary>
        [HttpPut("tenants/{tenantId}/admins/{adminId}")]
        public async Task<ActionResult<UserDTO>> UpdateTenantAdmin(int tenantId, int adminId, [FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                // Verificar que el tenant existe
                var tenantExists = await _tenantRepository.TenantExistsAsync(tenantId);
                if (!tenantExists)
                {
                    return NotFound(new { message = "Tenant no encontrado." });
                }

                // Para SuperAdmin, obtener el usuario directamente sin filtro de tenant
                var user = await _userRepository.GetByIdWithoutTenantFilterAsync(adminId);
                if (user == null || user.TenantId != tenantId || user.Role != "Admin")
                {
                    return NotFound(new { message = "Administrador no encontrado en este tenant." });
                }

                if (updateUserDto == null)
                    return BadRequest(new { message = "Los datos de actualización no pueden ser nulos." });

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de actualización inválidos.", errors = ModelState });
                }

                // Sanitizar si se actualiza el nombre de usuario
                if (!string.IsNullOrEmpty(updateUserDto.UserName))
                {
                    updateUserDto.UserName = InputSanitizer.SanitizeUserName(updateUserDto.UserName);

                    if (InputSanitizer.ContainsSqlInjection(updateUserDto.UserName) ||
                        InputSanitizer.ContainsXss(updateUserDto.UserName))
                    {
                        return BadRequest(new { message = "Entrada inválida detectada." });
                    }

                    // Verificar que el nuevo nombre no exista en otro usuario
                    var existingUser = await _userRepository.GetByUserNameAsync(updateUserDto.UserName);
                    if (existingUser != null && existingUser.Id != adminId)
                    {
                        return BadRequest(new { message = $"El nombre de usuario '{updateUserDto.UserName}' ya está en uso." });
                    }
                }

                // Actualizar el usuario
                if (!string.IsNullOrEmpty(updateUserDto.UserName))
                    user.UserName = updateUserDto.UserName;

                if (!string.IsNullOrEmpty(updateUserDto.Email))
                    user.Email = updateUserDto.Email;

                if (!string.IsNullOrEmpty(updateUserDto.Password))
                    user.Password = UserService.HashPasswordForStorage(updateUserDto.Password);

                if (updateUserDto.IsActive.HasValue)
                    user.IsActive = updateUserDto.IsActive.Value;

                await _userRepository.UpdateAsync(user);

                _auditService.LogUpdate("Admin (SuperAdmin)", adminId, User, null,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return Ok(_mapper.Map<UserDTO>(user));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al actualizar el administrador: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina un administrador de un tenant
        /// </summary>
        [HttpDelete("tenants/{tenantId}/admins/{adminId}")]
        public async Task<ActionResult> DeleteTenantAdmin(int tenantId, int adminId)
        {
            try
            {
                // Verificar que el tenant existe
                var tenantExists = await _tenantRepository.TenantExistsAsync(tenantId);
                if (!tenantExists)
                {
                    return NotFound(new { message = "Tenant no encontrado." });
                }

                // Para SuperAdmin, obtener el usuario directamente sin filtro de tenant
                var user = await _userRepository.GetByIdWithoutTenantFilterAsync(adminId);
                if (user == null || user.TenantId != tenantId || user.Role != "Admin")
                {
                    return NotFound(new { message = "Administrador no encontrado en este tenant." });
                }

                // Verificar que no sea el último admin del tenant
                var users = await _userRepository.GetUsersByTenantIdAsync(tenantId);
                var tenantAdmins = users.Where(u => u.Role == "Admin" && u.IsActive).ToList();
                if (tenantAdmins.Count <= 1)
                {
                    return BadRequest(new { message = "No se puede eliminar el último administrador activo del tenant." });
                }

                // Para SuperAdmin, eliminar directamente sin filtro de tenant
                await _userRepository.DeleteAsyncWithoutTenantFilter(adminId);

                _auditService.LogDelete("Admin (SuperAdmin)", adminId, User,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return Ok(new { message = "Administrador eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al eliminar el administrador: {ex.Message}" });
            }
        }

        #endregion
    }
}

