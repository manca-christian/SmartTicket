namespace SmartTicket.Application.DTOs.Tickets;

public record TicketEventDto(
    Guid Id,
    Guid TicketId,
    string Type,
    Guid? ActorUserId,
    DateTime CreatedAt,
    string? DataJson
);
