using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartTicket.Application.Observability;

namespace SmartTicket.API.Health;

public static class HealthResponseWriter
{
    public static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("HealthResponseWriter");

        if (context.Request.Path.StartsWithSegments("/health/ready") && report.Status != HealthStatus.Healthy)
        {
            logger.LogWarning(
                "Readiness check failed. Status={Status} CorrelationId={CorrelationId} TraceId={TraceId}",
                report.Status,
                CorrelationContext.Current,
                context.TraceIdentifier);
        }

        var payload = new
        {
            status = report.Status.ToString(),
            timestampUtc = DateTime.UtcNow,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                exception = env.IsDevelopment() && entry.Value.Exception is not null
                    ? entry.Value.Exception.Message
                    : null
            })
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
