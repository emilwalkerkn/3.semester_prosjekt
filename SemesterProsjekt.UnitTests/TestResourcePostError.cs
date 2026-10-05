using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class TestResourcePostError
{
    [Fact]
    public async Task ResourcePost_ShouldNotRedirectToNeedResult()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();
        var needRepository = new Mock<INeedRepository>();

        var controller = new HomeController(
            resourceRepository.Object,
            needRepository.Object
        );

        var model = new ResourceViewModel
        {
            Type = "Gravemaskin",
            Description = "Stor gravemaskin",
            Location = "Kristiansand"
        };

        // Act
        var result = await controller.Resource(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        // Resource() skal ikke sende brukeren til NeedResult
        Assert.NotEqual("NeedResult", redirectResult.ActionName);
    }
}