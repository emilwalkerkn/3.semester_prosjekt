
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;
using Xunit;

namespace SemesterProsjekt.UnitTests;

public class SettingsEmailSecurityTests
{
    [Fact]
    public async Task NeedsActor_CannotChangeEmail()
    {
        var user = new ApplicationUser
        {
            Id = "employee-1",
            FirstName = "Kari",
            LastName = "Hansen",
            Email = "kari@politiet.no",
            UserName = "kari@politiet.no"
        };

        var userManager = CreateUserManager(user);

        userManager
            .Setup(m => m.IsInRoleAsync(
                user, AppRoles.NeedsActor))
            .ReturnsAsync(true);

        var controller = CreateController(userManager);

        var profile = new ProfileSettingsViewModel
        {
            FirstName = "Kari",
            LastName = "Hansen",
            Email = "kari@gmail.com"
        };

        var result = await controller.UpdateProfile(profile);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);

        Assert.Equal("kari@politiet.no", user.Email);
        Assert.Equal("kari@politiet.no", user.UserName);

        userManager.Verify(
            m => m.UpdateAsync(
                It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Fact]
    public async Task NeedsActor_CanChangeName()
    {
        var user = new ApplicationUser
        {
            Id = "employee-2",
            FirstName = "Kari",
            LastName = "Hansen",
            Email = "kari@politiet.no",
            UserName = "kari@politiet.no"
        };

        var userManager = CreateUserManager(user);

        userManager
            .Setup(m => m.IsInRoleAsync(
                user, AppRoles.NeedsActor))
            .ReturnsAsync(true);

        userManager
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userManager);

        var profile = new ProfileSettingsViewModel
        {
            FirstName = "Katrine",
            LastName = "Hansen",
            Email = "kari@politiet.no"
        };

        var result = await controller.UpdateProfile(profile);

        Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Katrine", user.FirstName);
        Assert.Equal("kari@politiet.no", user.Email);

        userManager.Verify(
            m => m.UpdateAsync(user),
            Times.Once);
    }

    [Fact]
    public async Task ResourceOwner_CanChangeEmail()
    {
        var user = new ApplicationUser
        {
            Id = "owner-1",
            FirstName = "Ola",
            LastName = "Nordmann",
            Email = "ola@example.com",
            UserName = "ola@example.com"
        };

        var userManager = CreateUserManager(user);

        userManager
            .Setup(m => m.IsInRoleAsync(
                user, AppRoles.NeedsActor))
            .ReturnsAsync(false);

        userManager
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userManager);

        var profile = new ProfileSettingsViewModel
        {
            FirstName = "Ola",
            LastName = "Nordmann",
            Email = "ola.ny@example.com"
        };

        var result = await controller.UpdateProfile(profile);

        Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("ola.ny@example.com", user.Email);
        Assert.Equal("ola.ny@example.com", user.UserName);

        userManager.Verify(
            m => m.UpdateAsync(user),
            Times.Once);
    }

    private static Mock<UserManager<ApplicationUser>>
        CreateUserManager(ApplicationUser user)
    {
        var manager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!,
            null!, null!, null!, null!
        );

        manager
            .Setup(m => m.GetUserAsync(
                It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        manager
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string>
            {
                AppRoles.NeedsActor
            });

        return manager;
    }

    private static SettingsController CreateController(
        Mock<UserManager<ApplicationUser>> userManager)
    {
        var signInManager =
            new Mock<SignInManager<ApplicationUser>>(
                userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null!, null!, null!, null!
            );

        signInManager
            .Setup(m => m.RefreshSignInAsync(
                It.IsAny<ApplicationUser>()))
            .Returns(Task.CompletedTask);

        var controller = new SettingsController(
            userManager.Object,
            signInManager.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData =
            new Mock<ITempDataDictionary>().Object;

        return controller;
    }
}
