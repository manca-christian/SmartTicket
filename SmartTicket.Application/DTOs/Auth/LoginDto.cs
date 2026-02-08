using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Email è obbligatoria.")]
    [EmailAddress(ErrorMessage = "Email non valida.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password è obbligatoria.")]
    public string Password { get; set; } = null!;
}
