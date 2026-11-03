using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

/// <summary>
/// Oppretter lokal demonstrasjonsdata når LocalDemo:Enabled er aktivert.
/// Seeder-koden er idempotent: den kan kjøres flere ganger uten dupliserte brukere,
/// behov, ressurser eller matcher.
/// </summary>
public static class LocalDemoSeeder
{
    private const string AdminEmail = "admin@kriseportal.local";
    private const string NeedsActorEmail = "behov@kriseportal.local";
    private const string ResourceOwnerEmail = "ressurs@kriseportal.local";

    private const string WaterNeedTitle = "Test: Drikkevann til evakueringssenter";
    private const string TransportNeedTitle = "Test: Transport av evakuerte";
    private const string WaterResourceTitle = "Test: Drikkevann på lager";
    private const string BusResourceTitle = "Test: Minibuss med sjåfør";

    private const string KristiansandCenter =
        "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Point\",\"coordinates\":[7.9956,58.1467]}}";

    private const string EvacuationCenter =
        "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Point\",\"coordinates\":[8.0184,58.1599]}}";

    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("LocalDemo:Enabled"))
        {
            return;
        }

        var password = configuration["LocalDemo:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "LocalDemo er aktivert, men LocalDemo:Password mangler.");
        }

        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        var needRepository = scope.ServiceProvider
            .GetRequiredService<INeedRepository>();
        var resourceRepository = scope.ServiceProvider
            .GetRequiredService<IResourceRepository>();
        var matchRepository = scope.ServiceProvider
            .GetRequiredService<IMatchRepository>();

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

        await SeedNeedsAsync(needRepository, needsActor.Id);
        await SeedResourcesAsync(resourceRepository, resourceOwner.Id);
        await SeedMatchAsync(needRepository, resourceRepository, matchRepository);
    }

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
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName
            };

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

    private static async Task SeedNeedsAsync(INeedRepository needRepository, string userId)
    {
        var existingTitles = (await needRepository.GetAllAsync())
            .Select(need => need.Title)
            .ToHashSet(StringComparer.Ordinal);

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

    private static async Task SeedMatchAsync(
        INeedRepository needRepository,
        IResourceRepository resourceRepository,
        IMatchRepository matchRepository)
    {
        var waterNeed = (await needRepository.GetAllAsync())
            .Single(need => need.Title == WaterNeedTitle);
        var waterResource = (await resourceRepository.GetAllAsync())
            .Single(resource => resource.Title == WaterResourceTitle);

        if (await matchRepository.ExistsAsync(waterNeed.NeedId, waterResource.ResourceId))
        {
            return;
        }

        await matchRepository.CreateAsync(new Match
        {
            NeedId = waterNeed.NeedId,
            ResourceId = waterResource.ResourceId,
            Status = MatchStatuses.Proposed,
            MatchedAt = DateTime.UtcNow.AddHours(-1),
            Notes = "Testmatch for å vise matchoversikten."
        });
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Kunne ikke {operation}: {errors}");
    }
}
