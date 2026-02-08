using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;

public interface ITicketService
{
    Task<Guid> CreateAsync(CreateTicketDto dto, Guid createdByUserId);
    Task<PagedResult<TicketListItemDto>> GetMineAsync(Guid userId, TicketQueryDto query);
    Task<TicketDetailsDto> GetByIdAsync(Guid ticketId, Guid requesterUserId, bool isAdmin);
    Task CloseAsync(Guid ticketId, Guid requesterUserId, bool isAdmin);
    Task UpdateAsync(Guid ticketId, UpdateTicketDto dto, Guid requesterUserId, bool isAdmin);
    Task AssignAsync(Guid ticketId, AssignTicketDto dto, Guid requesterUserId, bool isAdmin);
    Task UpdatePriorityAsync(Guid ticketId, UpdateTicketPriorityDto dto, Guid requesterUserId, bool isAdmin);
    Task UpdateDueDateAsync(Guid ticketId, UpdateTicketDueDateDto dto, Guid requesterUserId, bool isAdmin);
    Task ClearDueDateAsync(Guid ticketId, Guid requesterUserId, bool isAdmin);
    Task<PagedResult<TicketListItemDto>> GetAllAsync(Guid requesterUserId, bool isAdmin, TicketQueryDto query);
}
