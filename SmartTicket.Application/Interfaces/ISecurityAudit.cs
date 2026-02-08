namespace SmartTicket.Application.Interfaces;

public interface ISecurityAudit
{
    void LoginFailed(string email, string reason);
    void UserLockedOut(string email, DateTime untilUtc);
    void LoginSucceeded(string email, Guid userId);
    void RefreshFailed(string reason);
    void Logout(Guid userId);
}
