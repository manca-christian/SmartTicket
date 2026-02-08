namespace SmartTicket.Domain.Entities;

public class TicketEvent
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Type { get; set; } = null!;
    public Guid? ActorUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? DataJson { get; set; }
}
