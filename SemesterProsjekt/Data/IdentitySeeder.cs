using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles =
        [
            AppRoles.Admin,
            AppRoles.ResourceOwner,
            AppRoles.NeedsActor
        ];

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(
                    $"Kunne ikke opprette rollen '{role}': {errors}");
            }
        }
    }
}
