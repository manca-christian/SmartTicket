using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.DTOs.Tickets;

public class CreateTicketCommentDto
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Text { get; set; } = default!;

    public List<string> AttachmentUrls { get; set; } = new();
}
