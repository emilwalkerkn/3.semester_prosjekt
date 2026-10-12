using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestResourcePost
{
    [Fact]
    public async Task Resource_Post_RedirectsToResult()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();

        var controller = new ResourceController(resourceRepository.Object);

        var model = new ResourceViewModel
        {
            Type = "Brannbil",
            Description = "Brannbil med mannskap",
            Location = "Kristiansand",
            Latitude = "58.1467",
            Longitude = "7.9956"
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Result", redirectResult.ActionName);
    }
}