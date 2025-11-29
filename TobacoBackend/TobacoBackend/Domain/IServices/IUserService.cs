using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IUserService
    {
        Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDto);
        Task<UserDTO?> GetUserByIdAsync(int id);
        Task<bool> ValidateUserAsync(string userName, string password);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto, int? creatorId = null);
        Task<UserDTO?> UpdateUserAsync(int id, UpdateUserDTO updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> IsAdminAsync(int userId);
    }
}
