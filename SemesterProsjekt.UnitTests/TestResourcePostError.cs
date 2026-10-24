using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class TestResourcePostError
{
    [Fact]
    public async Task ResourcePost_InvalidModel_ReturnsViewWithoutSaving()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();

        var controller = new ResourceController(resourceRepository.Object);
        controller.ControllerContext = AuthenticatedContext(1);

        var model = new ResourceViewModel
        {
            UserId = 1,
            ResourceTypeId = 1,
            Title = "Gravemaskin",
            Description = "Stor gravemaskin",
            Quantity = 1,
            Unit = "stk",
            Status = "Available"
        };

        // Simulerer at skjemadataene er ugyldige.
        controller.ModelState.AddModelError(
            "UserId",
            "Ugyldig bruker"
        );

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Same(model, viewResult.Model);

        resourceRepository.Verify(
            repository => repository.CreateAsync(It.IsAny<Resource>()),
            Times.Never
        );
    }

    private static ControllerContext AuthenticatedContext(int userId) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                "Test"))
        }
    };
}
