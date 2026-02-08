using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Repositories;

public sealed class TicketEventRepository : ITicketEventRepository
{
    private readonly AppDbContext _db;
    public TicketEventRepository(AppDbContext db) => _db = db;

    public Task AddAsync(TicketEvent ticketEvent)
    {
        _db.TicketEvents.Add(ticketEvent);
        return Task.CompletedTask;
    }

    public async Task<PagedResult<TicketEvent>> GetByTicketIdAsync(Guid ticketId, int page, int pageSize)
    {
        var query = _db.TicketEvents
            .AsNoTracking()
            .Where(e => e.TicketId == ticketId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new TicketEvent
            {
                Id = e.Id,
                TicketId = e.TicketId,
                Type = e.Type,
                ActorUserId = e.ActorUserId,
                CreatedAt = e.CreatedAt,
                DataJson = e.DataJson
            })
            .ToListAsync();

        return new PagedResult<TicketEvent>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
