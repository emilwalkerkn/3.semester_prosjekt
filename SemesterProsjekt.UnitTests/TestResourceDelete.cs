using Moq;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Controllers;

namespace SemesterProsjekt.UnitTests;

public class TestResourceDelete
{
    [Fact]
    public async Task DeleteConfirmed_OtherUsersResource_ReturnsForbid()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();

        resourceRepository
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(new Resource
            {
                ResourceId = 1,
                UserId = "bruker-a",
                Title = "Brannbil"
            });
        var controller = new ResourceController(resourceRepository.Object);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "bruker-b")
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        }; 
        // Act
        var result = await controller.DeleteConfirmed(1);
        // Assert
        Assert.IsType<ForbidResult>(result);

        resourceRepository.Verify(
            repository => repository.DeleteAsync(It.IsAny<int>()),
            Times.Never
        );
    }
    [Fact]
    public async Task DeleteConfirmed_OwnResource_DeletesAndRedirects()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();

        resourceRepository
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync(new Resource
            {
                ResourceId = 1,
                UserId = "bruker-a",
                Title = "Brannbil"
            });

        var controller = new ResourceController(resourceRepository.Object);
        
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "bruker-a")
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        // Act
        var result = await controller.DeleteConfirmed(1);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Overview", redirect.ActionName);
        
        resourceRepository.Verify(
            repository => repository.DeleteAsync(1),
            Times.Once
        ); 
        
    }
    [Fact]
    public async Task DeleteConfirmed_MissingResource_ReturnsNotFound()
    {
        // Arrange
        var resourceRepository = new Mock<IResourceRepository>();

        resourceRepository
            .Setup(repository => repository.GetByIdAsync(1))
            .ReturnsAsync((Resource?)null);

        var controller = new ResourceController(resourceRepository.Object);
        
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "bruker-a")
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        // Act
        var result = await controller.DeleteConfirmed(1);
        // Assert
        Assert.IsType<NotFoundResult>(result);

        resourceRepository.Verify(
            repository => repository.DeleteAsync(It.IsAny<int>()),
            Times.Never
        );
    }
}