namespace SmartTicket.Application.DTOs.Auth;

public record MeDto(
    Guid UserId,
    string Email,
    string Role
);
