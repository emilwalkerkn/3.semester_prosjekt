using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestResourceGet
{
    [Fact]
    public void Resource_Get_ReturnsViewWithResourceViewModel()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Resource();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ResourceViewModel>(viewResult.Model);
    }
}