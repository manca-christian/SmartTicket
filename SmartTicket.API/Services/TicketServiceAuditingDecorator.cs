using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Interfaces;

namespace SmartTicket.API.Services;

public sealed class TicketServiceAuditingDecorator : ITicketService
{
    private readonly ITicketService _inner;
    private readonly ITicketAudit _audit;
    private readonly ITicketRepository _repo;

    public TicketServiceAuditingDecorator(ITicketService inner, ITicketAudit audit, ITicketRepository repo)
    {
        _inner = inner;
        _audit = audit;
        _repo = repo;
    }

    public async Task<Guid> CreateAsync(CreateTicketDto dto, Guid createdByUserId)
    {
        var id = await _inner.CreateAsync(dto, createdByUserId);
        await _audit.TicketCreatedAsync(id, createdByUserId, dto);
        return id;
    }

    public Task<PagedResult<TicketListItemDto>> GetMineAsync(Guid userId, TicketQueryDto query)
        => _inner.GetMineAsync(userId, query);

    public Task<PagedResult<TicketListItemDto>> GetAllAsync(Guid requesterUserId, bool isAdmin, TicketQueryDto query)
        => _inner.GetAllAsync(requesterUserId, isAdmin, query);

    public Task<TicketDetailsDto> GetByIdAsync(Guid ticketId, Guid requesterUserId, bool isAdmin)
        => _inner.GetByIdAsync(ticketId, requesterUserId, isAdmin);

    public async Task UpdateAsync(Guid ticketId, UpdateTicketDto dto, Guid requesterUserId, bool isAdmin)
    {
        await _inner.UpdateAsync(ticketId, dto, requesterUserId, isAdmin);
        await _audit.TicketUpdatedAsync(ticketId, requesterUserId, dto, isAdmin);
    }

    public async Task CloseAsync(Guid ticketId, Guid requesterUserId, bool isAdmin)
    {
        await _inner.CloseAsync(ticketId, requesterUserId, isAdmin);
        await _audit.TicketClosedAsync(ticketId, requesterUserId, isAdmin);
    }

    public async Task AssignAsync(Guid ticketId, AssignTicketDto dto, Guid requesterUserId, bool isAdmin)
    {
        await _inner.AssignAsync(ticketId, dto, requesterUserId, isAdmin);
        await _audit.TicketAssignedAsync(ticketId, requesterUserId, dto, isAdmin);
    }

    public async Task UpdatePriorityAsync(Guid ticketId, UpdateTicketPriorityDto dto, Guid requesterUserId, bool isAdmin)
    {
        var existing = await _repo.GetByIdNoTrackingAsync(ticketId);
        var oldPriority = existing?.Priority.ToString() ?? dto.Priority.ToString();

        await _inner.UpdatePriorityAsync(ticketId, dto, requesterUserId, isAdmin);
        await _audit.TicketPriorityChangedAsync(ticketId, requesterUserId, oldPriority, dto.Priority.ToString(), isAdmin);
    }

    public async Task UpdateDueDateAsync(Guid ticketId, UpdateTicketDueDateDto dto, Guid requesterUserId, bool isAdmin)
    {
        var existing = await _repo.GetByIdNoTrackingAsync(ticketId);
        var oldDueAt = existing?.DueAt;

        await _inner.UpdateDueDateAsync(ticketId, dto, requesterUserId, isAdmin);
        await _audit.TicketDueDateChangedAsync(ticketId, requesterUserId, oldDueAt, dto.DueAt, isAdmin);
    }

    public async Task ClearDueDateAsync(Guid ticketId, Guid requesterUserId, bool isAdmin)
    {
        var existing = await _repo.GetByIdNoTrackingAsync(ticketId);
        var oldDueAt = existing?.DueAt;

        await _inner.ClearDueDateAsync(ticketId, requesterUserId, isAdmin);
        await _audit.TicketDueDateClearedAsync(ticketId, requesterUserId, oldDueAt, isAdmin);
    }
}
