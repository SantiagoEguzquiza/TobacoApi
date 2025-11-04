using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Services;
using System.Security.Claims;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly TokenService _tokenService;

        public UserController(IUserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                if (loginDto == null)
                    return BadRequest(new { message = "Los datos de login no pueden ser nulos." });

                if (string.IsNullOrWhiteSpace(loginDto.UserName) || string.IsNullOrWhiteSpace(loginDto.Password))
                    return BadRequest(new { message = "El nombre de usuario y la contraseña son requeridos." });

                var result = await _userService.LoginAsync(loginDto);

                if (result == null)
                    return Unauthorized(new { message = "Usuario o contraseña incorrectos. Verifica tus datos e intenta nuevamente." });

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

        [Authorize]
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

                var user = await _userService.CreateUserAsync(createUserDto);
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

        [Authorize]
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

                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado." });

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

        [Authorize]
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

                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                    return NotFound(new { message = "Usuario no encontrado." });

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
