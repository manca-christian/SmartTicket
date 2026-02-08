using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Repositories;

public sealed class IdempotencyKeyRepository : IIdempotencyKeyRepository
{
    private readonly AppDbContext _db;
    public IdempotencyKeyRepository(AppDbContext db) => _db = db;

    public Task<IdempotencyKey?> FindAsync(Guid userId, string key, string path, string method, CancellationToken ct = default)
        => _db.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.UserId == userId && i.Key == key && i.Path == path && i.Method == method, ct);

    public Task AddAsync(IdempotencyKey entry, CancellationToken ct = default)
    {
        _db.IdempotencyKeys.Add(entry);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(IdempotencyKey entry, CancellationToken ct = default)
    {
        _db.IdempotencyKeys.Remove(entry);
        return Task.CompletedTask;
    }

    public Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken ct = default)
        => _db.IdempotencyKeys
            .Where(i => i.ExpiresAt <= utcNow)
            .ExecuteDeleteAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
