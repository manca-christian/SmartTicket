namespace SmartTicket.API.RateLimiting;

public sealed class RateLimitingOptions
{
    public GlobalRateLimitingOptions Global { get; set; } = new();
    public LoginRateLimitingOptions Login { get; set; } = new();
}

public sealed class GlobalRateLimitingOptions
{
    public int AuthenticatedPerMinute { get; set; } = 100;
    public int AnonymousPerMinute { get; set; } = 30;
}

public sealed class LoginRateLimitingOptions
{
    public int PermitLimit { get; set; } = 5;
    public int WindowSeconds { get; set; } = 60;
}
