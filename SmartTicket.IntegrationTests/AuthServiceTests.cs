using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.DTOs.Auth;
using SmartTicket.Application.Interfaces;
using SmartTicket.Infrastructure.Services;
using SmartTicket.IntegrationTests.Fakes;

namespace SmartTicket.IntegrationTests;

public sealed class AuthServiceTests : IDisposable
{
    private readonly TestDbFactory _factory = new();

    private const string Pwd = "P@ssw0rd!";

    public void Dispose() => _factory.Dispose();

    private static ISecurityAudit Audit() => new NoopSecurityAudit();

    [Fact]
    public async Task Register_crea_user_e_refresh_token()
    {
        using var db = _factory.CreateDbContext();
        var svc = new AuthService(db, new FakeJwtTokenService(), Audit());

        var (resp, refresh) = await svc.RegisterAsync(new RegisterDto
        {
            Email = "test@example.com",
            Password = Pwd
        });

        resp.AccessToken.Should().NotBeNullOrWhiteSpace();
        refresh.Should().NotBeNullOrWhiteSpace();

        (await db.Users.CountAsync()).Should().Be(1);
        (await db.RefreshTokens.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Login_single_session_revoca_vecchi_refresh_token()
    {
        using var db = _factory.CreateDbContext();
        var svc = new AuthService(db, new FakeJwtTokenService(), Audit());

        await svc.RegisterAsync(new RegisterDto { Email = "a@a.it", Password = Pwd });

        var first = await svc.LoginAsync(new LoginDto { Email = "a@a.it", Password = Pwd });
        var second = await svc.LoginAsync(new LoginDto { Email = "a@a.it", Password = Pwd });

        var active = await db.RefreshTokens.CountAsync(t => t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
        active.Should().Be(1);

        second.refreshToken.Should().NotBe(first.refreshToken);
        second.response.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_ruota_token_e_revoca_tutti_gli_altri()
    {
        using var db = _factory.CreateDbContext();
        var svc = new AuthService(db, new FakeJwtTokenService(), Audit());

        var (_, rt1) = await svc.RegisterAsync(new RegisterDto { Email = "b@b.it", Password = Pwd });

        var (resp2, rt2) = await svc.RefreshAsync(rt1);

        resp2.AccessToken.Should().Contain("token-for-");
        rt2.Should().NotBeNullOrWhiteSpace();
        rt2.Should().NotBe(rt1);

        var active = await db.RefreshTokens.CountAsync(t => t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
        active.Should().Be(1);
    }

    [Fact]
    public async Task Logout_revoca_refresh_token()
    {
        using var db = _factory.CreateDbContext();
        var svc = new AuthService(db, new FakeJwtTokenService(), Audit());

        var (_, rt) = await svc.RegisterAsync(new RegisterDto { Email = "c@c.it", Password = Pwd });

        await svc.LogoutAsync(rt);

        var active = await db.RefreshTokens.CountAsync(t => t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
        active.Should().Be(0);
    }

    private sealed class NoopSecurityAudit : ISecurityAudit
    {
        public void LoginFailed(string email, string reason) { }
        public void UserLockedOut(string email, DateTime untilUtc) { }
        public void LoginSucceeded(string email, Guid userId) { }
        public void RefreshFailed(string reason) { }
        public void Logout(Guid userId) { }
    }
}
