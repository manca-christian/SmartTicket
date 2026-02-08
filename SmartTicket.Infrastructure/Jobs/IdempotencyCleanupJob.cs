using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartTicket.Application.Interfaces;
using SmartTicket.Application.Observability;

namespace SmartTicket.Infrastructure.Jobs;

public sealed class IdempotencyCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdempotencyCleanupJob> _logger;
    private readonly IdempotencyCleanupOptions _options;

    public IdempotencyCleanupJob(
        IServiceScopeFactory scopeFactory,
        ILogger<IdempotencyCleanupJob> logger,
        IOptions<IdempotencyCleanupOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var period = TimeSpan.FromMinutes(Math.Max(1, _options.RunEveryMinutes));
        using var timer = new PeriodicTimer(period);

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
                var repo = scope.ServiceProvider.GetRequiredService<IIdempotencyKeyRepository>();

                var deleted = await repo.DeleteExpiredAsync(DateTime.UtcNow, ct);
                _logger.LogInformation("Idempotency cleanup done. Deleted={Deleted}", deleted);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Idempotency cleanup failed");
            }
        }
    }
}
