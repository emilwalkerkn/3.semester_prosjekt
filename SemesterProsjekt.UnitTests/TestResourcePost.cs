using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestResourcePost
{
    [Fact]
    public void Resource_Post_RedirectsToResourceResult()
    {
        // Arrange
        var controller = new HomeController();

        var model = new ResourceViewModel
        {
            Type = "Brannbil",
            Description = "Brannbil med mannskap",
            Location = "Kristiansand",
            Latitude = "58.1467",
            Longitude = "7.9956"
        };

        // Act
        var result = controller.Resource(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ResourceResult", redirectResult.ActionName);
    }
}