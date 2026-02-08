using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Repositories;

public sealed class TicketCommentRepository : ITicketCommentRepository
{
    private readonly AppDbContext _db;
    public TicketCommentRepository(AppDbContext db) => _db = db;

    public Task<TicketComment> AddAsync(Guid ticketId, Guid authorUserId, string text)
    {
        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorUserId = authorUserId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        _db.TicketComments.Add(comment);
        return Task.FromResult(comment);
    }

    public async Task<PagedResult<TicketComment>> GetByTicketIdAsync(Guid ticketId, int page, int pageSize)
    {
        var query = _db.TicketComments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new TicketComment
            {
                Id = c.Id,
                TicketId = c.TicketId,
                AuthorUserId = c.AuthorUserId,
                Text = c.Text,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<TicketComment>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
