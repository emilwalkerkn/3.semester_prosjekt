
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace SemesterProsjekt.UnitTests;

public class OrganizationInvitationSecurityTests
{
    [Theory]
    [InlineData("Accepted")]
    [InlineData("Expired")]
    [InlineData("Revoked")]
    public async Task Register_RejectsInactiveInvitation(
        string invitationStatus)
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        await using var db = new ApplicationDbContext(options);

        var token = InvitationTokenService.GenerateToken();

        var organization = new Organization
        {
            OrganizationId = 1,
            Name = "Agder politidistrikt",
            Status = "Approved"
        };

        var invitation = new OrganizationInvitation
        {
            OrganizationInvitationId = 1,
            OrganizationId = 1,
            Organization = organization,
            Email = "ansatt@politiet.no",
            TokenHash = InvitationTokenService.HashToken(token),
            InvitedByUserId = "admin-1",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(22)
        };

        if (invitationStatus == "Accepted")
            invitation.AcceptedAt = DateTime.UtcNow;

        if (invitationStatus == "Expired")
            invitation.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        if (invitationStatus == "Revoked")
            invitation.RevokedAt = DateTime.UtcNow;

        db.Organizations.Add(organization);
        db.OrganizationInvitations.Add(invitation);

        await db.SaveChangesAsync();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        var configuration = new ConfigurationBuilder().Build();

        var emailSender = new InvitationEmailSender(
            configuration,
            Mock.Of<IWebHostEnvironment>()
        );

        var controller = new OrganizationInvitationController(
            db,
            userManager.Object,
            emailSender,
            configuration
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.Register(token);

        Assert.IsType<BadRequestObjectResult>(result);

        userManager.Verify(
            m => m.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
    }
    
[Theory]
[InlineData("Accepted")]
[InlineData("Expired")]
[InlineData("Revoked")]
public async Task PostRegister_RejectsInactiveInvitation(
    string invitationStatus)
{
    var options =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    await using var db = new ApplicationDbContext(options);

    var token = InvitationTokenService.GenerateToken();
    var now = DateTime.UtcNow;

    var organization = new Organization
    {
        OrganizationId = 1,
        Name = "Agder politidistrikt",
        Status = "Approved"
    };

    var invitation = new OrganizationInvitation
    {
        OrganizationInvitationId = 1,
        OrganizationId = 1,
        Organization = organization,
        Email = "ansatt@politiet.no",
        TokenHash = InvitationTokenService.HashToken(token),
        InvitedByUserId = "admin-1",
        CreatedAt = now.AddHours(-2),
        ExpiresAt = now.AddHours(22)
    };

    if (invitationStatus == "Accepted")
        invitation.AcceptedAt = now;

    if (invitationStatus == "Expired")
        invitation.ExpiresAt = now.AddMinutes(-1);

    if (invitationStatus == "Revoked")
        invitation.RevokedAt = now;

    db.Organizations.Add(organization);
    db.OrganizationInvitations.Add(invitation);

    await db.SaveChangesAsync();

    var userManager = new Mock<UserManager<ApplicationUser>>(
        Mock.Of<IUserStore<ApplicationUser>>(),
        null!, null!, null!, null!,
        null!, null!, null!, null!
    );

    var configuration = new ConfigurationBuilder().Build();

    var emailSender = new InvitationEmailSender(
        configuration,
        Mock.Of<IWebHostEnvironment>()
    );

    var controller = new OrganizationInvitationController(
        db,
        userManager.Object,
        emailSender,
        configuration
    );

    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext()
    };

    var model =
        new SemesterProsjekt.ViewModels
            .InvitedEmployeeRegisterViewModel
        {
            FirstName = "Kari",
            LastName = "Hansen",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

    // Vi kaller POST-metoden med både token og modell.
    var result = await controller.Register(token, model);

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

[Fact]
public async Task PostRegister_RejectsWrongEmailDomain()
{
    var options =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    await using var db = new ApplicationDbContext(options);

    var token = InvitationTokenService.GenerateToken();
    var now = DateTime.UtcNow;

    var organization = new Organization
    {
        OrganizationId = 1,
        Name = "Agder politidistrikt",
        Status = "Approved",
        AllowedEmailDomain = "agderpoliti.no"
    };

    var invitation = new OrganizationInvitation
    {
        OrganizationInvitationId = 1,
        OrganizationId = 1,
        Organization = organization,
        Email = "kari@gmail.com",
        TokenHash = InvitationTokenService.HashToken(token),
        InvitedByUserId = "admin-1",
        CreatedAt = now.AddHours(-1),
        ExpiresAt = now.AddHours(23)
    };

    db.Organizations.Add(organization);
    db.OrganizationInvitations.Add(invitation);

    await db.SaveChangesAsync();

    var userManager = new Mock<UserManager<ApplicationUser>>(
        Mock.Of<IUserStore<ApplicationUser>>(),
        null!, null!, null!, null!,
        null!, null!, null!, null!
    );

    var configuration = new ConfigurationBuilder().Build();

    var emailSender = new InvitationEmailSender(
        configuration,
        Mock.Of<IWebHostEnvironment>()
    );

    var controller = new OrganizationInvitationController(
        db,
        userManager.Object,
        emailSender,
        configuration
    );

    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext()
    };

    var model =
        new SemesterProsjekt.ViewModels
            .InvitedEmployeeRegisterViewModel
        {
            FirstName = "Kari",
            LastName = "Hansen",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

    // Et gyldig token skal ikke omgå domenekravet.
    var result = await controller.Register(token, model);

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

    // Invitasjonen skal ikke brukes opp.
    Assert.Null(invitation.AcceptedAt);
}

[Fact]
public async Task Invite_RejectsWrongEmailDomain()
{
    var options =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    await using var db = new ApplicationDbContext(options);

    var organization = new Organization
    {
        OrganizationId = 1,
        Name = "Agder politidistrikt",
        Status = "Approved",
        AllowedEmailDomain = "agderpoliti.no"
    };

    var admin = new ApplicationUser
    {
        Id = "admin-1",
        UserName = "admin@agderpoliti.no",
        Email = "admin@agderpoliti.no",
        OrganizationId = 1
    };

    db.Organizations.Add(organization);
    db.Users.Add(admin);

    await db.SaveChangesAsync();

    var userManager = new Mock<UserManager<ApplicationUser>>(
        Mock.Of<IUserStore<ApplicationUser>>(),
        null!, null!, null!, null!,
        null!, null!, null!, null!
    );

    userManager
        .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
        .ReturnsAsync(admin);

    var configuration = new ConfigurationBuilder().Build();

    var emailSender = new InvitationEmailSender(
        configuration,
        Mock.Of<IWebHostEnvironment>()
    );

    var controller = new OrganizationInvitationController(
        db,
        userManager.Object,
        emailSender,
        configuration
    );

    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext()
    };
    
    controller.TempData = new TempDataDictionary(
        controller.HttpContext,
        Mock.Of<ITempDataProvider>()
    );

    // Administratoren forsøker å invitere feil domene.
    var result = await controller.Invite("kari@gmail.com");

    Assert.IsType<RedirectToActionResult>(result);

    // Ingen invitasjon skal være opprettet.
    Assert.Empty(await db.OrganizationInvitations.ToListAsync());

    // Administratoren skal få en feilmelding.
    Assert.Equal(
        "E-postadressen må tilhøre organisasjonens " +
        "godkjente jobbdomene.",
        controller.TempData["Error"]);
}

}
