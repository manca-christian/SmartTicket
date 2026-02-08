namespace SmartTicket.Domain.Entities;

public sealed class TicketAttachment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Url { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
}
