using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;
using TobacoBackend.Services;

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
                    return Unauthorized(new { message = "Credenciales inválidas." });

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
    }

    public class TokenValidationRequest
    {
        public string Token { get; set; }
    }
}
