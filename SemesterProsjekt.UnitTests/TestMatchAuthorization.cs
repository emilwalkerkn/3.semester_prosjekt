using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.UnitTests;

public class TestMatchAuthorization
{
    [Fact]
    public async Task Create_Post_UsesCurrentNeedOwner_WhenCreatingMatch()
    {
        var matchRepository = new Mock<IMatchRepository>();
        var needRepository = new Mock<INeedRepository>();
        needRepository.Setup(data => data.GetByIdAsync(101)).ReturnsAsync(NeedOwnedBy(1));
        matchRepository.Setup(data => data.CreateAsync(101, 201, 20, 1, false)).ReturnsAsync(1);

        var controller = new MatchController(matchRepository.Object, needRepository.Object)
        {
            ControllerContext = AuthenticatedContext(1, "Behovsaktør")
        };

        var result = await controller.Create(101, new MatchCreateViewModel
        {
            ResourceId = 201,
            AssignedQuantity = 20
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Overview", redirect.ActionName);
        matchRepository.Verify(data => data.CreateAsync(101, 201, 20, 1, false), Times.Once);
    }

    [Fact]
    public async Task Create_Get_ReturnsForbid_WhenAnotherUserOwnsNeed()
    {
        var matchRepository = new Mock<IMatchRepository>();
        var needRepository = new Mock<INeedRepository>();
        needRepository.Setup(data => data.GetByIdAsync(101)).ReturnsAsync(NeedOwnedBy(1));

        var controller = new MatchController(matchRepository.Object, needRepository.Object)
        {
            ControllerContext = AuthenticatedContext(2, "Behovsaktør")
        };

        var result = await controller.Create(101);

        Assert.IsType<ForbidResult>(result);
        matchRepository.Verify(data => data.GetAvailableResourcesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_UsesCurrentResourceOwner_WhenAcceptingMatch()
    {
        var matchRepository = new Mock<IMatchRepository>();
        var needRepository = new Mock<INeedRepository>();
        matchRepository.Setup(data => data.UpdateStatusAsync(1, 2, false, "Accepted")).ReturnsAsync(true);

        var controller = new MatchController(matchRepository.Object, needRepository.Object)
        {
            ControllerContext = AuthenticatedContext(2, "Ressurseier")
        };

        var result = await controller.UpdateStatus(1, "Accepted");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Overview", redirect.ActionName);
        matchRepository.Verify(data => data.UpdateStatusAsync(1, 2, false, "Accepted"), Times.Once);
    }

    private static Need NeedOwnedBy(int userId) => new()
    {
        NeedId = 101,
        UserId = userId,
        Title = "Medisinsk utstyr",
        Quantity = 120,
        Unit = "sett"
    };

    private static ControllerContext AuthenticatedContext(int userId, string role) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role)
                ], "Test"))
        }
    };
}
