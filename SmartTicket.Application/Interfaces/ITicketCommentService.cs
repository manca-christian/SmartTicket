using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;

namespace SmartTicket.Application.Interfaces;

public interface ITicketCommentService
{
    Task<TicketCommentDto> AddAsync(Guid ticketId, Guid authorUserId, string text, IReadOnlyCollection<string> attachmentUrls);
    Task<PagedResult<TicketCommentDto>> GetByTicketIdAsync(Guid ticketId, int page = 1, int pageSize = 50);
}
