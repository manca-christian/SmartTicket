using SmartTicket.Application.DTOs.Auth;

namespace SmartTicket.Application.Interfaces;

public interface IAuthService
{
    Task<(AuthResponseDto response, string refreshToken)> RegisterAsync(RegisterDto dto);
    Task<(AuthResponseDto response, string refreshToken)> LoginAsync(LoginDto dto);
    Task<(AuthResponseDto response, string refreshToken)> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}
