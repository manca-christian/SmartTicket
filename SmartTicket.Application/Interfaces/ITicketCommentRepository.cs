using SmartTicket.Application.DTOs.Common;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Interfaces;

public interface ITicketCommentRepository
{
    Task<TicketComment> AddAsync(Guid ticketId, Guid authorUserId, string text);
    Task<PagedResult<TicketComment>> GetByTicketIdAsync(Guid ticketId, int page, int pageSize);
    Task SaveChangesAsync();
}
