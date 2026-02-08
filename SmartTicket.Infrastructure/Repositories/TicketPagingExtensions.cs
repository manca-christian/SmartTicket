using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Infrastructure.Repositories;

public static class TicketPagingExtensions
{
    public static async Task<PagedResult<TicketListItemDto>> ToPagedAsync(
        this IQueryable<Ticket> baseQuery,
        TicketQueryDto q)
    {
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => q.PageSize
        };

        var total = await baseQuery.CountAsync();

        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.Title,
                t.Status.ToString(),
                t.CreatedAt,
                t.CreatedByUserId,
                t.AssignedToUserId
            ))
            .ToListAsync();

        return new PagedResult<TicketListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}
