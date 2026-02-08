using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Jobs;

public sealed class RefreshTokenCleanupRunner(
    AppDbContext db,
    ILogger<RefreshTokenCleanupRunner> logger,
    IOptions<RefreshTokenCleanupOptions> options)
{
    private readonly AppDbContext _db = db;
    private readonly ILogger<RefreshTokenCleanupRunner> _logger = logger;
    private readonly RefreshTokenCleanupOptions _options = options.Value;

    public async Task<(int deletedExpired, int deletedRevoked)> RunOnceAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var expiredCutoff = now.AddDays(-Math.Max(0, _options.DeleteExpiredAfterDays));
        var revokedCutoff = now.AddDays(-Math.Max(0, _options.DeleteRevokedAfterDays));

        var deletedExpired = await _db.RefreshTokens
            .Where(t => t.ExpiresAt < expiredCutoff)
            .ExecuteDeleteAsync(ct);

        var deletedRevoked = await _db.RefreshTokens
            .Where(t => t.RevokedAt != null && t.RevokedAt < revokedCutoff)
            .ExecuteDeleteAsync(ct);

        _logger.LogInformation("Cleanup done. Expired={Expired}, Revoked={Revoked}", deletedExpired, deletedRevoked);
        return (deletedExpired, deletedRevoked);
    }
}
