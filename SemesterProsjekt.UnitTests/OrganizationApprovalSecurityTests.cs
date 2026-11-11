
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace SemesterProsjekt.UnitTests;

public class OrganizationApprovalSecurityTests
{
    [Fact]
    public async Task CannotAssignAdmin_FromAnotherOrganization()
    {
        // Opprett en midlertidig testdatabase.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        // Organisasjon 1 er godkjent.
        db.Organizations.Add(new Organization
        {
            OrganizationId = 1,
            Name = "Agder politidistrikt",
            Status = "Approved"
        });

        await db.SaveChangesAsync();

        // Brukeren tilhører organisasjon 2.
        var employee = new ApplicationUser
        {
            Id = "employee-nord",
            Email = "testnord@example.com",
            OrganizationId = 2
        };

        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        userManager
            .Setup(m => m.FindByEmailAsync(
                "testnord@example.com"))
            .ReturnsAsync(employee);

        var controller = new OrganizationApprovalController(
            db,
            userManager.Object
        );

        // Forsøk å utpeke brukeren i feil organisasjon.
        var result = await controller.AssignOrganizationAdmin(
            1,
            "testnord@example.com"
        );

        // Handlingen skal avvises.
        Assert.IsType<ForbidResult>(result);

        // Administratorrollen skal aldri tildeles.
        userManager.Verify(
            m => m.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                AppRoles.OrganizationAdmin),
            Times.Never);
    }
    
    [Theory]
    [InlineData("Pending")]
    [InlineData("Rejected")]
    public async Task CannotAssignAdmin_WhenOrganizationIsNotApproved(
        string status)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        db.Organizations.Add(new Organization
        {
            OrganizationId = 1,
            Name = "Testorganisasjon",
            Status = status
        });

        await db.SaveChangesAsync();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        var controller = new OrganizationApprovalController(
            db,
            userManager.Object
        );

        var result = await controller.AssignOrganizationAdmin(
            1,
            "test@example.com"
        );

        // Organisasjonen er ikke godkjent.
        Assert.IsType<ForbidResult>(result);

        // Brukeren skal ikke engang bli slått opp.
        userManager.Verify(
            m => m.FindByEmailAsync(It.IsAny<string>()),
            Times.Never);

        // Administratorrollen skal aldri tildeles.
        userManager.Verify(
            m => m.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                AppRoles.OrganizationAdmin),
            Times.Never);
    }
    
    [Fact]
    public async Task CannotAssignOrganizationAdmin_ToSystemAdmin()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        db.Organizations.Add(new Organization
        {
            OrganizationId = 1,
            Name = "Agder politidistrikt",
            Status = "Approved"
        });

        await db.SaveChangesAsync();

        var user = new ApplicationUser
        {
            Id = "system-admin",
            Email = "admin@example.com",
            OrganizationId = 1
        };

        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        userManager
            .Setup(m => m.FindByEmailAsync("admin@example.com"))
            .ReturnsAsync(user);

        // Brukeren har allerede systemadministratorrollen.
        userManager
            .Setup(m => m.IsInRoleAsync(user, AppRoles.Admin))
            .ReturnsAsync(true);

        var controller = new OrganizationApprovalController(
            db,
            userManager.Object
        );

        // Metoden bruker TempData til feilmeldinger.
        controller.TempData = new Mock<ITempDataDictionary>().Object;

        var result = await controller.AssignOrganizationAdmin(
            1,
            "admin@example.com"
        );

        // Brukeren skal sendes tilbake til oversikten.
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Approved", redirect.ActionName);

        // OrganizationAdmin-rollen skal aldri tildeles.
        userManager.Verify(
            m => m.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                AppRoles.OrganizationAdmin),
            Times.Never);
    }


}
