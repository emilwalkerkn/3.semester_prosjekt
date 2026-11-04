using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class TestMatchAvailability
{
    [Fact]
    public void MatchBrukerMinsteTilgjengeligeMengde()
    {
        // Oppsett: Behovet er større enn ressursen.
        var behov = new Need { Quantity = 10 };
        var ressurs = new Resource { Quantity = 5 };

        // Handling: Ingen bestemt mengde er valgt.
        var mengde = MatchAvailability.Allocate(
            behov, ressurs, [], null);

        // Kontroll: Matchen kan levere 5.
        Assert.Equal(5m, mengde);
    }

    [Fact]
    public void BrukerenKanVelgeEnMindreMengde()
    {
        var behov = new Need { Quantity = 10 };
        var ressurs = new Resource { Quantity = 5 };

        var mengde = MatchAvailability.Allocate(
            behov, ressurs, [], 3);

        Assert.Equal(3m, mengde);
    }

    [Fact]
    public void ReservasjonRedusererLedigMengde()
    {
        var behov = new Need { NeedId = 1, Quantity = 10 };
        var ressurs = new Resource { ResourceId = 2, Quantity = 20 };

        var reservasjoner = new[]
        {
            new MatchReservation
            {
                NeedId = 1,
                ResourceId = 2,
                AssignedQuantity = 6
            }
        };

        Assert.Equal(4m,
            MatchAvailability.AvailableNeed(behov, reservasjoner));

        Assert.Equal(14m,
            MatchAvailability.AvailableResource(ressurs, reservasjoner));

        // Reservasjon skal ikke registreres som levert.
        Assert.Equal(10m, behov.Quantity);
        Assert.Equal(20m, ressurs.Quantity);
    }

    [Fact]
    public void LostBehovKanIkkeMatches()
    {
        var behov = new Need
        {
            Status = NeedStatuses.Resolved,
            Quantity = 5
        };

        Assert.False(MatchAvailability.CanMatchNeed(behov, []));
    }

    // Samme test kjøres med en utilgjengelig og en tom ressurs.
    [Theory]
    [InlineData("Unavailable", 5)]
    [InlineData("Available", 0)]
    public void UtilgjengeligEllerTomRessursKanIkkeMatches(
        string status, int mengde)
    {
        var ressurs = new Resource
        {
            Status = status,
            Quantity = mengde
        };

        Assert.False(MatchAvailability.CanMatchResource(ressurs, []));
    }

    [Fact]
    public void FulltReservertMengdeKanIkkeMatchesIgjen()
    {
        var behov = new Need { NeedId = 1, Quantity = 5 };
        var ressurs = new Resource { ResourceId = 2, Quantity = 5 };

        var reservasjoner = new[]
        {
            new MatchReservation
            {
                NeedId = 1,
                ResourceId = 2,
                AssignedQuantity = 5
            }
        };

        Assert.False(
            MatchAvailability.CanMatchNeed(behov, reservasjoner));

        Assert.False(
            MatchAvailability.CanMatchResource(ressurs, reservasjoner));
    }

    [Fact]
    public void ForskjelligeEnheterKanIkkeMatches()
    {
        var behov = new Need { Quantity = 5, Unit = "kg" };
        var ressurs = new Resource { Quantity = 5, Unit = "liter" };

        Assert.Throws<MatchAvailabilityException>(() =>
            MatchAvailability.Allocate(behov, ressurs, [], null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public void UgyldigMengdeBlirAvvist(int mengde)
    {
        var behov = new Need { Quantity = 5 };
        var ressurs = new Resource { Quantity = 5 };

        Assert.Throws<MatchAvailabilityException>(() =>
            MatchAvailability.Allocate(behov, ressurs, [], mengde));
    }
}