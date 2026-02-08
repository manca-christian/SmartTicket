using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartTicket.Application.Observability;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Jobs;

public sealed class RefreshTokenCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupJob> _logger;
    private readonly RefreshTokenCleanupOptions _options;

    public RefreshTokenCleanupJob(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupJob> logger,
        IOptions<RefreshTokenCleanupOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var period = TimeSpan.FromMinutes(Math.Max(1, _options.RunEveryMinutes));
        using var timer = new PeriodicTimer(period);

        // parte subito una volta
        await RunOnce(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnce(stoppingToken);
        }
    }

    private async Task RunOnce(CancellationToken ct)
    {
        var correlationId = $"job-{Guid.NewGuid():N}";
        CorrelationContext.Current = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.UtcNow;
                var expiredCutoff = now.AddDays(-Math.Max(0, _options.DeleteExpiredAfterDays));
                var revokedCutoff = now.AddDays(-Math.Max(0, _options.DeleteRevokedAfterDays));

                var deletedExpired = await db.RefreshTokens
                    .Where(t => t.ExpiresAt < expiredCutoff)
                    .ExecuteDeleteAsync(ct);

                var deletedRevoked = await db.RefreshTokens
                    .Where(t => t.RevokedAt != null && t.RevokedAt < revokedCutoff)
                    .ExecuteDeleteAsync(ct);

                _logger.LogInformation(
                    "RefreshToken cleanup done. DeletedExpired={DeletedExpired}, DeletedRevoked={DeletedRevoked}",
                    deletedExpired, deletedRevoked);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken cleanup failed");
            }
        }
    }
}
