
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public static class OrganizationAdminTestSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var organization = await db.Organizations
            .SingleOrDefaultAsync(o =>
                o.Name == "Agder politidistrikt" &&
                o.Status == "Approved");

        if (organization == null)
            throw new InvalidOperationException(
                "Testorganisasjonen finnes ikke.");

        // Finn testadministratoren
        var user = await userManager.FindByEmailAsync(
            "politiadmin@agderpoliti.no");

        if (user == null)
            throw new InvalidOperationException(
                "Testadministratoren finnes ikke.");

        // Knytt administratoren til organisasjonen
        user.OrganizationId = organization.OrganizationId;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            throw new InvalidOperationException(
                "Kunne ikke knytte administratoren til organisasjonen.");

        // Gi administratorrollen hvis den mangler
        if (!await userManager.IsInRoleAsync(
                user, AppRoles.OrganizationAdmin))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user, AppRoles.OrganizationAdmin);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    "Kunne ikke tildele administratorrollen.");
        }

        // Finn testansatten
        var employee = await userManager.FindByEmailAsync(
            "ansatt1@agderpoliti.no");

        if (employee == null)
            throw new InvalidOperationException(
                "Testansatten finnes ikke.");

        if (employee.OrganizationId != organization.OrganizationId)
            throw new InvalidOperationException(
                "Testansatten tilhører ikke Agder politidistrikt.");

        // Gi behovsaktørrollen hvis den mangler
        if (!await userManager.IsInRoleAsync(
                employee, AppRoles.NeedsActor))
        {
            var result = await userManager.AddToRoleAsync(
                employee, AppRoles.NeedsActor);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Kunne ikke tildele behovsaktørrollen.");
        }
        // Finn testbrukeren i Nord.
        var northEmployee = await userManager.FindByEmailAsync(
            "testnord@example.com");

        if (northEmployee == null)
            throw new InvalidOperationException(
                "Testbrukeren i Nord finnes ikke.");

        if (northEmployee.OrganizationId == organization.OrganizationId)
            throw new InvalidOperationException(
                "Nord-brukeren må tilhøre en annen organisasjon.");

        if (!await userManager.IsInRoleAsync(
                northEmployee, AppRoles.NeedsActor))
        {
            var result = await userManager.AddToRoleAsync(
                northEmployee, AppRoles.NeedsActor);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Kunne ikke tildele behovsaktørrollen i Nord.");
        }
    }
}
