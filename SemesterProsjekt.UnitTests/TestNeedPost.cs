using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestNeedPost
{
    [Fact]
    public async Task Need_Post_RedirectsToResult()
    {
        // Arrange
        var needRepository = new Mock<INeedRepository>();

        needRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<Need>()))
            .ReturnsAsync(1);

        var controller = new NeedController(needRepository.Object);
        controller.ControllerContext = AuthenticatedContext(1);

        var model = new NeedViewModel
        {
            UserId = 1,
            NeedTypeId = 1,
            Title = "Tre over vei",
            Description = "Et tre har falt over veien i Kristiansand.",
            Quantity = 1,
            Unit = "stk",
            Status = "New",
            Priority = "High"
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Result", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues?["id"]);

        needRepository.Verify(
            repository => repository.CreateAsync(It.IsAny<Need>()),
            Times.Once
        );
    }

    private static ControllerContext AuthenticatedContext(int userId) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                "Test"))
        }
    };
}
