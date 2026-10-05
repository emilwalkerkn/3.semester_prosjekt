using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestNeedPost
{
    [Fact]
    public async Task Need_Post_RedirectsToNeedResult()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();
        var needRepository = new Mock<INeedRepository>();

        var controller = new HomeController(
            resourceRepository.Object,
            needRepository.Object
        );

        var model = new NeedViewModel
        {
            Type = "Brann",
            Description = "Tre har falt over vei",
            Location = "Kristiansand",
            Latitude = "58.1467",
            Longitude = "7.9956"
        };

        // Act
        var result = await controller.Need(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("NeedResult", redirectResult.ActionName);
    }
}