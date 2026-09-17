using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class TestNeedGetError
{
    [Fact]
    public void NeedGet_ShouldFail()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Need();
        
        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ResourceViewModel>(viewResult.Model);


    }
}