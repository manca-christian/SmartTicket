using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartTicket.Application.Interfaces;

namespace SmartTicket.API.Security;

public sealed class TicketAuthorizationHandler : IAuthorizationHandler
{
    private readonly ITicketRepository _repo;

    public TicketAuthorizationHandler(ITicketRepository repo) => _repo = repo;

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (!TryGetTicketId(context, out var ticketId))
            return;

        var ticket = await _repo.GetByIdNoTrackingAsync(ticketId);

        foreach (var requirement in context.PendingRequirements.ToList())
        {
            switch (requirement)
            {
                case TicketAssignRequirement:
                    if (context.User.IsAdmin())
                        context.Succeed(requirement);
                    break;
                case TicketWriteRequirement:
                    if (ticket is null)
                    {
                        context.Succeed(requirement);
                        break;
                    }

                    if (context.User.IsAdmin() || IsOwner(context, ticket.CreatedByUserId))
                        context.Succeed(requirement);
                    break;
                case TicketReadRequirement:
                    if (ticket is null)
                    {
                        context.Succeed(requirement);
                        break;
                    }

                    if (context.User.IsAdmin() || IsOwner(context, ticket.CreatedByUserId) || IsAssignee(context, ticket.AssignedToUserId))
                        context.Succeed(requirement);
                    break;
            }
        }
    }

    private static bool IsOwner(AuthorizationHandlerContext context, Guid ownerUserId)
    {
        try
        {
            return context.User.GetUserId() == ownerUserId;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAssignee(AuthorizationHandlerContext context, Guid? assigneeUserId)
    {
        try
        {
            return assigneeUserId.HasValue && context.User.GetUserId() == assigneeUserId.Value;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryGetTicketId(AuthorizationHandlerContext context, out Guid ticketId)
    {
        ticketId = default;

        if (context.Resource is HttpContext httpContext)
        {
            var value = httpContext.Request.RouteValues["id"] ?? httpContext.Request.RouteValues["ticketId"];
            return Guid.TryParse(value?.ToString(), out ticketId);
        }

        if (context.Resource is AuthorizationFilterContext mvcContext)
        {
            var value = mvcContext.RouteData.Values["id"] ?? mvcContext.RouteData.Values["ticketId"];
            return Guid.TryParse(value?.ToString(), out ticketId);
        }

        return false;
    }
}
