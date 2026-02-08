using System.Security.Claims;

namespace SmartTicket.API.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? user.FindFirstValue("uid");

        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("UserId mancante nel token");

        return userId;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.IsInRole("Admin") ||
           user.Claims.Any(c => c.Type == "role" &&
               string.Equals(c.Value, "Admin", StringComparison.OrdinalIgnoreCase));
}
