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
        var resourceRepository = new Mock<IResourceRepository>();
        var needRepository = new Mock<INeedRepository>();

        var controller = new HomeController(
            resourceRepository.Object,
            needRepository.Object
        );

        // Act
        var result = controller.Need();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<NeedViewModel>(viewResult.Model);
    }
}