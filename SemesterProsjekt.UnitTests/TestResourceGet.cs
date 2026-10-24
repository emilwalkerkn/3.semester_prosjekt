using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
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

        var controller = new ResourceController(resourceRepository.Object);
        controller.ControllerContext = AuthenticatedContext(1);

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ResourceViewModel>(viewResult.Model);
    }

    private static ControllerContext AuthenticatedContext(int userId) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"))
        }
    };
}
