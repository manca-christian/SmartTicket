using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Repositories;

public sealed class TicketAttachmentRepository : ITicketAttachmentRepository
{
    private readonly AppDbContext _db;
    public TicketAttachmentRepository(AppDbContext db) => _db = db;

    public Task AddRangeAsync(IEnumerable<TicketAttachment> attachments)
    {
        _db.TicketAttachments.AddRange(attachments);
        return Task.CompletedTask;
    }

    public Task<List<TicketAttachment>> GetByTicketIdAsync(Guid ticketId)
        => _db.TicketAttachments
            .AsNoTracking()
            .Where(a => a.TicketId == ticketId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
}
