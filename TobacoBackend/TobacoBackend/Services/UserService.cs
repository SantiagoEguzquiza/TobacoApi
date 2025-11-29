using System.Security.Cryptography;
using System.Text;
using TobacoBackend.Domain.IRepositories;
using TobacoBackend.Domain.IServices;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;
using TobacoBackend.Mapping;
using AutoMapper;

namespace TobacoBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, TokenService tokenService, IMapper mapper)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByUserNameAsync(loginDto.UserName);
            
            if (user == null || !ValidatePassword(loginDto.Password, user.Password))
            {
                return null;
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate token
            var token = _tokenService.GenerateToken(user.Id.ToString(), user.UserName);
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
            // Simple password validation - in production, use proper hashing like BCrypt
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static string HashPasswordForStorage(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto, int? creatorId = null)
        {
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
                user.Password = HashPasswordForStorage(updateUserDto.Password);
            
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
