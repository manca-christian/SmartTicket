using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.DTOs.Auth;
using SmartTicket.Application.Exceptions;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;
using SmartTicket.Infrastructure.Security;
using System.Security.Cryptography;

namespace SmartTicket.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly ISecurityAudit _audit;
    private readonly PasswordHasher<User> _hasher = new();

    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(14);

    public AuthService(AppDbContext db, IJwtTokenService jwt, ISecurityAudit audit)
    {
        _db = db;
        _jwt = jwt;
        _audit = audit;
    }

    public async Task<(AuthResponseDto response, string refreshToken)> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException("Email già registrata.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);

        await RevokeAllActiveRefreshTokensAsync(user.Id);
        var refreshToken = CreateRefreshToken();
        await AddRefreshTokenAsync(user.Id, refreshToken);

        await _db.SaveChangesAsync();

        var access = _jwt.CreateToken(user);
        return (new AuthResponseDto(access), refreshToken);
    }


    public async Task<(AuthResponseDto response, string refreshToken)> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            _audit.LoginFailed(email, "user_not_found");
            throw new UnauthorizedAccessException("Credenziali non valide.");
        }

        var now = DateTime.UtcNow;

        if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > now)
        {
            _audit.LoginFailed(email, "locked");
            throw new AccountLockedException(user.LockoutUntil.Value, "Account temporaneamente bloccato. Riprova più tardi.");
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;

            const int maxAttempts = 5;
            var lockoutDuration = TimeSpan.FromMinutes(5);

            if (user.FailedLoginCount >= maxAttempts)
            {
                user.LockoutUntil = now.Add(lockoutDuration);
                user.FailedLoginCount = 0;

                _audit.UserLockedOut(email, user.LockoutUntil.Value);
            }
            else
            {
                _audit.LoginFailed(email, $"bad_password_attempt_{user.FailedLoginCount}");
            }

            await _db.SaveChangesAsync();
            throw new UnauthorizedAccessException("Credenziali non valide.");
        }

        user.FailedLoginCount = 0;
        user.LockoutUntil = null;

        await RevokeAllActiveRefreshTokensAsync(user.Id);

        var refreshToken = CreateRefreshToken();
        await AddRefreshTokenAsync(user.Id, refreshToken);

        await _db.SaveChangesAsync();

        _audit.LoginSucceeded(email, user.Id);

        var access = _jwt.CreateToken(user);
        return (new AuthResponseDto(access), refreshToken);
    }

    public async Task<(AuthResponseDto response, string refreshToken)> RefreshAsync(string refreshToken)
    {
        var tokenHash = TokenHash.Sha256(refreshToken);

        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
        if (stored is null)
        {
            _audit.RefreshFailed("not_found");
            throw new UnauthorizedAccessException("Refresh token non valido.");
        }

        if (!stored.IsActive)
        {
            _audit.RefreshFailed("inactive");
            throw new UnauthorizedAccessException("Sessione scaduta o revocata. Effettua di nuovo il login.");
        }

        var user = await _db.Users.FirstAsync(u => u.Id == stored.UserId);

        await RevokeAllActiveRefreshTokensAsync(user.Id);

        var newRefresh = CreateRefreshToken();
        await AddRefreshTokenAsync(user.Id, newRefresh);

        await _db.SaveChangesAsync();

        var access = _jwt.CreateToken(user);
        return (new AuthResponseDto(access), newRefresh);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = TokenHash.Sha256(refreshToken);

        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
        if (stored is null) return;

        if (stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        _audit.Logout(stored.UserId);
    }

    private static string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private Task AddRefreshTokenAsync(Guid userId, string refreshTokenPlain)
    {
        var tokenHash = TokenHash.Sha256(refreshTokenPlain);

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(RefreshLifetime)
        });

        return Task.CompletedTask;
    }
    private async Task RevokeAllActiveRefreshTokensAsync(Guid userId)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        if (activeTokens.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var t in activeTokens)
            t.RevokedAt = now;
    }
}
