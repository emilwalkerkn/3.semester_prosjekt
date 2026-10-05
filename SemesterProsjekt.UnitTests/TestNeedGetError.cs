using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class TestNeedGetError
{
    [Fact]
    public void NeedGet_ShouldNotReturnResourceViewModel()
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

        // Need() skal ikke returnere en ResourceViewModel
        Assert.IsNotType<ResourceViewModel>(viewResult.Model);
    }
}