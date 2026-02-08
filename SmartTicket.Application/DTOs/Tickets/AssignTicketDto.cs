using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.DTOs.Tickets;

public class AssignTicketDto
{
    [Required]
    public Guid AssigneeUserId { get; set; }
}
