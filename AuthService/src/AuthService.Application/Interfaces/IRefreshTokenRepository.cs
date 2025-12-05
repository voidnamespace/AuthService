using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IRefreshTokenRepository
{

    Task<RefreshToken?> GetByTokenAsync(string token);

    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);

    Task UpdateAsync(RefreshToken refreshToken);

    Task DeleteAsync(Guid id);

    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);

    Task RevokeAllUserTokensAsync(Guid userId);

    Task DeleteExpiredTokensAsync();
}
