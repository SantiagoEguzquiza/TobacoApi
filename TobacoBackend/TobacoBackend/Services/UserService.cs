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
    }
}
