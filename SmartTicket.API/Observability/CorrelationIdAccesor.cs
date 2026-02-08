using SmartTicket.API.Middleware;

namespace SmartTicket.API.Observability;

public static class CorrelationIdAccessor
{
    public static string? Get(HttpContext context)
        => context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var v) ? v?.ToString() : null;
}
