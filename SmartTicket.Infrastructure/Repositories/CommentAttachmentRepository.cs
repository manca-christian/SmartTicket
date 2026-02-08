using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Repositories;

public sealed class CommentAttachmentRepository : ICommentAttachmentRepository
{
    private readonly AppDbContext _db;
    public CommentAttachmentRepository(AppDbContext db) => _db = db;

    public Task AddRangeAsync(IEnumerable<CommentAttachment> attachments)
    {
        _db.CommentAttachments.AddRange(attachments);
        return Task.CompletedTask;
    }

    public Task<List<CommentAttachment>> GetByCommentIdsAsync(IReadOnlyCollection<Guid> commentIds)
        => _db.CommentAttachments
            .AsNoTracking()
            .Where(a => commentIds.Contains(a.CommentId))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
}
