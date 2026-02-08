namespace SmartTicket.Domain.Entities;

public class IdempotencyKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Key { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Method { get; set; } = null!;
    public int StatusCode { get; set; }
    public string? ResponseBodyJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
