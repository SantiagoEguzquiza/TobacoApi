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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdminOrAdmin)] // SuperAdmin o Admin pueden gestionar tenants
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly AuditService _auditService;

        public TenantController(
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

        /// <summary>
        /// Endpoint público para onboarding de nuevos clientes
        /// Crea un tenant y su primer usuario administrador
        /// </summary>
        [HttpPost("onboarding")]
        [AllowAnonymous] // Endpoint público para registro de nuevos clientes
        public async Task<ActionResult<OnboardingResponseDTO>> Onboarding([FromBody] OnboardingRequestDTO request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Los datos de onboarding no pueden ser nulos." });

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de onboarding inválidos.", errors = ModelState });
                }

                // Sanitizar entrada
                request.EmpresaNombre = InputSanitizer.SanitizeString(request.EmpresaNombre);
                request.AdminUserName = InputSanitizer.SanitizeUserName(request.AdminUserName);

                if (InputSanitizer.ContainsSqlInjection(request.EmpresaNombre) || 
                    InputSanitizer.ContainsXss(request.EmpresaNombre) ||
                    InputSanitizer.ContainsSqlInjection(request.AdminUserName) || 
                    InputSanitizer.ContainsXss(request.AdminUserName))
                {
                    return BadRequest(new { message = "Entrada inválida detectada." });
                }

                // Validar que el nombre del tenant no exista
                var tenantNameExists = await _tenantRepository.TenantNameExistsAsync(request.EmpresaNombre);
                if (tenantNameExists)
                {
                    return BadRequest(new { message = $"Ya existe un tenant con el nombre '{request.EmpresaNombre}'." });
                }

                // Validar que el nombre de usuario no exista (en cualquier tenant)
                var userNameExists = await _userRepository.ExistsAsync(request.AdminUserName);
                if (userNameExists)
                {
                    return BadRequest(new { message = $"El nombre de usuario '{request.AdminUserName}' ya está en uso." });
                }

                // Validar política de contraseña
                if (!Helpers.PasswordPolicy.IsValid(request.AdminPassword, out string? passwordError))
                {
                    return BadRequest(new { message = passwordError ?? "La contraseña no cumple con los requisitos de seguridad." });
                }

                // Crear el tenant
                var createTenantDto = new CreateTenantDTO
                {
                    Nombre = request.EmpresaNombre,
                    Descripcion = request.EmpresaDescripcion,
                    Email = request.EmpresaEmail,
                    Telefono = request.EmpresaTelefono
                };

                var tenant = await _tenantService.CreateTenantAsync(createTenantDto);

                // Crear el usuario administrador para este tenant
                var createUserDto = new CreateUserDTO
                {
                    UserName = request.AdminUserName,
                    Password = request.AdminPassword,
                    Email = request.AdminEmail,
                    Role = "Admin"
                };

                // Necesitamos crear el usuario directamente con el TenantId
                var user = _mapper.Map<User>(createUserDto);
                user.TenantId = tenant.Id;
                user.Password = UserService.HashPasswordForStorage(createUserDto.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
                user.Plan = PlanType.FREE;

                await _userRepository.AddAsync(user);

                // Generar token para el nuevo admin
                var token = _tokenService.GenerateToken(user.Id.ToString(), user.UserName, user.TenantId);
                var expiresAt = _tokenService.GetTokenExpiration(token);

                var response = new OnboardingResponseDTO
                {
                    Tenant = _mapper.Map<TenantDTO>(tenant),
                    AdminUser = _mapper.Map<UserDTO>(user),
                    Token = token,
                    ExpiresAt = expiresAt
                };

                // Auditoría
                _auditService.LogCreate("Tenant (Onboarding)", tenant.Id, null,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error durante el onboarding: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene todos los tenants
        /// </summary>
        [HttpGet]
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
        [HttpGet("{id}")]
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
        [HttpPost]
        public async Task<ActionResult<TenantDTO>> CreateTenant([FromBody] CreateTenantDTO createTenantDto)
        {
            try
            {
                if (createTenantDto == null)
                    return BadRequest(new { message = "Los datos del tenant no pueden ser nulos." });

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos del tenant inválidos.", errors = ModelState });
                }

                // Sanitizar entrada
                createTenantDto.Nombre = InputSanitizer.SanitizeString(createTenantDto.Nombre);
                if (!string.IsNullOrEmpty(createTenantDto.Descripcion))
                    createTenantDto.Descripcion = InputSanitizer.SanitizeString(createTenantDto.Descripcion);

                if (InputSanitizer.ContainsSqlInjection(createTenantDto.Nombre) || 
                    InputSanitizer.ContainsXss(createTenantDto.Nombre))
                {
                    return BadRequest(new { message = "Entrada inválida detectada." });
                }

                var tenant = await _tenantService.CreateTenantAsync(createTenantDto);

                // Auditoría
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    _auditService.LogCreate("Tenant", tenant.Id, User,
                        SecurityLoggingService.GetClientIpAddress(HttpContext));
                }

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
        /// Actualiza un tenant existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TenantDTO>> UpdateTenant(int id, [FromBody] UpdateTenantDTO updateTenantDto)
        {
            try
            {
                if (updateTenantDto == null)
                    return BadRequest(new { message = "Los datos de actualización no pueden ser nulos." });

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de actualización inválidos.", errors = ModelState });
                }

                // Sanitizar entrada
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

                // Auditoría
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
        [HttpDelete("{id}")]
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

                // Auditoría
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
    }
}

