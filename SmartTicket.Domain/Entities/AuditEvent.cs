namespace SmartTicket.Domain.Entities;

public sealed class AuditEvent
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Category { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public Guid? UserId { get; set; }
    public string? SubjectType { get; set; }
    public Guid? SubjectId { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public string? TraceId { get; set; }
    public string? DataJson { get; set; }
    public string? Message { get; set; }
}
