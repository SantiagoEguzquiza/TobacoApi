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
        private readonly TokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly AplicationDbContext _context;

        public UserService(
            IUserRepository userRepository, 
            TokenService tokenService, 
            IMapper mapper, 
            IServiceProvider serviceProvider,
            AplicationDbContext context)
        {
            _userRepository = userRepository;
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

            // Validar contraseña
            bool isValid = ValidatePassword(loginDto.Password, user.Password);
            
            // Si falla con BCrypt, intentar con SHA256 antiguo (migración)
            if (!isValid && PasswordService.IsOldPasswordHash(user.Password))
            {
                if (PasswordService.ValidateOldPassword(loginDto.Password, user.Password))
                {
                    // Migrar automáticamente a BCrypt
                    user.Password = PasswordService.HashPassword(loginDto.Password);
                    isValid = true;
                }
            }

            if (!isValid)
            {
                return null;
            }

            // Si se migró la contraseña, guardar el cambio
            if (PasswordService.IsOldPasswordHash(user.Password) == false && 
                PasswordService.NeedsRehash(user.Password))
            {
                // Actualizar si el workFactor es menor al deseado
                user.Password = PasswordService.HashPassword(loginDto.Password);
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate token with TenantId
            var token = _tokenService.GenerateToken(user.Id.ToString(), user.UserName, user.TenantId);
            var expiresAt = _tokenService.GetTokenExpiration(token);

            return new LoginResponseDTO
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDTO>(user)
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
            // Si es un hash antiguo SHA256, no validar aquí (se hace en LoginAsync para migración)
            if (PasswordService.IsOldPasswordHash(hashedPassword))
            {
                return false; // Se manejará en LoginAsync
            }
            
            // Usar BCrypt para validación segura con protección contra timing attacks
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
