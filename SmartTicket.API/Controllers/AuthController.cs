using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Linq;
using SmartTicket.API.Security;
using SmartTicket.Application.DTOs.Auth;
using SmartTicket.Application.Interfaces;

namespace SmartTicket.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [EnableRateLimiting("auth-login")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (response, refresh) = await _auth.RegisterAsync(dto);
        SetRefreshCookie(refresh);
        return Ok(response);
    }

    [EnableRateLimiting("auth-login")]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (response, refresh) = await _auth.LoginAsync(dto);
        SetRefreshCookie(refresh);
        return Ok(response);
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[AuthCookies.RefreshCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token mancante." });

        var (response, newRefresh) = await _auth.RefreshAsync(refreshToken);
        SetRefreshCookie(newRefresh);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[AuthCookies.RefreshCookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await _auth.LogoutAsync(refreshToken);

        Response.Cookies.Delete(AuthCookies.RefreshCookieName);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var userId = User.GetUserId();
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return Ok(new MeDto(userId, email, role));
    }

    [Authorize]
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        Guid? userId = null;

        try
        {
            userId = User.GetUserId();
        }
        catch
        {
        }

        return Ok(new
        {
            userId,
            isAdmin = User.IsAdmin(),
            claims
        });
    }

    private void SetRefreshCookie(string token)
    {
        Response.Cookies.Append(AuthCookies.RefreshCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth/refresh",
            Expires = DateTimeOffset.UtcNow.AddDays(14)
        });
    }
}
