using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Interfaces;

public interface ICommentAttachmentRepository
{
    Task AddRangeAsync(IEnumerable<CommentAttachment> attachments);
    Task<List<CommentAttachment>> GetByCommentIdsAsync(IReadOnlyCollection<Guid> commentIds);
}
