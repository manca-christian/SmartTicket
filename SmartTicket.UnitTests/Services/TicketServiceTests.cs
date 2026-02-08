using FluentAssertions;
using Moq;
using SmartTicket.Application.DTOs.Common;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Application.Interfaces;
using SmartTicket.Application.Services;
using SmartTicket.Domain.Entities;
using SmartTicket.Domain.Enums;

namespace SmartTicket.UnitTests.Services;

public sealed class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _repo = new(MockBehavior.Strict);
    private readonly Mock<ITicketAttachmentRepository> _attachments = new(MockBehavior.Strict);
    private readonly Mock<ITicketEventWriter> _eventWriter = new(MockBehavior.Strict);
    private readonly TicketService _sut;

    public TicketServiceTests()
    {
        _sut = new TicketService(_repo.Object, _attachments.Object, _eventWriter.Object);
    }

    [Fact]
    public async Task CreateAsync_crea_ticket_salva_e_ritorna_id()
    {
        var userId = Guid.NewGuid();
        var dto = new CreateTicketDto { Title = "Titolo", Description = "Descrizione" };

        Ticket? captured = null;

        _repo.Setup(r => r.AddAsync(It.IsAny<Ticket>()))
            .Callback<Ticket>(t => captured = t)
            .Returns(Task.CompletedTask);

        _attachments.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<TicketAttachment>>()))
            .Returns(Task.CompletedTask);

        _eventWriter.Setup(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        _repo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var id = await _sut.CreateAsync(dto, userId);

        id.Should().NotBeEmpty();
        captured.Should().NotBeNull();
        captured!.Id.Should().Be(id);
        captured.Title.Should().Be(dto.Title);
        captured.Description.Should().Be(dto.Description);
        captured.CreatedByUserId.Should().Be(userId);
        captured.Status.Should().Be(TicketStatus.Open);
        captured.CreatedAt.Should().BeAfter(DateTime.UtcNow.AddSeconds(-5));

        _repo.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Once);
        _attachments.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<TicketAttachment>>()), Times.Never);
        _eventWriter.Verify(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMineAsync_chiama_repository_e_ritorna_risultato()
    {
        var userId = Guid.NewGuid();
        var query = new TicketQueryDto { Page = 1, PageSize = 10 };

        var expected = new PagedResult<TicketListItemDto>
        {
            Items = new List<TicketListItemDto>
            {
                new(Guid.NewGuid(), "T1", "Open", DateTime.UtcNow, userId, null)
            },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _repo.Setup(r => r.QueryMineAsync(userId, query)).ReturnsAsync(expected);

        var result = await _sut.GetMineAsync(userId, query);

        result.Should().BeSameAs(expected);

        _repo.Verify(r => r.QueryMineAsync(userId, query), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CloseAsync_se_ticket_non_esiste_lancia_KeyNotFound()
    {
        var ticketId = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync((Ticket?)null);

        Func<Task> act = () => _sut.CloseAsync(ticketId, Guid.NewGuid(), isAdmin: false);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Ticket non trovato*");

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CloseAsync_se_non_owner_e_non_admin_lancia_Unauthorized_e_non_salva()
    {
        var ticketId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var ticket = new Ticket { Id = ticketId, CreatedByUserId = ownerId, Status = TicketStatus.Open };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        Func<Task> act = () => _sut.CloseAsync(ticketId, requesterId, isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permessi*");

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CloseAsync_se_gia_closed_non_fa_SaveChanges()
    {
        var ticketId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatedByUserId = ownerId,
            Status = TicketStatus.Closed,
            ClosedAt = DateTime.UtcNow.AddDays(-1)
        };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        await _sut.CloseAsync(ticketId, ownerId, isAdmin: false);

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _repo.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CloseAsync_se_owner_chiude_ticket_e_salva()
    {
        var ticketId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var ticket = new Ticket { Id = ticketId, CreatedByUserId = ownerId, Status = TicketStatus.Open };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _eventWriter.Setup(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.CloseAsync(ticketId, ownerId, isAdmin: false);

        ticket.Status.Should().Be(TicketStatus.Closed);
        ticket.ClosedAt.Should().NotBeNull();
        ticket.ClosedAt!.Value.Should().BeAfter(DateTime.UtcNow.AddSeconds(-5));

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _eventWriter.Verify(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CloseAsync_se_admin_puo_chiudere_anche_se_non_owner()
    {
        var ticketId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var ticket = new Ticket { Id = ticketId, CreatedByUserId = ownerId, Status = TicketStatus.Open };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _eventWriter.Setup(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.CloseAsync(ticketId, requesterId, isAdmin: true);

        ticket.Status.Should().Be(TicketStatus.Closed);
        ticket.ClosedAt.Should().NotBeNull();

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _eventWriter.Verify(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_se_non_esiste_lancia_KeyNotFound()
    {
        var ticketId = Guid.NewGuid();
        var dto = new UpdateTicketDto { Title = "x", Description = "y" };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync((Ticket?)null);

        Func<Task> act = () => _sut.UpdateAsync(ticketId, dto, Guid.NewGuid(), isAdmin: false);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Ticket non trovato*");

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_se_non_owner_e_non_admin_lancia_Unauthorized_e_non_salva()
    {
        var ticketId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var dto = new UpdateTicketDto { Title = "new", Description = "new" };

        var ticket = new Ticket { Id = ticketId, CreatedByUserId = ownerId };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        Func<Task> act = () => _sut.UpdateAsync(ticketId, dto, requesterId, isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_se_owner_modifica_e_salva()
    {
        var ticketId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var ticket = new Ticket { Id = ticketId, CreatedByUserId = ownerId, Title = "old", Description = "old" };
        var dto = new UpdateTicketDto { Title = "new", Description = "new desc" };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _eventWriter.Setup(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.UpdateAsync(ticketId, dto, ownerId, isAdmin: false);

        ticket.Title.Should().Be(dto.Title);
        ticket.Description.Should().Be(dto.Description);

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _eventWriter.Verify(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_se_admin_puo_modificare_anche_se_non_owner()
    {
        var ticketId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var ticket = new Ticket { Id = ticketId, CreatedByUserId = ownerId, Title = "old", Description = "old" };
        var dto = new UpdateTicketDto { Title = "new", Description = "new desc" };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _eventWriter.Setup(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.UpdateAsync(ticketId, dto, requesterId, isAdmin: true);

        ticket.Title.Should().Be(dto.Title);
        ticket.Description.Should().Be(dto.Description);

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _eventWriter.Verify(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AssignAsync_se_non_admin_lancia_Unauthorized_e_non_chiama_repo()
    {
        var ticketId = Guid.NewGuid();
        var dto = new AssignTicketDto { AssigneeUserId = Guid.NewGuid() };

        Func<Task> act = () => _sut.AssignAsync(ticketId, dto, Guid.NewGuid(), isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Solo Admin*");

        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AssignAsync_se_ticket_non_esiste_lancia_KeyNotFound()
    {
        var ticketId = Guid.NewGuid();
        var dto = new AssignTicketDto { AssigneeUserId = Guid.NewGuid() };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync((Ticket?)null);

        Func<Task> act = () => _sut.AssignAsync(ticketId, dto, Guid.NewGuid(), isAdmin: true);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Ticket non trovato*");

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AssignAsync_se_admin_assegna_e_salva()
    {
        var ticketId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var dto = new AssignTicketDto { AssigneeUserId = assigneeId };

        var ticket = new Ticket { Id = ticketId, Status = TicketStatus.Open };

        _repo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);
        _eventWriter.Setup(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.AssignAsync(ticketId, dto, Guid.NewGuid(), isAdmin: true);

        ticket.AssignedToUserId.Should().Be(assigneeId);
        ticket.AssignedAt.Should().NotBeNull();
        ticket.AssignedAt!.Value.Should().BeAfter(DateTime.UtcNow.AddSeconds(-5));

        _repo.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _eventWriter.Verify(r => r.WriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAllAsync_se_non_admin_lancia_Unauthorized_e_non_chiama_repo()
    {
        var query = new TicketQueryDto();

        Func<Task> act = () => _sut.GetAllAsync(Guid.NewGuid(), isAdmin: false, query);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Solo Admin*");

        _repo.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAllAsync_se_admin_chiama_repo_e_ritorna_risultato()
    {
        var query = new TicketQueryDto { Page = 1, PageSize = 20 };

        var expected = new PagedResult<TicketListItemDto>
        {
            Items = new(),
            Page = 1,
            PageSize = 20,
            TotalCount = 0
        };

        _repo.Setup(r => r.QueryAllAsync(query)).ReturnsAsync(expected);

        var result = await _sut.GetAllAsync(Guid.NewGuid(), isAdmin: true, query);

        result.Should().BeSameAs(expected);

        _repo.Verify(r => r.QueryAllAsync(query), Times.Once);
        _repo.VerifyNoOtherCalls();
        _attachments.VerifyNoOtherCalls();
        _eventWriter.VerifyNoOtherCalls();
    }
}
