using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using System.IO;

namespace SmartTicket.Application.Services;

public sealed class TicketCommentService : ITicketCommentService
{
    private readonly ITicketCommentRepository _comments;
    private readonly ICommentAttachmentRepository _attachments;
    private readonly ITicketEventWriter _eventWriter;
    private readonly ITicketAudit _audit;

    public TicketCommentService(
        ITicketCommentRepository comments,
        ICommentAttachmentRepository attachments,
        ITicketEventWriter eventWriter,
        ITicketAudit audit)
    {
        _comments = comments;
        _attachments = attachments;
        _eventWriter = eventWriter;
        _audit = audit;
    }

    public async Task<TicketCommentDto> AddAsync(Guid ticketId, Guid authorUserId, string text, IReadOnlyCollection<string> attachmentUrls)
    {
        var comment = await _comments.AddAsync(ticketId, authorUserId, text);

        if (attachmentUrls.Count > 0)
        {
            var attachments = attachmentUrls
                .Select(url => BuildCommentAttachment(comment.Id, url))
                .ToList();

            await _attachments.AddRangeAsync(attachments);
        }

        await _eventWriter.WriteAsync(ticketId, "ticket_comment_added", authorUserId, new { commentId = comment.Id });
        await _comments.SaveChangesAsync();
        await _audit.TicketCommentAddedAsync(ticketId, comment.Id, authorUserId);

        return new TicketCommentDto(comment.Id, comment.TicketId, comment.AuthorUserId, comment.Text, comment.CreatedAt, attachmentUrls.ToList());
    }

    public async Task<PagedResult<TicketCommentDto>> GetByTicketIdAsync(Guid ticketId, int page = 1, int pageSize = 50)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 100);

        var result = await _comments.GetByTicketIdAsync(ticketId, page, pageSize);
        var commentIds = result.Items.Select(c => c.Id).ToList();
        var attachments = commentIds.Count == 0
            ? new List<CommentAttachment>()
            : await _attachments.GetByCommentIdsAsync(commentIds);

        var attachmentLookup = attachments
            .GroupBy(a => a.CommentId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(a => a.Url).ToList());

        return new PagedResult<TicketCommentDto>
        {
            Items = result.Items
                .Select(c => new TicketCommentDto(
                    c.Id,
                    c.TicketId,
                    c.AuthorUserId,
                    c.Text,
                    c.CreatedAt,
                    attachmentLookup.TryGetValue(c.Id, out var urls) ? urls : Array.Empty<string>()))
                .ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    private static CommentAttachment BuildCommentAttachment(Guid commentId, string url)
    {
        var fileName = Path.GetFileName(url);
        return new CommentAttachment
        {
            Id = Guid.NewGuid(),
            CommentId = commentId,
            Url = url,
            FileName = fileName,
            ContentType = ResolveContentType(fileName),
            Size = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string ResolveContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
