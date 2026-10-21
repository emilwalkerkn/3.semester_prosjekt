using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

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

        var model = new ResourceViewModel
        {
            ProviderId = 1,
            ResourceTypeId = 1,
            LocationId = 1,
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
            repository => repository.CreateAsync(It.IsAny<Resource>()),
            Times.Once
        );
    }
}