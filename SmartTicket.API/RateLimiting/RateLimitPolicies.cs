using System.Threading.RateLimiting;
using SmartTicket.API.Security;

namespace SmartTicket.API.RateLimiting;

public static class RateLimitPolicies
{
    public static RateLimitPartition<string> Global(HttpContext context, RateLimitingOptions options)
    {
        var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
        var key = isAuthenticated
            ? context.User!.GetUserId().ToString()
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var limit = isAuthenticated
            ? options.Global.AuthenticatedPerMinute
            : options.Global.AnonymousPerMinute;

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = Math.Max(1, limit),
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    }

    public static RateLimitPartition<string> Login(HttpContext context, RateLimitingOptions options)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"login|{ip}";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = Math.Max(1, options.Login.PermitLimit),
            Window = TimeSpan.FromSeconds(Math.Max(1, options.Login.WindowSeconds)),
            QueueLimit = 0
        });
    }
}
