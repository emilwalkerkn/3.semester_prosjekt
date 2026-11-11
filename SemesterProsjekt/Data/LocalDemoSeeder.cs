
using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Models;
using Microsoft.EntityFrameworkCore;

namespace SemesterProsjekt.Data;

/// Oppretter lokale testbrukere med ulike roller.
/// Brukes til å teste innlogging og tilgangsstyring i portalen.
/// Oppretter ikke behov, ressurser eller matcher.
public static class LocalDemoSeeder
{
    // Faste e-postadresser for de tre testbrukerne.
    private const string AdminEmail = "admin@kriseportal.local";
    private const string NeedsActorEmail = "behov@kriseportal.local";
    private const string ResourceOwnerEmail = "ressurs@kriseportal.local";
    private const string OrganizationAdminEmail = "orgadmin@kriseportal.local";
    private const string DemoOrganizationName = "Kriseportal Demoorganisasjon";
    private const string DemoOrganizationDomain = "kriseportal.local";

    /// Oppretter testbrukerne dersom LocalDemo er aktivert.
    /// Eksisterende brukere blir ikke opprettet på nytt.
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        // Kontrollerer om opprettelse av testbrukere er aktivert.
        if (!configuration.GetValue<bool>("LocalDemo:Enabled"))
        {
            return;
        }

        // Henter passordet fra konfigurasjonen.
        var password = configuration["LocalDemo:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "LocalDemo er aktivert, men LocalDemo:Password mangler.");
        }

        // Oppretter et eget tjenesteområde for Identity.
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // En fast organisasjonsadministrator skal bare opprettes i et lokalt utviklingsmiljø.
var environment = scope.ServiceProvider
    .GetRequiredService<IWebHostEnvironment>();

if (environment.IsDevelopment())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var organization = await db.Organizations
        .SingleOrDefaultAsync(o => o.Name == DemoOrganizationName);

    if (organization is null)
    {
        organization = new Organization
        {
            Name = DemoOrganizationName,
            Status = "Approved",
            AllowedEmailDomain = DemoOrganizationDomain
        };

        db.Organizations.Add(organization);
        await db.SaveChangesAsync();
    }
    else if (organization.Status != "Approved" ||
             organization.AllowedEmailDomain != DemoOrganizationDomain)
    {
        throw new InvalidOperationException(
            "Demoorganisasjonen har uventet status eller e-postdomene.");
    }

    var orgAdmin = await userManager.FindByEmailAsync(
        OrganizationAdminEmail);

    if (orgAdmin is null)
    {
        orgAdmin = new ApplicationUser
        {
            UserName = OrganizationAdminEmail,
            Email = OrganizationAdminEmail,
            EmailConfirmed = true,
            FirstName = "Demo",
            LastName = "Organisasjonsadministrator",
            OrganizationId = organization.OrganizationId
        };

        var createResult = await userManager.CreateAsync(
            orgAdmin, password);

        EnsureSucceeded(
            createResult,
            $"opprette demobrukeren {OrganizationAdminEmail}");
    }
    else if (orgAdmin.OrganizationId != organization.OrganizationId)
    {
        // Ikke gi en eksisterende konto administratorrettigheter uten at den allerede tilhører riktig organisasjon.
        throw new InvalidOperationException(
            "E-postadressen til demoorganisasjonsadministratoren er allerede i bruk.");
    }

    if (!await userManager.IsInRoleAsync(
            orgAdmin, AppRoles.OrganizationAdmin))
    {
        var roleResult = await userManager.AddToRoleAsync(
            orgAdmin, AppRoles.OrganizationAdmin);

        EnsureSucceeded(
            roleResult,
            "tildele OrganizationAdmin til demobrukeren");
    }
    // Demo-organisasjonsadministratoren skal også kunne registrere behov.
    if (!await userManager.IsInRoleAsync(
            orgAdmin, AppRoles.NeedsActor))
    {
        var roleResult = await userManager.AddToRoleAsync(
            orgAdmin, AppRoles.NeedsActor);

        EnsureSucceeded(
            roleResult,
            "tildele Behovsaktør til demo-organisasjonsadministratoren");
    }
}


        // Oppretter eller finner testbrukeren for administrator.
        await GetOrCreateUserAsync(
            userManager,
            AdminEmail,
            "Test",
            "Administrator",
            AppRoles.Admin,
            password);

        // Oppretter eller finner testbrukeren for offentlig aktør.
        await GetOrCreateUserAsync(
            userManager,
            NeedsActorEmail,
            "Test",
            "Behovsaktør",
            AppRoles.NeedsActor,
            password);

        // Oppretter eller finner testbrukeren for ressurseier.
        await GetOrCreateUserAsync(
            userManager,
            ResourceOwnerEmail,
            "Test",
            "Ressurseier",
            AppRoles.ResourceOwner,
            password);
    }

    /// Finner en eksisterende testbruker eller oppretter en ny.
    /// Sørger for at brukeren har riktig rolle i systemet.
    /// Eksisterende passord og profilopplysninger endres ikke.
    private static async Task<ApplicationUser> GetOrCreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string firstName,
        string lastName,
        string role,
        string password)
    {
        // Sjekker om brukeren allerede finnes i databasen.
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,

                // Testbrukerne markeres som bekreftet uten e-postutsending.
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName
            };


            // Identity oppretter brukeren og lagrer passordet sikkert.
            var createResult = await userManager.CreateAsync(user, password);

            EnsureSucceeded(
                createResult,
                $"opprette demobrukeren {email}");
        }

        // Kontrollerer at brukeren har riktig rolle.
        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);

            EnsureSucceeded(
                roleResult,
                $"gi rollen {role} til {email}");
        }


        return user;
    }


    /// Kontrollerer om en Identity-operasjon ble gjennomført.
    /// Kaster en feilmelding dersom opprettelse eller rolletildeling mislykkes.
    private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        // Samler feilmeldingene fra Identity.
        var errors = string.Join(
            ", ",
            result.Errors.Select(error => error.Description));

        throw new InvalidOperationException(
            $"Kunne ikke {operation}: {errors}");
    }

}
