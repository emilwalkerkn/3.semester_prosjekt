using Microsoft.AspNetCore.Mvc;
using Moq;
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
            repository => repository.CreateAsync(It.IsAny<Need>()),
            Times.Once
        );
    }
}