using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Services;
using TobacoBackend.Helpers;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using TobacoBackend.Authorization;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly TokenService _tokenService;
        private readonly SecurityLoggingService _securityLogger;
        private readonly AuditService _auditService;

        public UserController(IUserService userService, TokenService tokenService, SecurityLoggingService securityLogger, AuditService auditService)
        {
            _userService = userService;
            _tokenService = tokenService;
            _securityLogger = securityLogger;
            _auditService = auditService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                if (loginDto == null)
                    return BadRequest(new { message = "Los datos de login no pueden ser nulos." });

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos de login inválidos.", errors = ModelState });
                }

                // Sanitizar y validar entrada
                loginDto.UserName = InputSanitizer.SanitizeUserName(loginDto.UserName);
                
                if (InputSanitizer.ContainsSqlInjection(loginDto.UserName) || 
                    InputSanitizer.ContainsXss(loginDto.UserName))
                {
                    _securityLogger.LogUnauthorizedAccess(loginDto.UserName, "POST:/api/User/login",
                        SecurityLoggingService.GetClientIpAddress(HttpContext));
                    return BadRequest(new { message = "Entrada inválida detectada." });
                }

                var result = await _userService.LoginAsync(loginDto);

                if (result == null)
                {
                    // Log intento de login fallido
                    var ipAddress = SecurityLoggingService.GetClientIpAddress(HttpContext);
                    _securityLogger.LogFailedLoginAttempt(loginDto.UserName, ipAddress);
                    
                    return Unauthorized(new { 
                        message = "Usuario o contraseña incorrectos."
                    });
                }

                // Log login exitoso
                _securityLogger.LogSuccessfulLogin(
                    result.User.UserName, 
                    result.User.Id,
                    SecurityLoggingService.GetClientIpAddress(HttpContext)
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error durante el login: {ex.Message}" });
            }
        }

        [HttpGet("profile/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserProfile(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado." });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener el perfil del usuario: {ex.Message}" });
            }
        }

        [HttpPost("validate")]
        public async Task<ActionResult> ValidateUser([FromBody] LoginDTO loginDto)
        {
            try
            {
                if (loginDto == null)
                    return BadRequest(new { message = "Los datos de validación no pueden ser nulos." });

                var isValid = await _userService.ValidateUserAsync(loginDto.UserName, loginDto.Password);
                
                return Ok(new { isValid = isValid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error durante la validación: {ex.Message}" });
            }
        }

        [HttpPost("validate-token")]
        public ActionResult ValidateToken([FromBody] TokenValidationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                    return BadRequest(new { message = "El token es requerido." });

                var isValid = _tokenService.IsTokenValid(request.Token);
                var userId = isValid ? _tokenService.GetUserIdFromToken(request.Token) : null;
                var expiresAt = isValid ? _tokenService.GetTokenExpiration(request.Token) : (DateTime?)null;

                return Ok(new 
                { 
                    isValid = isValid,
                    userId = userId,
                    expiresAt = expiresAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error durante la validación del token: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Token inválido." });

                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado." });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener el usuario actual: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            try
            {
                // Extract user ID from claims (try both sub and NameIdentifier)
                var subClaim = User.FindFirst("sub")?.Value;
                var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userIdClaim = subClaim ?? nameIdClaim;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Token inválido. No se pudo extraer el ID del usuario." });

                // Get current user to check role and tipoVendedor
                var currentUser = await _userService.GetUserByIdAsync(userId);
                if (currentUser == null)
                    return Unauthorized(new { message = "Usuario no encontrado." });

                // Check if user is admin or vendedor
                var isAdmin = currentUser.Role == "Admin";
                var isVendedor = currentUser.Role == "Employee" && currentUser.TipoVendedor == TipoVendedor.Vendedor;
                
                if (!isAdmin && !isVendedor)
                    return Forbid("Solo los administradores y vendedores pueden acceder a esta funcionalidad.");

                var users = await _userService.GetAllUsersAsync();
                
                // Si es Vendedor, filtrar solo los repartidores (Repartidor o RepartidorVendedor)
                if (isVendedor)
                {
                    users = users.Where(u => 
                        u.IsActive && 
                        u.Role == "Employee" && 
                        (u.TipoVendedor == TipoVendedor.Repartidor || u.TipoVendedor == TipoVendedor.RepartidorVendedor)
                    ).ToList();
                }
                
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener los usuarios: {ex.Message}" });
            }
        }

        // Test endpoint to verify all users are being returned (remove in production)
        [HttpGet("test-all")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> TestGetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                
                Console.WriteLine($"Test endpoint: Returning {users.Count()} users");
                foreach (var user in users)
                {
                    Console.WriteLine($"Test endpoint: User {user.UserName} - Active: {user.IsActive}");
                }
                
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al obtener los usuarios: {ex.Message}" });
            }
        }

        [Authorize(Policy = AuthorizationPolicies.AdminOnly)] // Solo Admin puede crear usuarios
        [HttpPost("create")]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] CreateUserDTO createUserDto)
        {
            try
            {
                var subClaim = User.FindFirst("sub")?.Value;
                var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userIdClaim = subClaim ?? nameIdClaim;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Token inválido." });

                // Check if user is admin
                var isAdmin = await _userService.IsAdminAsync(userId);
                if (!isAdmin)
                    return Forbid("Solo los administradores pueden crear usuarios.");

                if (createUserDto == null)
                    return BadRequest(new { message = "Los datos del usuario no pueden ser nulos." });

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Datos del usuario inválidos.", errors = ModelState });
                }

                // Sanitizar nombre de usuario
                createUserDto.UserName = InputSanitizer.SanitizeUserName(createUserDto.UserName);
                
                if (InputSanitizer.ContainsSqlInjection(createUserDto.UserName) || 
                    InputSanitizer.ContainsXss(createUserDto.UserName))
                {
                    return BadRequest(new { message = "Entrada inválida detectada." });
                }

                var user = await _userService.CreateUserAsync(createUserDto, userId);
                
                // Auditoría
                _auditService.LogCreate("Usuario", user.Id, User,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return CreatedAtAction(nameof(GetUserProfile), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al crear el usuario: {ex.Message}" });
            }
        }

        [Authorize(Policy = AuthorizationPolicies.AdminOnly)] // Solo Admin puede actualizar usuarios
        [HttpPut("update/{id}")]
        public async Task<ActionResult<object>> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                // Extract user ID from claims (try both sub and NameIdentifier)
                var subClaim = User.FindFirst("sub")?.Value;
                var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userIdClaim = subClaim ?? nameIdClaim;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                    return Unauthorized(new { message = "Token inválido." });

                // Check if user is admin
                var isAdmin = await _userService.IsAdminAsync(currentUserId);
                if (!isAdmin)
                    return Forbid("Solo los administradores pueden actualizar usuarios.");

                if (updateUserDto == null)
                    return BadRequest(new { message = "Los datos de actualización no pueden ser nulos." });

                // Validar modelo
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
                }

                // Check if trying to update a SuperAdmin - only SuperAdmins can update other SuperAdmins
                var userToUpdate = await _userService.GetUserByIdAsync(id);
                if (userToUpdate == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                // Prevent deactivating SuperAdmins unless the current user is also a SuperAdmin
                if (userToUpdate.Role == "SuperAdmin" && updateUserDto.IsActive.HasValue && !updateUserDto.IsActive.Value)
                {
                    var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                    if (currentUser == null || currentUser.Role != "SuperAdmin")
                    {
                        return Forbid("No se puede desactivar un usuario SuperAdmin. Solo los SuperAdmins pueden desactivar otros SuperAdmins.");
                    }
                }

                // Prevent changing SuperAdmin role or other SuperAdmin properties unless current user is SuperAdmin
                if (userToUpdate.Role == "SuperAdmin" && currentUserId != id)
                {
                    var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                    if (currentUser == null || currentUser.Role != "SuperAdmin")
                    {
                        // Allow only isActive changes if current user is not SuperAdmin
                        if (updateUserDto.Role != null || updateUserDto.UserName != null || 
                            updateUserDto.Password != null || updateUserDto.Email != null)
                        {
                            return Forbid("No se pueden modificar las propiedades de un usuario SuperAdmin. Solo los SuperAdmins pueden modificar otros SuperAdmins.");
                        }
                    }
                }

                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado." });

                // Auditoría
                _auditService.LogUpdate("Usuario", id, User, null,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                // Check if the current user was deactivated
                bool currentUserAffected = false;
                if (id == currentUserId && updateUserDto.IsActive.HasValue && !updateUserDto.IsActive.Value)
                {
                    currentUserAffected = true;
                }

                return Ok(new { 
                    user = user, 
                    currentUserAffected = currentUserAffected,
                    message = currentUserAffected ? "Tu cuenta ha sido desactivada. Serás redirigido al login." : "Usuario actualizado exitosamente."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al actualizar el usuario: {ex.Message}" });
            }
        }

        [Authorize(Policy = AuthorizationPolicies.AdminOnly)] // Solo Admin puede eliminar usuarios
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<object>> DeleteUser(int id)
        {
            try
            {
                // Extract user ID from claims (try both sub and NameIdentifier)
                var subClaim = User.FindFirst("sub")?.Value;
                var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userIdClaim = subClaim ?? nameIdClaim;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                    return Unauthorized(new { message = "Token inválido." });

                // Check if user is admin
                var isAdmin = await _userService.IsAdminAsync(currentUserId);
                if (!isAdmin)
                    return Forbid("Solo los administradores pueden eliminar usuarios.");

                // Check if trying to delete themselves
                bool currentUserAffected = (currentUserId == id);

                // Validar ID
                if (id <= 0)
                {
                    return BadRequest(new { message = "ID de usuario inválido." });
                }

                // Check if trying to delete a SuperAdmin - only SuperAdmins can delete other SuperAdmins
                var userToDelete = await _userService.GetUserByIdAsync(id);
                if (userToDelete == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                // Prevent deleting SuperAdmins unless the current user is also a SuperAdmin
                if (userToDelete.Role == "SuperAdmin")
                {
                    var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                    if (currentUser == null || currentUser.Role != "SuperAdmin")
                    {
                        return Forbid("No se puede eliminar un usuario SuperAdmin. Solo los SuperAdmins pueden eliminar otros SuperAdmins.");
                    }
                }

                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                    return NotFound(new { message = "Usuario no encontrado." });

                // Auditoría
                _auditService.LogDelete("Usuario", id, User,
                    SecurityLoggingService.GetClientIpAddress(HttpContext));

                return Ok(new { 
                    currentUserAffected = currentUserAffected,
                    message = currentUserAffected ? "Tu cuenta ha sido eliminada. Serás redirigido al login." : "Usuario eliminado exitosamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al eliminar el usuario: {ex.Message}" });
            }
        }

        // Endpoint temporal para verificar la duración del token
        [HttpGet("test-token-duration")]
        public ActionResult TestTokenDuration()
        {
            try
            {
                var testToken = _tokenService.GenerateToken("999", "test_user");
                var expiration = _tokenService.GetTokenExpiration(testToken);
                var now = DateTime.UtcNow;
                var duration = expiration - now;

                return Ok(new
                {
                    message = "Token de prueba generado",
                    tokenGenerated = testToken,
                    expiration = expiration,
                    currentTime = now,
                    durationInDays = duration.TotalDays,
                    durationInHours = duration.TotalHours,
                    configuredExpirationDays = 30
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al generar token de prueba: {ex.Message}" });
            }
        }

    }

    public class TokenValidationRequest
    {
        public string Token { get; set; }
    }
}
