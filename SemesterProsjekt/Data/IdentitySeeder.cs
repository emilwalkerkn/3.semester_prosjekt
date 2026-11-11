using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;


/// Sørger for at applikasjonens faste roller finnes i Identity-databasen.
/// Oppretter verken brukere eller rolletildelinger til brukere.

public static class IdentitySeeder
{
 
    /// Oppretter manglende roller ved oppstart.
    /// Eksisterende roller hoppes over, slik at metoden kan kjøres flere ganger.
    
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        // Oppretter et eget tjenesteområde for oppstartsjobben.
        // using frigjør området og tilhørende tjenester etter bruk.
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        // Bruker samme rollenavn som tilgangskontrollen i controllerne.
        string[] roles =
        [
            AppRoles.Admin,
            AppRoles.OrganizationAdmin,
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
                // Avbryter oppstarten dersom en nødvendig rolle ikke kan opprettes.
                // Identity-feilene tas med for å gjøre årsaken synlig i loggene.
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Kunne ikke opprette rollen '{role}': {errors}");
            }
        }
    }
}