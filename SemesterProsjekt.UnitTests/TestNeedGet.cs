using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestNeedGet
{
    [Fact]
    public void Need_Get_ReturnsViewWithNeedViewModel()
    {
        // Arrange
        var needRepository = new Mock<INeedRepository>();

        var controller = new NeedController(needRepository.Object);

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<NeedViewModel>(viewResult.Model);
    }
}