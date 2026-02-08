using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SmartTicket.Application.Observability;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Services;

public interface IAuditWriter
{
    Task WriteAsync(
        string category,
        string eventType,
        Guid? userId = null,
        string? subjectType = null,
        Guid? subjectId = null,
        object? data = null,
        string? message = null);
}

public sealed class DbAuditWriter : IAuditWriter
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public DbAuditWriter(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task WriteAsync(
        string category,
        string eventType,
        Guid? userId = null,
        string? subjectType = null,
        Guid? subjectId = null,
        object? data = null,
        string? message = null)
    {
        var ctx = _http.HttpContext;

        var ip = ctx?.Connection.RemoteIpAddress?.ToString();
        var traceId = ctx?.TraceIdentifier;
        var correlationId = CorrelationContext.Current ?? traceId;

        var ev = new AuditEvent
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Category = category,
            EventType = eventType,
            UserId = userId,
            SubjectType = subjectType,
            SubjectId = subjectId,
            IpAddress = ip,
            TraceId = traceId,
            CorrelationId = correlationId,
            Message = message,
            DataJson = data is null ? null : JsonSerializer.Serialize(data)
        };

        _db.AuditEvents.Add(ev);
        var payload = JsonSerializer.Serialize(new
        {
            subjectId,
            type = eventType,
            createdAt = ev.CreatedAt
        });

        if (payload.Length <= 4000)
        {
            _db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = $"ticket.audit.{category}",
                PayloadJson = payload,
                OccurredAt = ev.CreatedAt
            });
        }
        await _db.SaveChangesAsync();
    }
}
