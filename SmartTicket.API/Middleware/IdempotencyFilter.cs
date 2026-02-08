using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SmartTicket.Application.Interfaces;
using SmartTicket.API.Security;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Jobs;

namespace SmartTicket.API.Middleware;

public sealed class IdempotencyFilter : IAsyncActionFilter
{
    private const int MaxKeyLength = 100;
    private const int MaxResponseLength = 4000;
    private readonly IIdempotencyKeyRepository _repo;
    private readonly IdempotencyCleanupOptions _options;

    public IdempotencyFilter(IIdempotencyKeyRepository repo, IOptions<IdempotencyCleanupOptions> options)
    {
        _repo = repo;
        _options = options.Value;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues))
        {
            await next();
            return;
        }

        var key = keyValues.ToString();
        if (string.IsNullOrWhiteSpace(key) || key.Length > MaxKeyLength)
        {
            context.Result = new BadRequestObjectResult(new { message = "Invalid Idempotency-Key" });
            return;
        }

        Guid userId;
        try
        {
            userId = context.HttpContext.User.GetUserId();
        }
        catch
        {
            await next();
            return;
        }

        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        var method = context.HttpContext.Request.Method;

        var existing = await _repo.FindAsync(userId, key, path, method, context.HttpContext.RequestAborted);
        if (existing is not null)
        {
            if (existing.ExpiresAt > DateTime.UtcNow)
            {
                context.Result = BuildCachedResult(existing);
                return;
            }

            await _repo.RemoveAsync(existing, context.HttpContext.RequestAborted);
            await _repo.SaveChangesAsync(context.HttpContext.RequestAborted);
        }

        var executed = await next();

        if (executed.Exception is not null)
            return;

        if (executed.Result is ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            var json = objectResult.Value is null ? null : JsonSerializer.Serialize(objectResult.Value);

            if (json is not null && json.Length > MaxResponseLength)
                return;

            await SaveEntryAsync(userId, key, path, method, statusCode, json, context.HttpContext.RequestAborted);
        }
        else if (executed.Result is StatusCodeResult statusResult)
        {
            await SaveEntryAsync(userId, key, path, method, statusResult.StatusCode, null, context.HttpContext.RequestAborted);
        }
    }

    private async Task SaveEntryAsync(Guid userId, string key, string path, string method, int statusCode, string? json, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var entry = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Key = key,
            Path = path,
            Method = method,
            StatusCode = statusCode,
            ResponseBodyJson = json,
            CreatedAt = now,
            ExpiresAt = now.AddHours(Math.Max(1, _options.ExpirationHours))
        };

        await _repo.AddAsync(entry, ct);
        await _repo.SaveChangesAsync(ct);
    }

    private static IActionResult BuildCachedResult(IdempotencyKey entry)
    {
        if (entry.ResponseBodyJson is null)
            return new StatusCodeResult(entry.StatusCode);

        return new ContentResult
        {
            StatusCode = entry.StatusCode,
            ContentType = "application/json",
            Content = entry.ResponseBodyJson
        };
    }
}
