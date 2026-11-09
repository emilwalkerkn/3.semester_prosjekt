namespace SemesterProsjekt.Services;

public static class OrganizationEmailDomainService
{
    public static bool IsAllowed(
        string? email,
        string? allowedDomain)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(allowedDomain))
        {
            return false;
        }

        var normalizedEmail = email.Trim();
        var normalizedDomain = allowedDomain.Trim();

        // E-postadressen må ha nøyaktig én @.
        var atIndex = normalizedEmail.IndexOf('@');

        if (atIndex <= 0 ||
            atIndex != normalizedEmail.LastIndexOf('@') ||
            atIndex == normalizedEmail.Length - 1)
        {
            return false;
        }

        var emailDomain = normalizedEmail[(atIndex + 1)..];

        // Krever nøyaktig domenematch.
        return string.Equals(
            emailDomain,
            normalizedDomain,
            StringComparison.OrdinalIgnoreCase);
    }
}