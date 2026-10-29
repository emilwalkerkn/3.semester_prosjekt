using Microsoft.AspNetCore.Mvc;
using Moq;
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

        var model = new ResourceViewModel
        {
            ResourceTypeId = 1,
            Title = "Gravemaskin",
            Description = "Stor gravemaskin",
            Quantity = 1,
            Unit = "stk",
            Status = "Available"
        };

        // Simulerer at skjemadataene er ugyldige.
        controller.ModelState.AddModelError(
            "LocationId",
            "Ugyldig lokasjon"
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
}