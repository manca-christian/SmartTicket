using SmartTicket.Application.DTOs.Common;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Interfaces;

public interface ITicketEventRepository
{
    Task AddAsync(TicketEvent ticketEvent);
    Task<PagedResult<TicketEvent>> GetByTicketIdAsync(Guid ticketId, int page, int pageSize);
    Task SaveChangesAsync();
}
