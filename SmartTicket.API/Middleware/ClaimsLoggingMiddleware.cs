using System.Security.Claims;
using SmartTicket.API.Security;

namespace SmartTicket.API.Middleware;

public sealed class ClaimsLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClaimsLoggingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ClaimsLoggingMiddleware(RequestDelegate next, ILogger<ClaimsLoggingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        if (_environment.IsDevelopment() && context.User?.Identity?.IsAuthenticated == true)
        {
            var claims = context.User.Claims
                .Select(c => new { c.Type, c.Value })
                .ToList();

            Guid? userId = null;
            try
            {
                userId = context.User.GetUserId();
            }
            catch
            {
            }

            _logger.LogInformation("Auth claims: {@Claims} ResolvedUserId={UserId}", claims, userId);
        }

        await _next(context);
    }
}
