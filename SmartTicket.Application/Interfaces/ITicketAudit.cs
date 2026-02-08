using SmartTicket.Application.DTOs.Tickets;

namespace SmartTicket.Application.Interfaces;

public interface ITicketAudit
{
    Task TicketCreatedAsync(Guid ticketId, Guid createdByUserId, CreateTicketDto dto);
    Task TicketUpdatedAsync(Guid ticketId, Guid requesterUserId, UpdateTicketDto dto, bool isAdmin);
    Task TicketClosedAsync(Guid ticketId, Guid requesterUserId, bool isAdmin);
    Task TicketPriorityChangedAsync(Guid ticketId, Guid requesterUserId, string oldPriority, string newPriority, bool isAdmin);
    Task TicketDueDateChangedAsync(Guid ticketId, Guid requesterUserId, DateTime? oldDueAt, DateTime newDueAt, bool isAdmin);
    Task TicketDueDateClearedAsync(Guid ticketId, Guid requesterUserId, DateTime? oldDueAt, bool isAdmin);
    Task TicketCommentAddedAsync(Guid ticketId, Guid commentId, Guid requesterUserId);
    Task TicketAssignedAsync(Guid ticketId, Guid requesterUserId, AssignTicketDto dto, bool isAdmin);
}
