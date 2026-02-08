namespace SmartTicket.Application.Interfaces;

public interface ITicketEventWriter
{
    Task WriteAsync(Guid ticketId, string type, Guid? actorUserId, object? data = null);
}
