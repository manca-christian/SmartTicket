using System.Text.Json;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Services;

public sealed class TicketEventWriter : ITicketEventWriter
{
    private readonly AppDbContext _db;

    public TicketEventWriter(AppDbContext db) => _db = db;

    public Task WriteAsync(Guid ticketId, string type, Guid? actorUserId, object? data = null)
    {
        var occurredAt = DateTime.UtcNow;
        var ticketEvent = new TicketEvent
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Type = type,
            ActorUserId = actorUserId,
            CreatedAt = occurredAt,
            DataJson = data is null ? null : JsonSerializer.Serialize(data)
        };

        _db.TicketEvents.Add(ticketEvent);

        var payload = JsonSerializer.Serialize(new
        {
            subjectId = ticketId,
            type,
            createdAt = occurredAt
        });

        if (payload.Length <= 4000)
        {
            _db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "ticket.event.created",
                PayloadJson = payload,
                OccurredAt = occurredAt
            });
        }

        return Task.CompletedTask;
    }
}
