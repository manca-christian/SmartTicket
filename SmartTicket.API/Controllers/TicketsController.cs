using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.Middleware;
using SmartTicket.API.Security;
using SmartTicket.API.Swagger;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Exceptions;
using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;

namespace SmartTicket.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tickets")]
[Route("api/tickets")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly ITicketService _service;
    private readonly ITicketRepository _repo;
    private readonly ITicketCommentService _comments;
    private readonly ITicketEventService _events;
    private readonly ITicketAttachmentRepository _attachments;

    public TicketsController(
        ITicketService service,
        ITicketRepository repo,
        ITicketCommentService comments,
        ITicketEventService events,
        ITicketAttachmentRepository attachments)
    {
        _service = service;
        _repo = repo;
        _comments = comments;
        _events = events;
        _attachments = attachments;
    }

    [HttpPost]
    [Idempotency]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        var userId = User.GetUserId();
        var id = await _service.CreateAsync(dto, userId);

        // REST: 201 + Location header verso GET by id
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // ✅ GET che ritorna ETag strong + details
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "TicketRead")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await _repo.GetByIdNoTrackingAsync(id) ?? throw new KeyNotFoundException("Ticket non trovato.");
        var attachments = await _attachments.GetByTicketIdAsync(id);

        var etag = TicketETag.Compute(entity);
        Response.Headers.ETag = etag;

        var dto = new TicketDetailsDto(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Status.ToString(),
            entity.Priority.ToString(),
            entity.CreatedAt,
            entity.CreatedByUserId,
            entity.AssignedToUserId,
            entity.DueAt,
            entity.ClosedAt,
            attachments.Select(a => a.Url).ToList()
        );

        return Ok(dto);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromQuery] TicketQueryDto query)
    {
        var userId = User.GetUserId();
        var result = await _service.GetMineAsync(userId, query);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> All([FromQuery] TicketQueryDto query)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        var result = await _service.GetAllAsync(userId, isAdmin, query);
        return Ok(result);
    }

    [HttpGet("{id:guid}/comments")]
    [Authorize(Policy = "TicketRead")]
    public async Task<IActionResult> GetComments(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var comments = await _comments.GetByTicketIdAsync(id, page, pageSize);
        return Ok(comments);
    }

    [HttpPost("{id:guid}/comments")]
    [Authorize(Policy = "TicketRead")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateTicketCommentDto dto)
    {
        await EnsureIfMatchOrThrowAsync(id);

        var userId = User.GetUserId();
        var comment = await _comments.AddAsync(id, userId, dto.Text, dto.AttachmentUrls);
        return CreatedAtAction(nameof(GetComments), new { id }, comment);
    }

    [HttpGet("{id:guid}/history")]
    [Authorize(Policy = "TicketRead")]
    public async Task<IActionResult> History(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        var events = await _events.GetByTicketAsync(id, userId, isAdmin, page, pageSize);
        return Ok(events);
    }

    // -----------------------------
    // Mutazioni con If-Match
    // -----------------------------

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "TicketWrite")]
    [RequireIfMatch]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTicketDto dto)
    {
        await EnsureIfMatchOrThrowAsync(id);

        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        await _service.UpdateAsync(id, dto, userId, isAdmin);
        return NoContent();
    }

    [HttpPut("{id:guid}/assign")]
    [Authorize(Policy = "TicketAssign")]
    [RequireIfMatch]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignTicketDto dto)
    {
        await EnsureIfMatchOrThrowAsync(id);

        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        await _service.AssignAsync(id, dto, userId, isAdmin);
        return NoContent();
    }

    [HttpPut("{id:guid}/close")]
    [Authorize(Policy = "TicketWrite")]
    [RequireIfMatch]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> Close(Guid id)
    {
        await EnsureIfMatchOrThrowAsync(id);

        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        await _service.CloseAsync(id, userId, isAdmin);
        return NoContent();
    }

    [HttpPut("{id:guid}/priority")]
    [Authorize(Policy = "TicketWrite")]
    [RequireIfMatch]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> UpdatePriority(Guid id, [FromBody] UpdateTicketPriorityDto dto)
    {
        await EnsureIfMatchOrThrowAsync(id);

        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        await _service.UpdatePriorityAsync(id, dto, userId, isAdmin);
        return NoContent();
    }

    [HttpPut("{id:guid}/due-date")]
    [Authorize(Policy = "TicketWrite")]
    [RequireIfMatch]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> UpdateDueDate(Guid id, [FromBody] UpdateTicketDueDateDto dto)
    {
        await EnsureIfMatchOrThrowAsync(id);

        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        await _service.UpdateDueDateAsync(id, dto, userId, isAdmin);
        return NoContent();
    }

    [HttpDelete("{id:guid}/due-date")]
    [Authorize(Policy = "TicketWrite")]
    [RequireIfMatch]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> ClearDueDate(Guid id)
    {
        await EnsureIfMatchOrThrowAsync(id);

        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        await _service.ClearDueDateAsync(id, userId, isAdmin);
        return NoContent();
    }

    // Helper: controlla header If-Match contro ETag corrente
    private async Task EnsureIfMatchOrThrowAsync(Guid ticketId)
    {
        var ifMatch = Request.Headers.IfMatch.ToString();
        if (string.IsNullOrWhiteSpace(ifMatch))
            throw new PreconditionRequiredException();

        var current = await _repo.GetByIdNoTrackingAsync(ticketId) ?? throw new KeyNotFoundException("Ticket non trovato.");
        var expected = TicketETag.Compute(current);

        if (!TicketETag.IfMatchSatisfied(ifMatch, expected))
            throw new PreconditionFailedException();
    }

}
