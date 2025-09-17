using TobacoBackend.DTOs;

namespace TobacoBackend.Domain.IServices
{
    public interface IUserService
    {
        Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDto);
        Task<UserDTO?> GetUserByIdAsync(int id);
        Task<bool> ValidateUserAsync(string userName, string password);
    }
}
