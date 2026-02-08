using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.API.Health;

public sealed class DbReadyHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;

    public DbReadyHealthCheck(AppDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Database.CanConnectAsync(cancellationToken))
            return HealthCheckResult.Unhealthy("Database connection failed.");

        var pending = await _db.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pending.Any())
            return HealthCheckResult.Degraded("Database has pending migrations.");

        return HealthCheckResult.Healthy("Database reachable.");
    }
}
