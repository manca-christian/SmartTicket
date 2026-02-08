using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Interfaces;

public interface ITicketAttachmentRepository
{
    Task AddRangeAsync(IEnumerable<TicketAttachment> attachments);
    Task<List<TicketAttachment>> GetByTicketIdAsync(Guid ticketId);
}
