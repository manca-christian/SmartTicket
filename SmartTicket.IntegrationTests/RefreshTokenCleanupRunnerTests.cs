using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Jobs;

namespace SmartTicket.IntegrationTests;

public class RefreshTokenCleanupRunnerTests : IDisposable
{
    private readonly TestDbFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Cleanup_cancella_scaduti_e_revocati_oltre_cutoff()
    {
        using var db = _factory.CreateDbContext();

        var userId = Guid.NewGuid();

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            ExpiresAt = DateTime.UtcNow.AddDays(-10),
        });

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            RevokedAt = DateTime.UtcNow.AddDays(-10)
        });

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(10),
        });

        await db.SaveChangesAsync();

        var options = Options.Create(new RefreshTokenCleanupOptions
        {
            DeleteExpiredAfterDays = 7,
            DeleteRevokedAfterDays = 7
        });

        var runner = new RefreshTokenCleanupRunner(db, NullLogger<RefreshTokenCleanupRunner>.Instance, options);

        var (deletedExpired, deletedRevoked) = await runner.RunOnceAsync();

        deletedExpired.Should().Be(1);
        deletedRevoked.Should().Be(1);

        db.RefreshTokens.Count().Should().Be(1); 
    }
}
