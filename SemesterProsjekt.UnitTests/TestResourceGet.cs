using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestResourceGet
{
    [Fact]
    public void Resource_Get_ReturnsViewWithResourceViewModel()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();
        var needRepository = new Mock<INeedRepository>();

        var controller = new HomeController(
            resourceRepository.Object,
            needRepository.Object
        );

        // Act
        var result = controller.Resource();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ResourceViewModel>(viewResult.Model);
    }
}