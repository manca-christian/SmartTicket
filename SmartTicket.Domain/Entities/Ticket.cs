using SmartTicket.Domain.Enums;

namespace SmartTicket.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open; 
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public DateTime? DueAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

}
