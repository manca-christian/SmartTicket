using Microsoft.Extensions.Logging;
using SmartTicket.Application.Interfaces;

namespace SmartTicket.Infrastructure.Observability;

public sealed class LogSecurityAudit : ISecurityAudit
{
    private readonly ILogger<LogSecurityAudit> _logger;
    public LogSecurityAudit(ILogger<LogSecurityAudit> logger) => _logger = logger;

    public void LoginFailed(string email, string reason) =>
        _logger.LogWarning("SECURITY login_failed email={Email} reason={Reason}", email, reason);

    public void UserLockedOut(string email, DateTime untilUtc) =>
        _logger.LogWarning("SECURITY lockout email={Email} untilUtc={UntilUtc:o}", email, untilUtc);

    public void LoginSucceeded(string email, Guid userId) =>
        _logger.LogInformation("SECURITY login_ok email={Email} userId={UserId}", email, userId);

    public void RefreshFailed(string reason) =>
        _logger.LogWarning("SECURITY refresh_failed reason={Reason}", reason);

    public void Logout(Guid userId) =>
        _logger.LogInformation("SECURITY logout userId={UserId}", userId);
}
