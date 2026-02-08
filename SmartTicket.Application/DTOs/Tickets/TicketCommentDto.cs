namespace SmartTicket.Application.DTOs.Tickets;

public record TicketCommentDto(
    Guid Id,
    Guid TicketId,
    Guid AuthorUserId,
    string Text,
    DateTime CreatedAt,
    IReadOnlyList<string> AttachmentUrls
);
