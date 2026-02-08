using System.Net.Http.Headers;

namespace SmartTicket.IntegrationTests;

public static class CookieHelper
{
    public static string? ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            return null;

        foreach (var v in values)
        {
            if (v.StartsWith(cookieName + "=", StringComparison.OrdinalIgnoreCase))
            {
                var part = v.Split(';', 2)[0];
                var eq = part.IndexOf('=');
                return eq >= 0 ? part[(eq + 1)..] : null;
            }
        }

        return null;
    }
}
