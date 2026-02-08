using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.DTOs.Tickets;

public class UpdateTicketDto
{
    [Required]
    public string Title { get; set; } = default!;

    [Required]
    public string Description { get; set; } = default!;
}
