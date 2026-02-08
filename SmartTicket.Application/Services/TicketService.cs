using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;
using SmartTicket.Domain.Enums;
using System.IO;

namespace SmartTicket.Application.Services;

public sealed class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;
    private readonly ITicketAttachmentRepository _attachments;
    private readonly ITicketEventWriter _eventWriter;

    public TicketService(ITicketRepository repo, ITicketAttachmentRepository attachments, ITicketEventWriter eventWriter)
    {
        _repo = repo;
        _attachments = attachments;
        _eventWriter = eventWriter;
    }

    public async Task<Guid> CreateAsync(CreateTicketDto dto, Guid createdByUserId)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            Status = TicketStatus.Open
        };

        await _repo.AddAsync(ticket);

        if (dto.AttachmentUrls.Count > 0)
        {
            var attachments = dto.AttachmentUrls
                .Select(url => BuildTicketAttachment(ticket.Id, url))
                .ToList();

            await _attachments.AddRangeAsync(attachments);
        }

        await _eventWriter.WriteAsync(ticket.Id, "ticket_created", createdByUserId);
        await _repo.SaveChangesAsync();
        return ticket.Id;
    }

    public Task<PagedResult<TicketListItemDto>> GetMineAsync(Guid userId, TicketQueryDto query)
        => _repo.QueryMineAsync(userId, query);

    public async Task<TicketDetailsDto> GetByIdAsync(Guid ticketId, Guid requesterUserId, bool isAdmin)
    {
        var ticket = await _repo.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket non trovato.");

        if (ticket.CreatedByUserId != requesterUserId && !isAdmin)
            throw new UnauthorizedAccessException("Non hai i permessi per vedere questo ticket.");

        var attachments = await _attachments.GetByTicketIdAsync(ticketId);

        return new TicketDetailsDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.CreatedAt,
            ticket.CreatedByUserId,
            ticket.AssignedToUserId,
            ticket.DueAt,
            ticket.ClosedAt,
            attachments.Select(a => a.Url).ToList()
        );
    }

    public async Task UpdateAsync(Guid ticketId, UpdateTicketDto dto, Guid requesterUserId, bool isAdmin)
    {
        var ticket = await _repo.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket non trovato.");

        if (ticket.CreatedByUserId != requesterUserId && !isAdmin)
            throw new UnauthorizedAccessException("Non hai i permessi per modificare questo ticket.");

        ticket.Title = dto.Title;
        ticket.Description = dto.Description;

        await _eventWriter.WriteAsync(ticket.Id, "ticket_updated", requesterUserId, new { dto.Title });
        await _repo.SaveChangesAsync();
    }

    public async Task AssignAsync(Guid ticketId, AssignTicketDto dto, Guid requesterUserId, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Solo Admin può assegnare ticket.");

        var ticket = await _repo.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket non trovato.");

        ticket.AssignedToUserId = dto.AssigneeUserId;
        ticket.AssignedAt = DateTime.UtcNow;

        await _eventWriter.WriteAsync(ticket.Id, "ticket_assigned", requesterUserId, new { assigneeUserId = dto.AssigneeUserId });
        await _repo.SaveChangesAsync();
    }

    public async Task CloseAsync(Guid ticketId, Guid requesterUserId, bool isAdmin)
    {
        var ticket = await _repo.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket non trovato.");

        if (ticket.CreatedByUserId != requesterUserId && !isAdmin)
            throw new UnauthorizedAccessException("Non hai i permessi per chiudere questo ticket.");

        if (ticket.Status == TicketStatus.Closed) return;

        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;

        await _eventWriter.WriteAsync(ticket.Id, "ticket_closed", requesterUserId);
        await _repo.SaveChangesAsync();
    }


    public Task<PagedResult<TicketListItemDto>> GetAllAsync(Guid requesterUserId, bool isAdmin, TicketQueryDto query)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Solo Admin può vedere tutti i ticket.");

        return _repo.QueryAllAsync(query);
    }

    public async Task UpdatePriorityAsync(Guid ticketId, UpdateTicketPriorityDto dto, Guid requesterUserId, bool isAdmin)
    {
        var ticket = await _repo.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket non trovato.");

        if (ticket.CreatedByUserId != requesterUserId && !isAdmin)
            throw new UnauthorizedAccessException("Non hai i permessi per modificare questo ticket.");

        if (!Enum.IsDefined(typeof(TicketPriority), dto.Priority))
            throw new ArgumentException("Priority non valida.");

        var oldPriority = ticket.Priority;
        ticket.Priority = dto.Priority;

        await _eventWriter.WriteAsync(ticket.Id, "ticket_priority_changed", requesterUserId,
            new { oldPriority = oldPriority.ToString(), newPriority = dto.Priority.ToString() });
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateDueDateAsync(Guid ticketId, UpdateTicketDueDateDto dto, Guid requesterUserId, bool isAdmin)
    {
        var ticket = await _repo.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket non trovato.");

        if (ticket.CreatedByUserId != requesterUserId && !isAdmin)
            throw new UnauthorizedAccessException("Non hai i permessi per modificare questo ticket.");

        var minDue = DateTime.UtcNow.AddMinutes(-1);
        if (dto.DueAt < minDue)
            throw new ArgumentException("DueAt deve essere nel futuro.");

        var oldDueAt = ticket.DueAt;
        ticket.DueAt = dto.DueAt;

        await _eventWriter.WriteAsync(ticket.Id, "ticket_due_changed", requesterUserId,
            new { oldDueAt, newDueAt = dto.DueAt });
        await _repo.SaveChangesAsync();
    }

    public async Task ClearDueDateAsync(Guid ticketId, Guid requesterUserId, bool isAdmin)
    {
        var ticket = await _repo.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket non trovato.");

        if (ticket.CreatedByUserId != requesterUserId && !isAdmin)
            throw new UnauthorizedAccessException("Non hai i permessi per modificare questo ticket.");

        var oldDueAt = ticket.DueAt;
        ticket.DueAt = null;

        await _eventWriter.WriteAsync(ticket.Id, "ticket_due_cleared", requesterUserId,
            new { oldDueAt });
        await _repo.SaveChangesAsync();
    }

    private static TicketAttachment BuildTicketAttachment(Guid ticketId, string url)
    {
        var fileName = Path.GetFileName(url);
        return new TicketAttachment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Url = url,
            FileName = fileName,
            ContentType = ResolveContentType(fileName),
            Size = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string ResolveContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

}
