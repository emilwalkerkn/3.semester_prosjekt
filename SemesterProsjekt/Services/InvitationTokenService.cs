using System.Security.Cryptography;
using System.Text;

namespace SemesterProsjekt.Services;

public static class InvitationTokenService
{
    // Lager en sikker og tilfeldig invitasjonskode.
    public static string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToHexString(randomBytes);
    }

    // Lager en SHA-256-hash av koden.
    public static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}