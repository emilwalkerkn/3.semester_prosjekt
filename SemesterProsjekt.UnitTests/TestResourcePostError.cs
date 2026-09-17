using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class TestResourcePostError
{
    [Fact]
    public void ResourcePost_ShouldFail()
    {
        // Arange
        var controller = new HomeController();
        var model = new ResourceViewModel
        {
            Type = "Gravemaskin",
            Description = "Stor gravemaskin",
            Location = "Kristiansand"
        };
        
        // Act
        var result = controller.Resource(model);
        
        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("NeedResult", redirectResult.ActionName);
    }
}

