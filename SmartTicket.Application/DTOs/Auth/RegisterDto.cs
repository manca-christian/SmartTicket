using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Email è obbligatoria.")]
    [EmailAddress(ErrorMessage = "Email non valida.")]
    [MaxLength(254, ErrorMessage = "Email troppo lunga.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password è obbligatoria.")]
    [MinLength(8, ErrorMessage = "Password minimo 8 caratteri.")]
    [MaxLength(100, ErrorMessage = "Password massimo 100 caratteri.")]
    public string Password { get; set; } = null!;
}
