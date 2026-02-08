namespace SmartTicket.Application.DTOs.Tickets;

public record TicketDetailsDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    DateTime? DueAt,
    DateTime? ClosedAt,
    IReadOnlyList<string> AttachmentUrls
);
