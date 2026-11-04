using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SemesterProsjekt.UnitTests;

public class UnitTestResourcePost
{
    [Fact]
    public async Task Resource_Post_RedirectsToResult()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();

        resourceRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<Resource>()))
            .ReturnsAsync(1);

        var controller = new ResourceController(resourceRepository.Object);

        // Simuler en innlogget bruker uten å kontakte databasen.
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testbruker-1")
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
        
        var model = new ResourceViewModel
        {
            ResourceTypeId = 1,
            Title = "Brannbil",
            Description = "Brannbil med mannskap.",
            Quantity = 1,
            Unit = "stk",
            Status = "Available"
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Result", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues?["id"]);

        resourceRepository.Verify(
            repository => repository.CreateAsync(
                It.Is<Resource>(resource =>
                    resource.UserId == "testbruker-1" &&
                    resource.Title == model.Title)),
            Times.Once
        );
    }
}