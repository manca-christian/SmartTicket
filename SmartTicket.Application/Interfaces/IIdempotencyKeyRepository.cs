using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Interfaces;

public interface IIdempotencyKeyRepository
{
    Task<IdempotencyKey?> FindAsync(Guid userId, string key, string path, string method, CancellationToken ct = default);
    Task AddAsync(IdempotencyKey entry, CancellationToken ct = default);
    Task RemoveAsync(IdempotencyKey entry, CancellationToken ct = default);
    Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
