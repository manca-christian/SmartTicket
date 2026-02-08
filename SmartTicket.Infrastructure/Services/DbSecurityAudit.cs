using SmartTicket.Application.Interfaces;

namespace SmartTicket.Infrastructure.Services;

public sealed class DbSecurityAudit : ISecurityAudit
{
    private readonly IAuditWriter _audit;
    public DbSecurityAudit(IAuditWriter audit) => _audit = audit;

    public void LoginFailed(string email, string reason)
        => _audit.WriteAsync(
            category: "auth",
            eventType: "login_failed",
            data: new { email, reason }).GetAwaiter().GetResult();

    public void UserLockedOut(string email, DateTime untilUtc)
        => _audit.WriteAsync(
            category: "auth",
            eventType: "user_locked",
            data: new { email, untilUtc }).GetAwaiter().GetResult();

    public void LoginSucceeded(string email, Guid userId)
        => _audit.WriteAsync(
            category: "auth",
            eventType: "login_succeeded",
            userId: userId,
            data: new { email }).GetAwaiter().GetResult();

    public void RefreshFailed(string reason)
        => _audit.WriteAsync(
            category: "auth",
            eventType: "refresh_failed",
            data: new { reason }).GetAwaiter().GetResult();

    public void Logout(Guid userId)
        => _audit.WriteAsync(
            category: "auth",
            eventType: "logout",
            userId: userId).GetAwaiter().GetResult();
}
