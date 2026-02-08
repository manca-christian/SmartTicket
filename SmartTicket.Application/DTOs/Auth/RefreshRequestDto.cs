using System.ComponentModel.DataAnnotations;

namespace SmartTicket.Application.DTOs.Auth;

public class RefreshRequestDto
{
    public string? RefreshToken { get; set; }
}
