
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Models;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Data;
using Xunit;

namespace SemesterProsjekt.UnitTests;

public class OrganizationAdminSecurityTests
{
    [Fact]
    public async Task Admin_CannotRemoveRole_FromAnotherOrganization()
    {
        // Administratoren tilhører Agder (organisasjon 1).
        var admin = new ApplicationUser
        {
            Id = "admin-agder",
            OrganizationId = 1
        };

        // Ansatten tilhører Nord (organisasjon 2).
        var employee = new ApplicationUser
        {
            Id = "employee-nord",
            OrganizationId = 2
        };

        // Lager en falsk UserManager for testen.
        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        // Simulerer innlogget administrator.
        userManager
            .Setup(m => m.GetUserAsync(
                It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(admin);

        // Simulerer at vi finner Nord-brukeren.
        userManager
            .Setup(m => m.FindByIdAsync("employee-nord"))
            .ReturnsAsync(employee);

        // Lager controlleren uten å koble til databasen.
        // Denne handlingen bruker bare UserManager.
        var controller = new OrganizationAdminController(
            userManager.Object,
            null!
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Agder-administratoren forsøker å endre Nord-brukeren.
        var result = await controller.RemoveNeedsActorRole(
            "employee-nord");

        // Vi forventer 403 Forbidden.
        Assert.IsType<ForbidResult>(result);

        // Bekreft at rollen ALDRI forsøkes fjernet.
        userManager.Verify(
            m => m.RemoveFromRoleAsync(
                It.IsAny<ApplicationUser>(),
                AppRoles.NeedsActor),
            Times.Never);
    }
    
    [Theory]
    [InlineData("Rejected")]
    [InlineData("Pending")]
    public async Task Admin_CannotRemoveRole_WhenOrganizationIsNotApproved(
        string organizationStatus)
    {
        // Opprett en testdatabase i minnet.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        // Organisasjonen er enten Rejected eller Pending.
        db.Organizations.Add(new Organization
        {
            OrganizationId = 1,
            Name = "Agder politidistrikt",
            Status = organizationStatus
        });

        await db.SaveChangesAsync();

        // Administrator og ansatt tilhører samme organisasjon.
        var admin = new ApplicationUser
        {
            Id = "admin-agder",
            OrganizationId = 1
        };

        var employee = new ApplicationUser
        {
            Id = "employee-agder",
            OrganizationId = 1
        };

        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        userManager
            .Setup(m => m.GetUserAsync(
                It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(admin);

        userManager
            .Setup(m => m.FindByIdAsync("employee-agder"))
            .ReturnsAsync(employee);

        var controller = new OrganizationAdminController(
            userManager.Object,
            db
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Administrator forsøker å fjerne rollen.
        var result = await controller.RemoveNeedsActorRole(
            "employee-agder");

        // Tilgangen skal nektes.
        Assert.IsType<ForbidResult>(result);

        // Rollen skal aldri forsøkes fjernet.
        userManager.Verify(
            m => m.RemoveFromRoleAsync(
                It.IsAny<ApplicationUser>(),
                AppRoles.NeedsActor),
            Times.Never);
    }

}
