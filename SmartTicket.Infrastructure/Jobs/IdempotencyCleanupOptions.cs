namespace SmartTicket.Infrastructure.Jobs;

public class IdempotencyCleanupOptions
{
    public int ExpirationHours { get; set; } = 24;
    public int RunEveryMinutes { get; set; } = 60;
}
