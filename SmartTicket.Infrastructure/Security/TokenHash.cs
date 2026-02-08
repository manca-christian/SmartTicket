using System.Security.Cryptography;
using System.Text;

namespace SmartTicket.Infrastructure.Security;

public static class TokenHash
{
    public static string Sha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes); 
    }
}
