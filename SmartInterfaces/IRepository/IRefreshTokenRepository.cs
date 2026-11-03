using SmartModels;

namespace SmartInterfaces.IRepository
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

        Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(string userId);

        /// <summary>
        /// Atomically revokes an active refresh token. Returns false when the token was already
        /// revoked (for example by a concurrent refresh request).
        /// </summary>
        Task<bool> TryRevokeAsync(Guid refreshTokenId, DateTime revoked, string? revokedByIp, string? replacedByTokenHash);
    }
}
