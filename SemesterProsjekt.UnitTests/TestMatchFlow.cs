using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SemesterProsjekt.Controllers;
using SemesterProsjekt.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;
using Match = SemesterProsjekt.Models.Match;

namespace SemesterProsjekt.UnitTests;

public class TestMatchFlow
{
    // Mock erstatter databaselaget mens testen kjører.
    private readonly Mock<IMatchRepository> _matcher = new();
    private readonly Mock<INeedRepository> _behov = new();
    private readonly Mock<IResourceRepository> _ressurser = new();
    private readonly Mock<INotificationRepository> _varsler = new();

    private readonly Need _testbehov = new()
    {
        NeedId = 1,
        UserId = "behovseier"
    };

    private readonly Resource _testressurs = new()
    {
        ResourceId = 2,
        UserId = "ressurseier"
    };

    private readonly Match _testmatch = new()
    {
        MatchId = 10,
        NeedId = 1,
        ResourceId = 2,
        ProposedByUserId = "behovseier",
        Status = MatchStatuses.Proposed
    };

    // Dette oppsettet brukes av alle testene i filen.
    public TestMatchFlow()
    {
        _behov.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(_testbehov);

        _ressurser.Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync(_testressurs);

        _matcher.Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(_testmatch);

        _behov.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .ReturnsAsync(new[] { _testbehov });

        _ressurser.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new[] { _testressurs });

        _matcher.Setup(x => x.GetReservationsAsync())
            .ReturnsAsync(Array.Empty<MatchReservation>());

        _matcher.Setup(x => x.TryUpdateStatusAsync(
                10,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>()))
            .ReturnsAsync(true);
    }

    [Theory]
    [InlineData(MatchStatuses.Approved)]
    [InlineData(MatchStatuses.Declined)]
    public async Task MotpartenKanGodkjenneEllerAvsla(string status)
    {
        var controller = LagController(
            "ressurseier", AppRoles.ResourceOwner);

        var resultat = await controller.UpdateStatus(10, status);

        var videresending =
            Assert.IsType<RedirectToActionResult>(resultat);

        Assert.Equal(status == MatchStatuses.Approved ? "Overview" : "History", videresending.ActionName);

        _matcher.Verify(x => x.TryUpdateStatusAsync(
            10, MatchStatuses.Proposed, status,
            It.IsAny<DateTime?>()), Times.Once);
    }

    [Fact]
    public async Task ForslagsstillerKanIkkeGodkjenneEgetForslag()
    {
        var controller = LagController(
            "behovseier", AppRoles.NeedsActor);

        var resultat = await controller.UpdateStatus(
            10, MatchStatuses.Approved);

        Assert.IsType<ForbidResult>(resultat);
        KontrollerAtStatusIkkeBleLagret();
    }

    [Fact]
    public async Task UvedkommendeKanIkkeGodkjenne()
    {
        var controller = LagController(
            "annen-bruker", AppRoles.ResourceOwner);

        var resultat = await controller.UpdateStatus(
            10, MatchStatuses.Approved);

        Assert.IsType<ForbidResult>(resultat);
        KontrollerAtStatusIkkeBleLagret();
    }

    [Fact]
    public async Task BehovseierKanSvarePaRessurseiersForslag()
    {
        _testmatch.ProposedByUserId = "ressurseier";

        var controller = LagController(
            "behovseier", AppRoles.NeedsActor);

        var resultat = await controller.UpdateStatus(
            10, MatchStatuses.Approved);

        Assert.IsType<RedirectToActionResult>(resultat);
    }

    [Fact]
    public async Task BehovseierKanBekrefteMottakEtterAtRessurseierHarLevert()
    {
        _testmatch.Status = MatchStatuses.Delivered;
        var controller = LagController("behovseier", AppRoles.NeedsActor);

        var resultat = await controller.UpdateStatus(
            10, MatchStatuses.Completed);

        Assert.IsType<RedirectToActionResult>(resultat);

        _matcher.Verify(x => x.TryUpdateStatusAsync(
            10, MatchStatuses.Delivered, MatchStatuses.Completed,
            It.IsAny<DateTime?>()), Times.Once);
    }

    [Fact]
    public async Task ForslagMaGodkjennesForDetKanFullfores()
    {
        var controller = LagController(
            "behovseier", AppRoles.NeedsActor);

        var resultat = await controller.UpdateStatus(
            10, MatchStatuses.Completed);

        Assert.IsType<BadRequestObjectResult>(resultat);
        KontrollerAtStatusIkkeBleLagret();
    }

    [Fact]
    public async Task GjentattGodkjenningLagresIkkePaNytt()
    {
        _testmatch.Status = MatchStatuses.Approved;

        var controller = LagController(
            "ressurseier", AppRoles.ResourceOwner);

        await controller.UpdateStatus(10, MatchStatuses.Approved);

        KontrollerAtStatusIkkeBleLagret();
    }

    [Fact]
    public async Task EldreForslagsskjemaSenderTilKontrollForInnsending()
    {
        var controller = LagController(
            "behovseier", AppRoles.NeedsActor);

        var skjema = new CreateMatchViewModel
        {
            NeedId = 1,
            ResourceId = 2
        };

        var resultat = await controller.Create(skjema);

        Assert.Equal("DeliveryRequest", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        _matcher.Verify(x => x.CreateAsync(It.IsAny<Match>()), Times.Never);
    }

    [Fact]
    public async Task LostBehovKanIkkeBrukesINyttForslag()
    {
        _testbehov.Status = NeedStatuses.Resolved;

        var controller = LagController(
            "behovseier", AppRoles.NeedsActor);

        var resultat = await controller.Create(
            new CreateMatchViewModel { NeedId = 1, ResourceId = 2 });

        Assert.IsType<ViewResult>(resultat);
        Assert.False(controller.ModelState.IsValid);

        _matcher.Verify(x => x.CreateAsync(
            It.IsAny<Match>()), Times.Never);
    }

    [Fact]
    public async Task UtilgjengeligRessursKanIkkeBrukesINyttForslag()
    {
        _testressurs.Status = "Unavailable";

        var controller = LagController(
            "behovseier", AppRoles.NeedsActor);

        var resultat = await controller.Create(
            new CreateMatchViewModel { NeedId = 1, ResourceId = 2 });

        Assert.IsType<ViewResult>(resultat);
        Assert.False(controller.ModelState.IsValid);

        _matcher.Verify(x => x.CreateAsync(
            It.IsAny<Match>()), Times.Never);
    }

    // Lager en controller med en simulert innlogget bruker.
    private MatchController LagController(string brukerId, string rolle)
    {
        var controller = new MatchController(
            _matcher.Object, _behov.Object, _ressurser.Object, _varsler.Object);

        var identitet = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, brukerId),
                new Claim(ClaimTypes.Role, rolle)
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identitet)
            }
        };

        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    // Kontrollerer at et avvist eller gjentatt svar ikke endrer status.
    private void KontrollerAtStatusIkkeBleLagret()
    {
        _matcher.Verify(x => x.TryUpdateStatusAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>()), Times.Never);
    }
}
