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
            Type = "Gravemaskin",
            Description = "Stor gravemaskin",
            Location = "Kristiansand"
        };

        // Simulerer at skjemadataene er ugyldige
        controller.ModelState.AddModelError(
            "Location",
            "Ugyldig lokasjon"
        );

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Same(model, viewResult.Model);

        resourceRepository.Verify(
            r => r.CreateAsync(It.IsAny<Resource>()),
            Times.Never
        );
    }
}