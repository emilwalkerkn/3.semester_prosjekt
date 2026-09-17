using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestNeedPost
{
    [Fact]
    public void Need_Post_RedirectsToNeedResult()
    {
        // Arrange
        var controller = new HomeController();

        var model = new NeedViewModel
        {
            Type = "Brann",
            Description = "Tre har falt over vei",
            Location = "Kristiansand",
            Latitude = "58.1467",
            Longitude = "7.9956"
        };

        // Act
        var result = controller.Need(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("NeedResult", redirectResult.ActionName);
    }
}