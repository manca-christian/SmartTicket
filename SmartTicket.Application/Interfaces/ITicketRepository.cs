using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<Ticket?> GetByIdNoTrackingAsync(Guid id);
    Task AddAsync(Ticket ticket);
    void SetOriginalRowVersion(Ticket ticket, byte[] rowVersion);
    Task SaveChangesAsync();
    Task<PagedResult<TicketListItemDto>> QueryMineAsync(Guid ownerUserId, TicketQueryDto query);
    Task<PagedResult<TicketListItemDto>> QueryAllAsync(TicketQueryDto query);
}
