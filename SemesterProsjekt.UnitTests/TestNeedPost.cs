using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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
        
        // Simuler en innlogget bruker uten å kontakte databasen.
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testbruker-1")
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var model = new NeedViewModel
        {
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
            repository => repository.CreateAsync(
                It.Is<Need>(need =>
                    need.UserId == "testbruker-1" &&
                    need.Title == model.Title)),
            Times.Once
        );
        
    }
    
}