using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository;
using SmartModels;

namespace SmartRepository
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _Context;

        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _Context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(string userId)
        {
            return await _Context.RefreshTokens
                                 .Where(token => token.UserId == userId &&
                                                 token.Revoked == null &&
                                                 token.Expires > DateTime.UtcNow)
                                 .ToListAsync();
        }

        public async Task<bool> TryRevokeAsync(Guid refreshTokenId,
                                               DateTime revoked,
                                               string? revokedByIp,
                                               string? replacedByTokenHash)
        {
            var now = DateTime.UtcNow;

            var affected = await _Context.RefreshTokens
                .Where(token => token.RefreshTokenId == refreshTokenId &&
                                token.Revoked == null &&
                                token.Expires > now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.Revoked, revoked)
                    .SetProperty(token => token.RevokedByIp, revokedByIp)
                    .SetProperty(token => token.ReplacedByTokenHash, replacedByTokenHash));

            return affected == 1;
        }
    }
}
