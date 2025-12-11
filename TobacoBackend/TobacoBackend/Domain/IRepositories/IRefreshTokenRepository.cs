using TobacoBackend.Domain.Models;

namespace TobacoBackend.Domain.IRepositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task RevokeTokenAsync(string token);
        Task RevokeAllUserTokensAsync(int userId);
        Task CleanExpiredTokensAsync();
    }
}
