using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Services;

public class PortalPasswordHasher : IPasswordHasher<PortalUser>
{
    private readonly PasswordService _passwordService;

    public PortalPasswordHasher(PasswordService passwordService)
    {
        _passwordService = passwordService;
    }

    public string HashPassword(PortalUser user, string password) =>
        _passwordService.Hash(password);

    public PasswordVerificationResult VerifyHashedPassword(
        PortalUser user,
        string hashedPassword,
        string providedPassword) =>
        _passwordService.Verify(providedPassword, hashedPassword)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
}
