using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Exceptions;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Domain.Enums;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _db;
    public TicketRepository(AppDbContext db) => _db = db;

    // tracked (serve per update/assign/close)
    public Task<Ticket?> GetByIdAsync(Guid id)
        => _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);

    // no tracking (serve per GET + ETag)
    public Task<Ticket?> GetByIdNoTrackingAsync(Guid id)
        => _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    public Task AddAsync(Ticket ticket)
    {
        _db.Tickets.Add(ticket);
        return Task.CompletedTask;
    }

    public void SetOriginalRowVersion(Ticket ticket, byte[] rowVersion)
    {
        _db.Entry(ticket).Property(x => x.RowVersion).OriginalValue = rowVersion;
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Mappala a 412 nel middleware (consigliato)
            throw new ConcurrencyException();
        }
    }

    public Task<PagedResult<TicketListItemDto>> QueryMineAsync(Guid ownerUserId, TicketQueryDto query)
        => BuildQuery(query)
            .Where(t => t.CreatedByUserId == ownerUserId)
            .ToPagedAsync(query);

    public Task<PagedResult<TicketListItemDto>> QueryAllAsync(TicketQueryDto query)
        => BuildQuery(query).ToPagedAsync(query);

    private IQueryable<Ticket> BuildQuery(TicketQueryDto q)
    {
        var query = _db.Tickets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Status) &&
            Enum.TryParse<TicketStatus>(q.Status, ignoreCase: true, out var status))
            query = query.Where(t => t.Status == status);

        if (q.Assigned.HasValue)
            query = q.Assigned.Value
                ? query.Where(t => t.AssignedToUserId != null)
                : query.Where(t => t.AssignedToUserId == null);

        if (q.AssignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == q.AssignedToUserId);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            query = query.Where(t => t.Title.Contains(s) || t.Description.Contains(s));
        }

        return query.OrderByDescending(t => t.CreatedAt);
    }
}
