namespace SmartTicket.Infrastructure.Jobs;

public class RefreshTokenCleanupOptions
{
    public int RunEveryMinutes { get; set; } = 60;     
    public int DeleteExpiredAfterDays { get; set; } = 7; 
    public int DeleteRevokedAfterDays { get; set; } = 7; 
}
