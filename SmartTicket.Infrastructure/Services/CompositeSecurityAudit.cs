using SmartTicket.Application.Interfaces;
using SmartTicket.Infrastructure.Observability;

namespace SmartTicket.Infrastructure.Services;

public sealed class CompositeSecurityAudit : ISecurityAudit
{
    private readonly LogSecurityAudit _logAudit;
    private readonly DbSecurityAudit _dbAudit;

    public CompositeSecurityAudit(LogSecurityAudit logAudit, DbSecurityAudit dbAudit)
    {
        _logAudit = logAudit;
        _dbAudit = dbAudit;
    }

    public void LoginFailed(string email, string reason)
    {
        _logAudit.LoginFailed(email, reason);
        _dbAudit.LoginFailed(email, reason);
    }

    public void UserLockedOut(string email, DateTime untilUtc)
    {
        _logAudit.UserLockedOut(email, untilUtc);
        _dbAudit.UserLockedOut(email, untilUtc);
    }

    public void LoginSucceeded(string email, Guid userId)
    {
        _logAudit.LoginSucceeded(email, userId);
        _dbAudit.LoginSucceeded(email, userId);
    }

    public void RefreshFailed(string reason)
    {
        _logAudit.RefreshFailed(reason);
        _dbAudit.RefreshFailed(reason);
    }

    public void Logout(Guid userId)
    {
        _logAudit.Logout(userId);
        _dbAudit.Logout(userId);
    }
}
