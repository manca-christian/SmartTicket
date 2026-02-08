using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;

namespace SmartTicket.Application.Interfaces;

public interface ITicketEventService
{
    Task<PagedResult<TicketEventDto>> GetByTicketAsync(Guid ticketId, Guid requesterUserId, bool isAdmin, int page = 1, int pageSize = 50);
}
