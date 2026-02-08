namespace SmartTicket.Domain.Entities;

public class TicketComment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
