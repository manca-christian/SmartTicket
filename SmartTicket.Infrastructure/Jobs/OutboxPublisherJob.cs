using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTicket.Application.Observability;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Jobs;

public sealed class OutboxPublisherJob : BackgroundService
{
    private const int BatchSize = 50;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherJob> _logger;

    public OutboxPublisherJob(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

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

                var pending = await db.OutboxMessages
                    .Where(o => o.ProcessedAt == null)
                    .OrderBy(o => o.OccurredAt)
                    .Take(BatchSize)
                    .ToListAsync(ct);

                if (pending.Count == 0)
                    return;

                var now = DateTime.UtcNow;

                foreach (var message in pending)
                {
                    try
                    {
                        _logger.LogInformation("Outbox publish simulated. Type={Type} Id={Id}", message.Type, message.Id);
                        message.ProcessedAt = now;
                        message.Error = null;
                    }
                    catch (Exception ex)
                    {
                        message.Error = ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message;
                    }
                }

                await db.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publish failed");
            }
        }
    }
}
