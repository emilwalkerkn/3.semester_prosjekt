using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestNeedGet
{
    [Fact]
    public void Need_Get_ReturnsViewWithNeedViewModel()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Need();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<NeedViewModel>(viewResult.Model);
    }
}