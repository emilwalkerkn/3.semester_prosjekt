using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class TestResourceAuthorization
{
    [Fact]
    public async Task Edit_Get_ReturnsView_WhenCurrentUserOwnsResource()
    {
        var repository = new Mock<IResourceRepository>();
        repository.Setup(data => data.GetByIdAsync(201)).ReturnsAsync(ResourceOwnedBy(2));

        var controller = new ResourceController(repository.Object)
        {
            ControllerContext = AuthenticatedContext(2)
        };

        var result = await controller.Edit(201);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ResourceViewModel>(view.Model);
        Assert.Equal("Transportkjøretøy", model.Title);
    }

    [Fact]
    public async Task Edit_Get_ReturnsForbid_WhenAnotherUserOwnsResource()
    {
        var repository = new Mock<IResourceRepository>();
        repository.Setup(data => data.GetByIdAsync(201)).ReturnsAsync(ResourceOwnedBy(2));

        var controller = new ResourceController(repository.Object)
        {
            ControllerContext = AuthenticatedContext(3)
        };

        var result = await controller.Edit(201);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Delete_Post_UsesCurrentUserId_WhenDeletingResource()
    {
        var repository = new Mock<IResourceRepository>();
        repository.Setup(data => data.DeleteAsync(201, 2, false)).ReturnsAsync(true);

        var controller = new ResourceController(repository.Object)
        {
            ControllerContext = AuthenticatedContext(2)
        };

        var result = await controller.Delete(201);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Overview", redirect.ActionName);
        repository.Verify(data => data.DeleteAsync(201, 2, false), Times.Once);
    }

    private static Resource ResourceOwnedBy(int userId) => new()
    {
        ResourceId = 201,
        UserId = userId,
        ResourceTypeId = 1,
        Title = "Transportkjøretøy",
        Description = "Varebil med sjåfør.",
        Quantity = 1,
        Unit = "stk",
        Status = "Available"
    };

    private static ControllerContext AuthenticatedContext(int userId) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"))
        }
    };
}
