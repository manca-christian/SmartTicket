using SmartTicket.Application.Interfaces;

namespace SmartTicket.IntegrationTests.Fakes;

public sealed class NoopSecurityAudit : ISecurityAudit
{
    public void LoginFailed(string email, string reason) { }
    public void UserLockedOut(string email, DateTime untilUtc) { }
    public void LoginSucceeded(string email, Guid userId) { }
    public void RefreshFailed(string reason) { }
    public void Logout(Guid userId) { }
}
