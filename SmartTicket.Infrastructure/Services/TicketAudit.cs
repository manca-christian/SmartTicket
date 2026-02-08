using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Interfaces;

namespace SmartTicket.Infrastructure.Services;

public sealed class TicketAudit : ITicketAudit
{
    private readonly IAuditWriter _audit;
    public TicketAudit(IAuditWriter audit) => _audit = audit;

    public Task TicketCreatedAsync(Guid ticketId, Guid createdByUserId, CreateTicketDto dto)
        => _audit.WriteAsync(
            category: "ticket",
            eventType: "ticket_created",
            userId: createdByUserId,
            subjectType: "ticket",
            subjectId: ticketId,
            data: new { dto.Title });

    public Task TicketUpdatedAsync(Guid ticketId, Guid requesterUserId, UpdateTicketDto dto, bool isAdmin)
        => _audit.WriteAsync(
            category: "ticket",
            eventType: "ticket_updated",
            userId: requesterUserId,
            subjectType: "ticket",
            subjectId: ticketId,
            data: new { dto.Title, isAdmin });

    public Task TicketClosedAsync(Guid ticketId, Guid requesterUserId, bool isAdmin)
        => _audit.WriteAsync(
            category: "ticket",
            eventType: "ticket_closed",
            userId: requesterUserId,
            subjectType: "ticket",
            subjectId: ticketId,
            data: new { isAdmin });

    public Task TicketPriorityChangedAsync(Guid ticketId, Guid requesterUserId, string oldPriority, string newPriority, bool isAdmin)
        => _audit.WriteAsync(
            category: "ticket",
            eventType: "ticket_priority_changed",
            userId: requesterUserId,
            subjectType: "ticket",
            subjectId: ticketId,
            data: new { oldPriority, newPriority, isAdmin });

    public Task TicketDueDateChangedAsync(Guid ticketId, Guid requesterUserId, DateTime? oldDueAt, DateTime newDueAt, bool isAdmin)
        => _audit.WriteAsync(
            category: "ticket",
            eventType: "ticket_due_changed",
            userId: requesterUserId,
            subjectType: "ticket",
            subjectId: ticketId,
            data: new { oldDueAt, newDueAt, isAdmin });

    public Task TicketDueDateClearedAsync(Guid ticketId, Guid requesterUserId, DateTime? oldDueAt, bool isAdmin)
        => _audit.WriteAsync(
            category: "ticket",
            eventType: "ticket_due_cleared",
            userId: requesterUserId,
            subjectType: "ticket",
            subjectId: ticketId,
            data: new { oldDueAt, isAdmin });

    public Task TicketCommentAddedAsync(Guid ticketId, Guid commentId, Guid requesterUserId)
        => _audit.WriteAsync(
            category: "ticket",
            eventType: "ticket_comment_added",
            userId: requesterUserId,
            subjectType: "ticket",
            subjectId: ticketId,
            data: new { commentId });

    public Task TicketAssignedAsync(Guid ticketId, Guid requesterUserId, AssignTicketDto dto, bool isAdmin)
        => _audit.WriteAsync(
            category: "ticket",
            eventType: "ticket_assigned",
            userId: requesterUserId,
            subjectType: "ticket",
            subjectId: ticketId,
            data: new { dto.AssigneeUserId, isAdmin });
}
