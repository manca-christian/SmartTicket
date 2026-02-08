using System.Diagnostics;
using Serilog.Context;
using SmartTicket.API.Security;
using SmartTicket.Application.Observability;

namespace SmartTicket.API.Middleware;

public sealed class RequestLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLogContextMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        var correlationId = CorrelationContext.Current ?? context.TraceIdentifier;

        string? userId = null;
        string? role = null;

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            try
            {
                userId = context.User.GetUserId().ToString();
                role = context.User.IsAdmin() ? "Admin" : "User";
            }
            catch
            {
                
            }
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
        using (LogContext.PushProperty("Role", role ?? "anonymous"))
        using (LogContext.PushProperty("Path", context.Request.Path.Value ?? ""))
        using (LogContext.PushProperty("Method", context.Request.Method))
        {
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();

                Serilog.Log.Information(
                    "HTTP {Method} {Path} => {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds
                );
            }
        }
    }
}
