using Microsoft.AspNetCore.Identity;
using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;


/// Oppretter lokale demonstrasjonskontoer, behov, ressurser og matchforslag
/// når LocalDemo:Enabled er aktivert.
/// Kontrollerer eksisterende data før opprettelse, slik at vanlige omstarter
/// ikke oppretter de samme demonstrasjonsdataene på nytt.

public static class LocalDemoSeeder
{
    // Faste e-postadresser brukes til å finne igjen demonstrasjonskontoene.
    private const string AdminEmail = "admin@kriseportal.local";
    private const string NeedsActorEmail = "behov@kriseportal.local";
    private const string ResourceOwnerEmail = "ressurs@kriseportal.local";

    // Titlene brukes til å gjenkjenne eksisterende demonstrasjonsdata.
    private const string WaterNeedTitle = "Test: Drikkevann til evakueringssenter";
    private const string TransportNeedTitle = "Test: Transport av evakuerte";
    private const string WaterResourceTitle = "Test: Drikkevann på lager";
    private const string BusResourceTitle = "Test: Minibuss med sjåfør";

    // Kartpunktene lagres som GeoJSON.
    // Koordinatrekkefølgen i GeoJSON er lengdegrad, deretter breddegrad.
    private const string KristiansandCenter =
        "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Point\",\"coordinates\":[7.9956,58.1467]}}";

    private const string EvacuationCenter =
        "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Point\",\"coordinates\":[8.0184,58.1599]}}";

   
    /// Leser demokonfigurasjonen og oppretter manglende demonstrasjonsdata.
    /// Databasetabeller, kategorier og Identity-roller må finnes før metoden kjøres.
   
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        // Demonstrasjonsdata opprettes bare når dette er aktivert i konfigurasjonen.
        if (!configuration.GetValue<bool>("LocalDemo:Enabled"))
        {
            return;
        }

        // Passordet hentes fra konfigurasjonen og brukes ved opprettelse av nye kontoer.
        var password = configuration["LocalDemo:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "LocalDemo er aktivert, men LocalDemo:Password mangler.");
        }

        // Oppretter et eget tjenesteområde for oppstartsjobben.
        // Området og tilhørende tjenester frigjøres etter bruk.
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var needRepository = scope.ServiceProvider
            .GetRequiredService<INeedRepository>();

        var resourceRepository = scope.ServiceProvider
            .GetRequiredService<IResourceRepository>();

        var matchRepository = scope.ServiceProvider
            .GetRequiredService<IMatchRepository>();

        // Oppretter eller finner én demonstrasjonskonto for hver aktørrolle.
        var admin = await GetOrCreateUserAsync(
            userManager,
            AdminEmail,
            "Test",
            "Administrator",
            AppRoles.Admin,
            password);

        var needsActor = await GetOrCreateUserAsync(
            userManager,
            NeedsActorEmail,
            "Test",
            "Behovsaktør",
            AppRoles.NeedsActor,
            password);

        var resourceOwner = await GetOrCreateUserAsync(
            userManager,
            ResourceOwnerEmail,
            "Test",
            "Ressurseier",
            AppRoles.ResourceOwner,
            password);

        // Behov og ressurser knyttes til de tilhørende demonstrasjonskontoene.
        // Matchen opprettes etter at behovet og ressursen finnes.
        await SeedNeedsAsync(needRepository, needsActor.Id);
        await SeedResourcesAsync(resourceRepository, resourceOwner.Id);
        await SeedMatchAsync(needRepository, resourceRepository, matchRepository,
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>(),
            needsActor.Id, resourceOwner.Id);
    }

   
    /// Finner en bruker etter e-post eller oppretter en ny gjennom Identity.
    /// Sørger også for at brukeren har angitt rolle.
    /// Eksisterende brukeres passord og profilopplysninger endres ikke.
   
    private static async Task<ApplicationUser> GetOrCreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string firstName,
        string lastName,
        string role,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,

                // Demokontoene markeres som bekreftet uten en e-postutsending.
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName
            };

            // Identity validerer passordet og håndterer lagring av passordhash.
            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult, $"opprette demobrukeren {email}");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(roleResult, $"gi rollen {role} til {email}");
        }

        return user;
    }

   
    /// Oppretter demonstrasjonsbehov for drikkevann og transport dersom
    /// de faste titlene ikke finnes fra før.
    /// Behovene knyttes til brukeren med angitt ID.
   
    private static async Task SeedNeedsAsync(
        INeedRepository needRepository,
        string userId)
    {
        // Samler titlene for å kunne kontrollere om demonstrasjonsbehovene finnes.
        // Ordinal sammenligner titlene nøyaktig og skiller mellom store og små bokstaver.
        var existingTitles = (await needRepository.GetAllAsync())
            .Select(need => need.Title)
            .ToHashSet(StringComparer.Ordinal);

        // Type-ID-ene samsvarer med de faste kategoriene i init.sql.
        if (!existingTitles.Contains(WaterNeedTitle))
        {
            await needRepository.CreateAsync(new Need
            {
                UserId = userId,
                NeedTypeId = 1,
                Title = WaterNeedTitle,
                Description = "Evakueringssenteret trenger drikkevann de neste to døgnene.",
                GeometryType = "marker",
                GeometryData = KristiansandCenter,
                ContactPoint = "beredskap@kriseportal.local",

                // Datoene beregnes når demonstrasjonsbehovet opprettes.
                NeededFrom = DateTime.Today,
                Deadline = DateTime.Today.AddDays(2).AddHours(16),
                Quantity = 300,
                Unit = "flasker",
                Status = NeedStatuses.New,
                Priority = "High"
            });
        }

        if (!existingTitles.Contains(TransportNeedTitle))
        {
            await needRepository.CreateAsync(new Need
            {
                UserId = userId,
                NeedTypeId = 2,
                Title = TransportNeedTitle,
                Description = "Det trengs transport mellom oppmøtestedet og evakueringssenteret.",
                GeometryType = "marker",
                GeometryData = EvacuationCenter,
                ContactPoint = "transport@kriseportal.local",
                NeededFrom = DateTime.Today.AddHours(10),
                Deadline = DateTime.Today.AddDays(1).AddHours(20),
                Quantity = 2,
                Unit = "kjøretøy",
                Status = NeedStatuses.InProgress,
                Priority = "High"
            });
        }
    }

   
    /// Oppretter demonstrasjonsressurser for drikkevann og transport dersom
    /// de faste titlene ikke finnes fra før.
    /// Ressursene knyttes til brukeren med angitt ID.
   
    private static async Task SeedResourcesAsync(
        IResourceRepository resourceRepository,
        string userId)
    {
        var existingTitles = (await resourceRepository.GetAllAsync())
            .Select(resource => resource.Title)
            .ToHashSet(StringComparer.Ordinal);

        if (!existingTitles.Contains(WaterResourceTitle))
        {
            await resourceRepository.CreateAsync(new Resource
            {
                UserId = userId,
                ResourceTypeId = 3,
                Title = WaterResourceTitle,
                Description = "Flaskevann som kan leveres til evakueringssenteret samme dag.",
                GeometryType = "marker",
                GeometryData = KristiansandCenter,
                ContactPoint = "lager@kriseportal.local",
                AvailableFrom = DateTime.Today,
                AvailableTo = DateTime.Today.AddDays(5),
                Quantity = 500,
                Unit = "flasker",
                Status = "Available"
            });
        }

        if (!existingTitles.Contains(BusResourceTitle))
        {
            await resourceRepository.CreateAsync(new Resource
            {
                UserId = userId,
                ResourceTypeId = 1,
                Title = BusResourceTitle,
                Description = "To minibusser med sjåfører er tilgjengelige for lokal evakuering.",
                GeometryType = "marker",
                GeometryData = EvacuationCenter,
                ContactPoint = "logistikk@kriseportal.local",
                AvailableFrom = DateTime.Today.AddHours(8),
                AvailableTo = DateTime.Today.AddDays(2).AddHours(22),
                Quantity = 2,
                Unit = "kjøretøy",
                Status = "Available"
            });
        }
    }

   
    /// Klargjør ett demoforslag i hver retning, uten å tilbakestille behandlede matcher.
    /// Eldre åpne demoforslag får en avsender som del av testoppsettet.
    /// Andre brukeres matcher og forslag med kjent avsender endres ikke.
   
    private static async Task SeedMatchAsync(
        INeedRepository needRepository,
        IResourceRepository resourceRepository,
        IMatchRepository matchRepository,
        IDbConnectionFactory connectionFactory,
        string needsActorId,
        string resourceOwnerId)
    {
        var needs = (await needRepository.GetAllAsync())
            .Where(need => need.UserId == needsActorId).ToList();
        var resources = (await resourceRepository.GetAllAsync())
            .Where(resource => resource.UserId == resourceOwnerId).ToList();
        var examples = new[]
        {
            (NeedTitle: WaterNeedTitle, ResourceTitle: WaterResourceTitle, Sender: needsActorId),
            (NeedTitle: TransportNeedTitle, ResourceTitle: BusResourceTitle, Sender: resourceOwnerId)
        };

        using var connection = connectionFactory.CreateConnection();
        foreach (var example in examples)
        {
            var need = needs.SingleOrDefault(item => item.Title == example.NeedTitle);
            var resource = resources.SingleOrDefault(item => item.Title == example.ResourceTitle);
            if (need is null || resource is null) continue;

            // Demoavtaler gjenopprettes ikke når de er fullført eller avslått.
            if (!await connection.ExecuteScalarAsync<bool>(
                "SELECT COUNT(*) > 0 FROM `Match` WHERE NeedId = @NeedId AND ResourceId = @ResourceId;",
                new { need.NeedId, resource.ResourceId }))
            {
                var reservations = (await matchRepository.GetReservationsAsync()).ToList();
                if (!MatchAvailability.CanMatchNeed(need, reservations) ||
                    !MatchAvailability.CanMatchResource(resource, reservations)) continue;

                await matchRepository.CreateAsync(new Match
                {
                    NeedId = need.NeedId,
                    ResourceId = resource.ResourceId,
                    ProposedByUserId = example.Sender,
                    Status = MatchStatuses.Proposed,
                    MatchedAt = DateTime.UtcNow.AddHours(-1),
                    Notes = "Testmatch for å vise matchoversikten."
                });
            }
            else
            {
                // Bare faste demopar eid av demokontoene får testavsenderen.
                // Status, tidspunkt og eksisterende avsender beholdes.
                await connection.ExecuteAsync(
                    """
                    UPDATE `Match` m
                    INNER JOIN Needs n ON n.NeedId = m.NeedId
                    INNER JOIN Resources r ON r.ResourceId = m.ResourceId
                    SET m.ProposedByUserId = @Sender
                    WHERE m.NeedId = @NeedId AND m.ResourceId = @ResourceId
                      AND n.UserId = @NeedsActorId AND r.UserId = @ResourceOwnerId
                      AND m.ProposedByUserId IS NULL AND m.Status = @ProposedStatus;
                    """,
                    new
                    {
                        Sender = example.Sender, NeedId = need.NeedId,
                        ResourceId = resource.ResourceId, NeedsActorId = needsActorId,
                        ResourceOwnerId = resourceOwnerId, ProposedStatus = MatchStatuses.Proposed
                    });
            }
        }
    }

   
    /// Kontrollerer resultatet av en Identity-operasjon.
    /// Avbryter med en beskrivende feilmelding dersom operasjonen mislykkes.
   
    private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            ", ",
            result.Errors.Select(error => error.Description));

        throw new InvalidOperationException(
            $"Kunne ikke {operation}: {errors}");
    }
}
