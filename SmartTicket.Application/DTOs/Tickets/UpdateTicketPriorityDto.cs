using System.ComponentModel.DataAnnotations;
using SmartTicket.Domain.Enums;

namespace SmartTicket.Application.DTOs.Tickets;

public class UpdateTicketPriorityDto
{
    [Required]
    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority Priority { get; set; }
}
