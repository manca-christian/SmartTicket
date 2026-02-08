namespace SmartTicket.Application.DTOs.Tickets;

public record TicketListItemDto(
    Guid Id,
    string Title,
    string Status,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    Guid? AssignedToUserId
);
