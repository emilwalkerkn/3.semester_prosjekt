using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestResourcePost
{
    [Fact]
    public async Task Resource_Post_RedirectsToResourceResult()
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
            Type = "Brannbil",
            Description = "Brannbil med mannskap",
            Location = "Kristiansand",
            Latitude = "58.1467",
            Longitude = "7.9956"
        };

        // Act
        var result = await controller.Resource(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ResourceResult", redirectResult.ActionName);
    }
}