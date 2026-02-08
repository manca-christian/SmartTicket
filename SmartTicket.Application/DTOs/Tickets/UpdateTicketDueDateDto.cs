using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.DTOs.Tickets;

public class UpdateTicketDueDateDto
{
    [Required]
    public DateTime DueAt { get; set; }
}
