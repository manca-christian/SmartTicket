using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Interfaces;

namespace SmartTicket.Application.Services;

public sealed class TicketEventService : ITicketEventService
{
    private readonly ITicketEventRepository _events;
    private readonly ITicketRepository _tickets;

    public TicketEventService(ITicketEventRepository events, ITicketRepository tickets)
    {
        _events = events;
        _tickets = tickets;
    }

    public async Task<PagedResult<TicketEventDto>> GetByTicketAsync(Guid ticketId, Guid requesterUserId, bool isAdmin, int page = 1, int pageSize = 50)
    {
        var ticket = await _tickets.GetByIdNoTrackingAsync(ticketId)
            ?? throw new KeyNotFoundException("Ticket non trovato.");

        if (!isAdmin && ticket.CreatedByUserId != requesterUserId)
            throw new UnauthorizedAccessException("Non hai i permessi per vedere questo ticket.");

        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 100);

        var result = await _events.GetByTicketIdAsync(ticketId, page, pageSize);

        return new PagedResult<TicketEventDto>
        {
            Items = result.Items
                .Select(e => new TicketEventDto(e.Id, e.TicketId, e.Type, e.ActorUserId, e.CreatedAt, e.DataJson))
                .ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}
