using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SmartTicket.Domain.Entities;

namespace SmartTicket.API.Security;

public static class TicketETag
{
    public static string Compute(Ticket t)
    {
        var canonical = string.Join("|", new[]
        {
            t.Id.ToString("N"),
            t.Title ?? "",
            t.Description ?? "",
            t.Status.ToString(),
            t.Priority.ToString(),
            t.CreatedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            (t.DueAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)) ?? "",
            (t.ClosedAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)) ?? "",
            t.CreatedByUserId.ToString("N"),
            (t.AssignedToUserId?.ToString("N")) ?? "",
            (t.AssignedAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)) ?? ""
        });

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));

        var b64 = Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return $"\"{b64}\""; 
    }

    public static bool IfMatchSatisfied(string? ifMatchHeader, string expectedEtag)
    {
        if (string.IsNullOrWhiteSpace(ifMatchHeader))
            return false;

        if (ifMatchHeader.Trim() == "*")
            return true;

        var parts = ifMatchHeader.Split(',')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p));

        return parts.Any(p => string.Equals(p, expectedEtag, StringComparison.Ordinal));
    }
}
