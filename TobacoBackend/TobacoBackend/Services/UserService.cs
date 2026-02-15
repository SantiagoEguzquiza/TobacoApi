using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Mapping;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TobacoBackend.Persistence;

namespace TobacoBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly TokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly AplicationDbContext _context;

        public UserService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            TokenService tokenService, 
            IMapper mapper, 
            IServiceProvider serviceProvider,
            AplicationDbContext context)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _context = context;
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByUserNameAsync(loginDto.UserName);
            
            if (user == null)
            {
                // No revelar si el usuario existe o no (seguridad)
                return null;
            }

            // Validar contraseña (solo BCrypt)
            if (!ValidatePassword(loginDto.Password, user.Password))
                return null;

            // Actualizar hash si el workFactor es menor al deseado
            if (PasswordService.NeedsRehash(user.Password))
            {
                user.Password = PasswordService.HashPassword(loginDto.Password);
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Revoke all existing refresh tokens for this user
            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

            // Generate access token with TenantId
            var token = _tokenService.GenerateToken(user.Id.ToString(), user.UserName, user.TenantId);
            var expiresAt = _tokenService.GetTokenExpiration(token);
            var expiresIn = (int)(expiresAt - DateTime.UtcNow).TotalSeconds;

            // Generate refresh token (duración: RefreshTokenExpirationMinutes si > 0, si no RefreshTokenExpirationDays)
            var refreshTokenString = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.Add(_tokenService.RefreshTokenExpiration);

            // Save refresh token to database
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                TenantId = user.TenantId,
                ExpiresAt = refreshTokenExpiration,
                IsRevoked = false
            };
            await _refreshTokenRepository.CreateAsync(refreshToken);

            // Limpiar tokens expirados/revocados para que la tabla no crezca sin límite
            await _refreshTokenRepository.CleanExpiredTokensAsync();

            return new LoginResponseDTO
            {
                Token = token,
                RefreshToken = refreshTokenString,
                ExpiresAt = expiresAt,
                ExpiresIn = expiresIn,
                User = _mapper.Map<UserDTO>(user)
            };
        }

        public async Task<RefreshTokenResponseDTO?> RefreshTokenAsync(string refreshTokenString)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenString);
            
            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }

            // Get user
            var user = refreshToken.User;
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // Revoke the old refresh token
            await _refreshTokenRepository.RevokeTokenAsync(refreshTokenString);

            // Generate new access token
            var newAccessToken = _tokenService.GenerateToken(user.Id.ToString(), user.UserName, user.TenantId);
            var expiresAt = _tokenService.GetTokenExpiration(newAccessToken);
            var expiresIn = (int)(expiresAt - DateTime.UtcNow).TotalSeconds;

            // Generate new refresh token (misma duración configurada)
            var newRefreshTokenString = _tokenService.GenerateRefreshToken();
            var newRefreshTokenExpiration = DateTime.UtcNow.Add(_tokenService.RefreshTokenExpiration);

            // Save new refresh token
            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshTokenString,
                UserId = user.Id,
                TenantId = user.TenantId,
                ExpiresAt = newRefreshTokenExpiration,
                IsRevoked = false
            };
            await _refreshTokenRepository.CreateAsync(newRefreshToken);

            // Limpiar tokens expirados/revocados para que la tabla no crezca sin límite
            await _refreshTokenRepository.CleanExpiredTokensAsync();

            return new RefreshTokenResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString, // Rotar refresh token por seguridad
                ExpiresIn = expiresIn
            };
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDTO>(user) : null;
        }

        public async Task<bool> ValidateUserAsync(string userName, string password)
        {
            var user = await _userRepository.GetByUserNameAsync(userName);
            return user != null && ValidatePassword(password, user.Password);
        }

        private bool ValidatePassword(string password, string hashedPassword)
        {
            return PasswordService.VerifyPassword(password, hashedPassword);
        }

        public static string HashPasswordForStorage(string password)
        {
            // Usar BCrypt para hashing seguro con salt automático
            return PasswordService.HashPassword(password);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto, int? creatorId = null)
        {
            // Validar política de contraseña
            if (!Helpers.PasswordPolicy.IsValid(createUserDto.Password, out string? passwordError))
            {
                throw new InvalidOperationException(passwordError ?? "La contraseña no cumple con los requisitos de seguridad.");
            }

            // Check if username already exists (checking all users, not just active ones)
            var usernameExists = await _userRepository.ExistsAsync(createUserDto.UserName);
            if (usernameExists)
            {
                throw new InvalidOperationException("El nombre de usuario ya existe.");
            }

            var user = _mapper.Map<User>(createUserDto);
            user.Password = HashPasswordForStorage(createUserDto.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            user.TipoVendedor = createUserDto.TipoVendedor; // Asegurar que se asigne el tipo de vendedor

            // Set TenantId from current context
            var tenantId = _context.GetCurrentTenantId();
            if (!tenantId.HasValue)
            {
                throw new InvalidOperationException("No se pudo determinar el TenantId del contexto actual.");
            }
            user.TenantId = tenantId.Value;

            // Manejo del Plan y CreatedById
            if (creatorId.HasValue)
            {
                // Verificar si el creador es un admin
                var creator = await _userRepository.GetByIdAsync(creatorId.Value);
                if (creator != null && creator.Role == "Admin")
                {
                    // Si el creador es admin, el sub-usuario hereda su plan
                    user.Plan = creator.Plan;
                    user.CreatedById = creatorId.Value;
                    
                    // Validar que no se intente asignar un plan diferente al del admin
                    if (createUserDto.Plan.HasValue && createUserDto.Plan.Value != creator.Plan)
                    {
                        throw new InvalidOperationException($"Los sub-usuarios deben heredar el plan del administrador. El plan '{creator.Plan}' será asignado automáticamente.");
                    }
                }
                else if (creator != null)
                {
                    // Si el creador no es admin, solo puede asignar plan si es desarrollador (por ahora permitimos)
                    // Por defecto, usar el plan especificado o FREE
                    user.Plan = createUserDto.Plan ?? PlanType.FREE;
                    user.CreatedById = creatorId.Value;
                }
            }
            else
            {
                // Si no hay creador (creación directa por desarrollador), usar el plan especificado o FREE por defecto
                user.Plan = createUserDto.Plan ?? PlanType.FREE;
                user.CreatedById = null;
            }

            var createdUser = await _userRepository.CreateAsync(user);
            
            // Si NO es admin, crear permisos por defecto (todos desactivados)
            // Esto aplica para: Employee, Vendedor, Repartidor, Vendedor-Repartidor
            if (createdUser.Role != "Admin")
            {
                // Inyectar el servicio de permisos de forma lazy para evitar dependencia circular
                var permisosService = _serviceProvider.GetRequiredService<IPermisosEmpleadoService>();
                await permisosService.CreateDefaultPermisosAsync(createdUser.Id);
            }
            
            return _mapper.Map<UserDTO>(createdUser);
        }

        public async Task<UserDTO?> UpdateUserAsync(int id, UpdateUserDTO updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            // Check if username is being changed and if it already exists
            if (!string.IsNullOrEmpty(updateUserDto.UserName) && updateUserDto.UserName != user.UserName)
            {
                var usernameExists = await _userRepository.ExistsAsync(updateUserDto.UserName);
                if (usernameExists)
                {
                    throw new InvalidOperationException("El nombre de usuario ya existe.");
                }
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateUserDto.UserName))
                user.UserName = updateUserDto.UserName;
            
            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.Email = updateUserDto.Email;
            
            if (!string.IsNullOrEmpty(updateUserDto.Role))
                user.Role = updateUserDto.Role;
            
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                // Validar política de contraseña
                if (!Helpers.PasswordPolicy.IsValid(updateUserDto.Password, out string? passwordError))
                {
                    throw new InvalidOperationException(passwordError ?? "La contraseña no cumple con los requisitos de seguridad.");
                }
                user.Password = HashPasswordForStorage(updateUserDto.Password);
            }
            
            if (updateUserDto.IsActive.HasValue)
                user.IsActive = updateUserDto.IsActive.Value;
            
            if (updateUserDto.TipoVendedor.HasValue)
                user.TipoVendedor = updateUserDto.TipoVendedor.Value;
            
            if (updateUserDto.Zona != null)
                user.Zona = updateUserDto.Zona;

            // Actualización del Plan
            if (updateUserDto.Plan.HasValue && updateUserDto.Plan.Value != user.Plan)
            {
                var oldPlan = user.Plan;
                user.Plan = updateUserDto.Plan.Value;

                // Si el usuario es un Admin y cambia su plan, actualizar automáticamente todos sus sub-usuarios
                if (user.Role == "Admin")
                {
                    await _userRepository.UpdateSubUsersPlanAsync(user.Id, updateUserDto.Plan.Value);
                }
            }

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // Hard delete - actually remove the user from database
            await _userRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Role == "Admin";
        }
    }
}
