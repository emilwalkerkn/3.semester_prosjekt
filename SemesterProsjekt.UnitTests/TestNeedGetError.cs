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
        var needRepository = new Mock<INeedRepository>();

        var controller = new NeedController(needRepository.Object);

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);

        // Create() skal ikke returnere en ResourceViewModel
        Assert.IsNotType<ResourceViewModel>(viewResult.Model);
    }
}