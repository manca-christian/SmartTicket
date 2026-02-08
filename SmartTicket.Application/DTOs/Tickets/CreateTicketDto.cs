using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.DTOs.Tickets;

public class CreateTicketDto
{
    [Required(ErrorMessage = "Title è obbligatorio.")]
    [MinLength(3, ErrorMessage = "Title deve avere almeno 3 caratteri.")]
    [MaxLength(100, ErrorMessage = "Title massimo 100 caratteri.")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Description è obbligatoria.")]
    [MinLength(3, ErrorMessage = "Description deve avere almeno 3 caratteri.")]
    [MaxLength(2000, ErrorMessage = "Description massimo 2000 caratteri.")]
    public string Description { get; set; } = null!;

    public List<string> AttachmentUrls { get; set; } = new();
}
