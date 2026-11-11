using MySqlConnector;
using Testcontainers.MariaDb;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using SemesterProsjekt.Controllers;
using Microsoft.Extensions.DependencyInjection;
using SemesterProsjekt.ViewModels;
using Xunit;

namespace SemesterProsjekt.UnitTests;

public class MariaDbIntegrationTests
{
    [Fact]
    public async Task MariaDbContainer_CanConnectToDatabase()
    {
        // Oppretter en separat testdatabase.
        await using var container = new MariaDbBuilder(
                "mariadb:10.11")
            .Build();

        // Starter databasen i Docker.
        await container.StartAsync();

        // Kobler til med Testcontainers sin tilkoblingsstreng.
        await using var connection = new MySqlConnection(
            container.GetConnectionString());

        await connection.OpenAsync();

        // Kontrollerer at tilkoblingen fungerer.
        Assert.Equal(
            System.Data.ConnectionState.Open,
            connection.State);
    }
    
[Fact]
public async Task SameInvitation_CanOnlyBeClaimedOnce()
{
    // Separat MariaDB, kun for denne testen.
    await using var container =
        new MariaDbBuilder("mariadb:10.11").Build();

    await container.StartAsync();

    var connectionString = container.GetConnectionString();

    DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(
                connectionString,
                new MariaDbServerVersion(new Version(10, 11, 0)))
            .Options;
    }

    var token = InvitationTokenService.GenerateToken();
    var tokenHash = InvitationTokenService.HashToken(token);

    // Opprett tabellene og testdataene.
    await using (var setupDb =
                 new ApplicationDbContext(CreateOptions()))
    {
        await setupDb.Database.EnsureCreatedAsync();

        setupDb.Organizations.Add(new Organization
        {
            OrganizationId = 1,
            Name = "Testorganisasjon",
            Status = "Approved"
        });

        setupDb.Users.Add(new ApplicationUser
        {
            Id = "test-admin",
            UserName = "admin@example.com",
            Email = "admin@example.com",
            OrganizationId = 1
        });

        setupDb.OrganizationInvitations.Add(
            new OrganizationInvitation
            {
                OrganizationId = 1,
                Email = "ansatt@example.com",
                TokenHash = tokenHash,
                InvitedByUserId = "test-admin",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });

        await setupDb.SaveChangesAsync();
    }

    // Hvert forsøk bruker sin egen DbContext.
    async Task<int> ClaimInvitationAsync()
    {
        await using var db =
            new ApplicationDbContext(CreateOptions());

        var now = DateTime.UtcNow;

        return await db.OrganizationInvitations
            .Where(i =>
                i.TokenHash == tokenHash &&
                i.AcceptedAt == null &&
                i.RevokedAt == null &&
                i.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.AcceptedAt, now));
    }

    // Forsøk å bruke samme invitasjon samtidig.
    var results = await Task.WhenAll(
        ClaimInvitationAsync(),
        ClaimInvitationAsync());

    // Én forespørsel får oppdatere én rad.
    Assert.Equal(1, results.Count(result => result == 1));

    // Den andre må få null oppdaterte rader.
    Assert.Equal(1, results.Count(result => result == 0));

    // Kontroller sluttresultatet i databasen.
    await using var verifyDb =
        new ApplicationDbContext(CreateOptions());

    var savedInvitation =
        await verifyDb.OrganizationInvitations.SingleAsync();

    Assert.NotNull(savedInvitation.AcceptedAt);
}

[Fact]
public async Task FailedRegistration_RollsBackInvitationClaim()
{
    await using var container =
        new MariaDbBuilder("mariadb:10.11").Build();

    await container.StartAsync();

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseMySql(
            container.GetConnectionString(),
            new MariaDbServerVersion(new Version(10, 11, 0)))
        .Options;

    var token = InvitationTokenService.GenerateToken();
    var tokenHash = InvitationTokenService.HashToken(token);

    // Opprett testdata.
    await using (var setupDb =
                 new ApplicationDbContext(options))
    {
        await setupDb.Database.EnsureCreatedAsync();

        setupDb.Organizations.Add(new Organization
        {
            OrganizationId = 1,
            Name = "Testorganisasjon",
            Status = "Approved"
        });

        setupDb.Users.Add(new ApplicationUser
        {
            Id = "test-admin",
            UserName = "admin@example.com",
            Email = "admin@example.com",
            OrganizationId = 1
        });

        setupDb.OrganizationInvitations.Add(
            new OrganizationInvitation
            {
                OrganizationId = 1,
                Email = "ansatt@example.com",
                TokenHash = tokenHash,
                InvitedByUserId = "test-admin",
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });

        await setupDb.SaveChangesAsync();
    }

    // Simuler at registreringen starter, men ikke fullføres.
    await using (var db =
                 new ApplicationDbContext(options))
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;

        var claimed = await db.OrganizationInvitations
            .Where(i =>
                i.TokenHash == tokenHash &&
                i.AcceptedAt == null &&
                i.RevokedAt == null &&
                i.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.AcceptedAt, now));

        Assert.Equal(1, claimed);

        // Simuler en feil ved å rulle tilbake transaksjonen.
        await transaction.RollbackAsync();
    }

    // Invitasjonen skal fortsatt være tilgjengelig.
    await using var verifyDb =
        new ApplicationDbContext(options);

    var invitation = await verifyDb.OrganizationInvitations
        .SingleAsync(i => i.TokenHash == tokenHash);

    Assert.Null(invitation.AcceptedAt);
    Assert.Null(invitation.RevokedAt);
}

[Fact]
public async Task Admin_CannotRevokeInvitation_FromAnotherOrganization()
{
    await using var container =
        new MariaDbBuilder("mariadb:10.11").Build();

    await container.StartAsync();

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseMySql(
            container.GetConnectionString(),
            new MariaDbServerVersion(new Version(10, 11, 0)))
        .Options;

    await using var db = new ApplicationDbContext(options);

    await db.Database.EnsureCreatedAsync();

    // To forskjellige organisasjoner.
    db.Organizations.AddRange(
        new Organization
        {
            OrganizationId = 1,
            Name = "Agder politidistrikt",
            Status = "Approved"
        },
        new Organization
        {
            OrganizationId = 2,
            Name = "Nord politidistrikt",
            Status = "Approved"
        });

    var admin = new ApplicationUser
    {
        Id = "admin-agder",
        UserName = "admin@agder.no",
        Email = "admin@agder.no",
        OrganizationId = 1
    };

    db.Users.Add(admin);

    // Invitasjonen tilhører Nord, ikke Agder.
    var invitation = new OrganizationInvitation
    {
        OrganizationId = 2,
        Email = "ansatt@nord.no",
        TokenHash = InvitationTokenService.HashToken(
            InvitationTokenService.GenerateToken()),
        InvitedByUserId = admin.Id,
        ExpiresAt = DateTime.UtcNow.AddHours(24)
    };

    db.OrganizationInvitations.Add(invitation);

    await db.SaveChangesAsync();

    var userManager = new Mock<UserManager<ApplicationUser>>(
        Mock.Of<IUserStore<ApplicationUser>>(),
        null!, null!, null!, null!,
        null!, null!, null!, null!
    );

    userManager
        .Setup(m => m.GetUserAsync(
            It.IsAny<ClaimsPrincipal>()))
        .ReturnsAsync(admin);

    var configuration = new ConfigurationBuilder().Build();

    var emailSender = new InvitationEmailSender(
        configuration,
        Mock.Of<IWebHostEnvironment>());

    var controller = new OrganizationInvitationController(
        db,
        userManager.Object,
        emailSender,
        configuration);

    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext()
    };

    controller.TempData =
        new Mock<ITempDataDictionary>().Object;

    // Agder-admin forsøker å tilbakekalle Nord-invitasjonen.
    var result = await controller.Revoke(
        invitation.OrganizationInvitationId);

    Assert.IsType<RedirectToActionResult>(result);

    // Last verdien direkte fra databasen på nytt.
    await db.Entry(invitation).ReloadAsync();

    // Nord-invitasjonen må fortsatt være aktiv.
    Assert.Null(invitation.RevokedAt);
}

[Fact]
public async Task FailedRoleAssignment_RollsBackEntireRegistration()
{
    await using var container =
        new MariaDbBuilder("mariadb:10.11").Build();

    await container.StartAsync();

    // Identity og invitasjonene skal bruke samme database.
    var services = new ServiceCollection();

    services.AddLogging();

    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(
            container.GetConnectionString(),
            new MariaDbServerVersion(new Version(10, 11, 0))));

    services
        .AddIdentityCore<ApplicationUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    using var provider = services.BuildServiceProvider();

    var token = InvitationTokenService.GenerateToken();
    var tokenHash = InvitationTokenService.HashToken(token);

    // Opprett organisasjon, administrator og invitasjon.
    await using (var setupScope = provider.CreateAsyncScope())
    {
        var db = setupScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();

        db.Organizations.Add(new Organization
        {
            OrganizationId = 1,
            Name = "Agder politidistrikt",
            Status = "Approved",
            AllowedEmailDomain = "politiet.no"

                
        });

        db.Users.Add(new ApplicationUser
        {
            Id = "admin-1",
            UserName = "admin@example.com",
            Email = "admin@example.com",
            OrganizationId = 1
        });

        db.OrganizationInvitations.Add(
            new OrganizationInvitation
            {
                OrganizationId = 1,
                Email = "kari@politiet.no",
                TokenHash = tokenHash,
                InvitedByUserId = "admin-1",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });

        await db.SaveChangesAsync();
    }

    // Kjør den faktiske POST-metoden.
    await using (var testScope = provider.CreateAsyncScope())
    {
        var db = testScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var realUserManager = testScope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // Vi bruker ekte Identity-kontoopprettelse,
        // men simulerer feil ved rolletildeling.
        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        userManager
            .Setup(m => m.FindByEmailAsync(
                It.IsAny<string>()))
            .Returns((string email) =>
                realUserManager.FindByEmailAsync(email));

        userManager
            .Setup(m => m.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()))
            .Returns((ApplicationUser user, string password) =>
                realUserManager.CreateAsync(user, password));

        userManager
            .Setup(m => m.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                AppRoles.NeedsActor))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Simulert feil ved rolletildeling."
                }));

        var configuration = new ConfigurationBuilder().Build();

        var emailSender = new InvitationEmailSender(
            configuration,
            Mock.Of<IWebHostEnvironment>());

        var controller = new OrganizationInvitationController(
            db,
            userManager.Object,
            emailSender,
            configuration);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var model = new InvitedEmployeeRegisterViewModel
        {
            FirstName = "Kari",
            LastName = "Hansen",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        var result = await controller.Register(token, model);

        // Registreringen skal avbrytes.
        Assert.IsType<ViewResult>(result);

        // Bekreft at konto-opprettelsen faktisk ble forsøkt.
        userManager.Verify(
            m => m.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Once);

        // Bekreft at rolletildelingen ble forsøkt.
        userManager.Verify(
            m => m.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                AppRoles.NeedsActor),
            Times.Once);
    }

    // Kontroller databasen med en ny DbContext.
    await using (var verifyScope = provider.CreateAsyncScope())
    {
        var db = verifyScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        // Brukerkontoen skal ikke eksistere.
        var user = await db.Users
            .SingleOrDefaultAsync(u =>
                u.Email == "kari@politiet.no");

        Assert.Null(user);

        // Invitasjonen skal fortsatt være tilgjengelig.
        var invitation = await db.OrganizationInvitations
            .SingleAsync(i => i.TokenHash == tokenHash);

        Assert.Null(invitation.AcceptedAt);
        Assert.Null(invitation.RevokedAt);
    }
}

[Fact]
public async Task ChangedOrganizationDomain_DuringRegistration_RejectsAndRollsBack()
{
    // Egen MariaDB-container for denne testen.
    await using var container =
        new MariaDbBuilder("mariadb:10.11").Build();

    await container.StartAsync();

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseMySql(
            container.GetConnectionString(),
            new MariaDbServerVersion(new Version(10, 11, 0)))
        .Options;

    var token = InvitationTokenService.GenerateToken();
    var tokenHash = InvitationTokenService.HashToken(token);

    // Opprett en organisasjon med gyldig domene
    // og en aktiv invitasjon.
    await using (var setupDb = new ApplicationDbContext(options))
    {
        await setupDb.Database.EnsureCreatedAsync();

        setupDb.Organizations.Add(new Organization
        {
            OrganizationId = 1,
            Name = "Agder politidistrikt",
            Status = "Approved",
            AllowedEmailDomain = "agderpoliti.no"
        });

        setupDb.Users.Add(new ApplicationUser
        {
            Id = "admin-1",
            UserName = "admin@agderpoliti.no",
            Email = "admin@agderpoliti.no",
            OrganizationId = 1
        });

        setupDb.OrganizationInvitations.Add(
            new OrganizationInvitation
            {
                OrganizationId = 1,
                Email = "kari@agderpoliti.no",
                TokenHash = tokenHash,
                InvitedByUserId = "admin-1",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });

        await setupDb.SaveChangesAsync();
    }

    // Holder oversikt over at domenet faktisk endres.
    var domainWasChanged = false;

    // Simuler at Heimevernet endrer domenet etter
    // controllerens første kontroll, men før transaksjonen.
    async Task<ApplicationUser?> ChangeDomainAsync()
    {
        await using var changeDb =
            new ApplicationDbContext(options);

        var updatedRows = await changeDb.Organizations
            .Where(o => o.OrganizationId == 1)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    o => o.AllowedEmailDomain,
                    "annet.no"));

        Assert.Equal(1, updatedRows);

        domainWasChanged = true;

        // Den inviterte har ikke en eksisterende konto.
        return null;
    }

    await using (var db = new ApplicationDbContext(options))
    {
        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        // Register() kaller FindByEmailAsync etter den første
        // domenekontrollen. Her simulerer vi endringen.
        userManager
            .Setup(m => m.FindByEmailAsync(
                It.IsAny<string>()))
            .Returns((string _) => ChangeDomainAsync());

        var configuration =
            new ConfigurationBuilder().Build();

        var emailSender = new InvitationEmailSender(
            configuration,
            Mock.Of<IWebHostEnvironment>());

        var controller = new OrganizationInvitationController(
            db,
            userManager.Object,
            emailSender,
            configuration);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var model = new InvitedEmployeeRegisterViewModel
        {
            FirstName = "Kari",
            LastName = "Hansen",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        // Kjør den faktiske registreringsmetoden.
        var result = await controller.Register(token, model);

        // Endringen skal ha skjedd under registreringsflyten.
        Assert.True(domainWasChanged);

        // Den siste domenekontrollen skal avvise forsøket.
        Assert.IsType<BadRequestObjectResult>(result);

        // Ingen konto skal opprettes.
        userManager.Verify(
            m => m.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);

        // Ingen rolle skal tildeles.
        userManager.Verify(
            m => m.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                AppRoles.NeedsActor),
            Times.Never);
    }

    // Bruk en ny DbContext for å kontrollere databasen.
    await using (var verifyDb = new ApplicationDbContext(options))
    {
        var organization = await verifyDb.Organizations
            .SingleAsync(o => o.OrganizationId == 1);

        // Domenet skal faktisk være endret.
        Assert.Equal("annet.no", organization.AllowedEmailDomain);

        var invitation = await verifyDb.OrganizationInvitations
            .SingleAsync(i => i.TokenHash == tokenHash);

        // Registreringen skal ikke ha brukt opp invitasjonen.
        Assert.Null(invitation.AcceptedAt);
        Assert.Null(invitation.RevokedAt);

        // Ingen ansattkonto skal eksistere.
        var user = await verifyDb.Users
            .SingleOrDefaultAsync(u =>
                u.Email == "kari@agderpoliti.no");

        Assert.Null(user);
    }
}


}