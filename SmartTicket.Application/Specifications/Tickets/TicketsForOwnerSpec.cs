using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Specifications;
using SmartTicket.Domain.Entities;
using SmartTicket.Domain.Enums;

namespace SmartTicket.Application.Specifications.Tickets;

public sealed class TicketsForOwnerSpec : BaseSpecification<Ticket>
{
    public TicketsForOwnerSpec(Guid ownerUserId, TicketQueryDto q)
    {
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize switch { < 1 => 20, > 100 => 100, _ => q.PageSize };
        ApplyPaging((page - 1) * pageSize, pageSize);

        Criteria = t => t.CreatedByUserId == ownerUserId;

        if (!string.IsNullOrWhiteSpace(q.Status) &&
            Enum.TryParse<TicketStatus>(q.Status, true, out var status))
        {
            Criteria = Criteria!.AndAlso(t => t.Status == status);
        }

        if (q.Assigned.HasValue)
        {
            Criteria = q.Assigned.Value
                ? Criteria!.AndAlso(t => t.AssignedToUserId != null)
                : Criteria!.AndAlso(t => t.AssignedToUserId == null);
        }

        if (q.AssignedToUserId.HasValue)
        {
            var assignee = q.AssignedToUserId.Value;
            Criteria = Criteria!.AndAlso(t => t.AssignedToUserId == assignee);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            Criteria = Criteria!.AndAlso(t => t.Title.Contains(s) || t.Description.Contains(s));
        }

        ApplyOrderByDescending(t => t.CreatedAt);
    }
}
