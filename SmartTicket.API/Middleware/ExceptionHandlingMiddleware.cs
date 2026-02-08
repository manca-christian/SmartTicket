using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.Exceptions;
using SmartTicket.Application.Observability;
using System.Security.Authentication;


namespace SmartTicket.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteProblemDetails(context, ex);
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, Exception ex)
    {
        
       var (status, title, errorCode) = ex switch
       {
           PreconditionRequiredException => (StatusCodes.Status428PreconditionRequired, "Precondition Required", "PRECONDITION_REQUIRED"),
           PreconditionFailedException => (StatusCodes.Status412PreconditionFailed, "Precondition Failed", "PRECONDITION_FAILED"),
           ConcurrencyException => (StatusCodes.Status412PreconditionFailed, "Precondition Failed", "CONCURRENCY_CONFLICT"),
           DbUpdateConcurrencyException => (StatusCodes.Status412PreconditionFailed, "Precondition Failed", "CONCURRENCY_CONFLICT"),
           AccountLockedException => (StatusCodes.Status423Locked, "Account locked", "ACCOUNT_LOCKED"),
           UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", "FORBIDDEN"),
           AuthenticationException => (StatusCodes.Status401Unauthorized, "Unauthorized", "UNAUTHORIZED"),
           KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", "NOT_FOUND"),
           InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", "CONFLICT"),
           ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", "VALIDATION_FAILED"),
           _ => (StatusCodes.Status500InternalServerError, "Server Error", "INTERNAL_SERVER_ERROR")
       };



        var problem = new ProblemDetails
        {
            Type = $"https://errors.smartticket.dev/{errorCode}",
            Status = status,
            Title = title,
            Detail = status == 500 ? "Si è verificato un errore interno." : ex.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;
        problem.Extensions["correlationId"] = CorrelationContext.Current;
        problem.Extensions["errorCode"] = errorCode;

        if (ex is AccountLockedException locked)
        {
            problem.Extensions["lockoutUntilUtc"] = locked.UntilUtc;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        await context.Response.WriteAsJsonAsync(problem);
    }
}
