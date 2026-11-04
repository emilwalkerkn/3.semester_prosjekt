using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class TestNeedGetError
{
    [Fact]
    public async Task NeedGet_ShouldNotReturnResourceViewModel()    {
        // Arrange
        var needRepository = new Mock<INeedRepository>();

        var controller = new NeedController(needRepository.Object);

        // Act
        var result = await controller.Create();
        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);

        // Create() skal ikke returnere en ResourceViewModel
        Assert.IsNotType<ResourceViewModel>(viewResult.Model);
    }
}