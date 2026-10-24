using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class UnitTestNeedGet
{
    [Fact]
    public async Task Need_Get_ReturnsViewWithNeedViewModel()
    {
        // Arrange
        var needRepository = new Mock<INeedRepository>();
        needRepository
            .Setup(repository => repository.GetNeedTypesAsync())
            .ReturnsAsync(new List<NeedType>());

        var controller = new NeedController(needRepository.Object);
        controller.ControllerContext = AuthenticatedContext(1);

        // Act
        var result = await controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<NeedViewModel>(viewResult.Model);
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
