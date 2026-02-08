using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Interfaces;
using SmartTicket.Application.Specifications.Tickets;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Services;

public class TicketQueryService
{
    private readonly IRepository<Ticket> _repo;

    public TicketQueryService(IRepository<Ticket> repo) => _repo = repo;

    public async Task<PagedResult<TicketListItemDto>> MineAsync(Guid userId, TicketQueryDto q)
    {
        var spec = new TicketsForOwnerSpec(userId, q);
        var total = await _repo.CountAsync(spec);
        var items = await _repo.ListAsync(spec);

        var dto = items.Select(t => new TicketListItemDto(
            t.Id, t.Title, t.Status.ToString(), t.CreatedAt, t.CreatedByUserId, t.AssignedToUserId
        )).ToList();

        var pageSize = q.PageSize switch { < 1 => 20, > 100 => 100, _ => q.PageSize };
        var page = q.Page < 1 ? 1 : q.Page;

        return new PagedResult<TicketListItemDto>
        {
            Items = dto,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}
