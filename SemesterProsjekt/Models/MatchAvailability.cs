namespace SemesterProsjekt.Models;


/// Godkjente matcher reserverer mengde frem til levering. Quantity på behov og
/// ressurs er resterende mengde; den reduseres først når leveringen fullføres.
/// NULL betyr at registreringen ikke har en oppgitt mengde, ikke at den er tom.

public static class MatchAvailability
{
    public static decimal? AvailableNeed(Need need, IEnumerable<MatchReservation> reservations) =>
        need.Quantity - reservations.Where(x => x.NeedId == need.NeedId)
            .Sum(x => x.AssignedQuantity ?? 0);

    public static decimal? AvailableResource(Resource resource, IEnumerable<MatchReservation> reservations) =>
        resource.Quantity - reservations.Where(x => x.ResourceId == resource.ResourceId)
            .Sum(x => x.AssignedQuantity ?? 0);

    public static bool CanMatchNeed(Need need, IEnumerable<MatchReservation> reservations) =>
        need.Status != NeedStatuses.Resolved &&
        (need.Quantity.HasValue
            ? AvailableNeed(need, reservations) > 0
            : !reservations.Any(x => x.NeedId == need.NeedId));

    public static bool CanMatchResource(Resource resource, IEnumerable<MatchReservation> reservations) =>
        resource.Status == "Available" &&
        (!resource.Quantity.HasValue || AvailableResource(resource, reservations) > 0);

    
    /// Tomt mengdefelt bruker den minste resterende mengden. En eksplisitt
    /// mengde må kunne dekkes helt; den justeres aldri uten brukerens kunnskap.
   
    public static decimal? Allocate(
        Need need, Resource resource, IEnumerable<MatchReservation> reservations,
        decimal? requestedQuantity, bool completing = false)
    {
        var reserved = reservations.ToList();
        if (!CanMatchNeed(need, reserved))
            throw new MatchAvailabilityException("Behovet er løst eller allerede dekket av godkjente matcher.");
        if ((!completing && resource.Status != "Available") ||
            (resource.Quantity.HasValue && AvailableResource(resource, reserved) <= 0))
            throw new MatchAvailabilityException("Ressursen er utilgjengelig, tom eller allerede reservert.");

        if (!string.IsNullOrWhiteSpace(need.Unit) && !string.IsNullOrWhiteSpace(resource.Unit) &&
            !string.Equals(need.Unit.Trim(), resource.Unit.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new MatchAvailabilityException("Behov og ressurs må bruke samme mengdeenhet.");

        var needLeft = AvailableNeed(need, reserved);
        var resourceLeft = AvailableResource(resource, reserved);
        var maximum = needLeft.HasValue && resourceLeft.HasValue
            ? Math.Min(needLeft.Value, resourceLeft.Value)
            : needLeft ?? resourceLeft;
        var quantity = requestedQuantity ?? maximum;
        if (quantity.HasValue && (quantity <= 0 || quantity > 99999999.99m ||
            decimal.Round(quantity.Value, 2) != quantity ||
            (maximum.HasValue && quantity > maximum)))
            throw new MatchAvailabilityException("Mengden må være positiv, ha høyst to desimaler og ikke overstige ledig mengde.");

        return quantity;
    }
}

/// En godkjent match som ennå ikke er levert.
public class MatchReservation
{
    public int NeedId { get; set; }
    public int ResourceId { get; set; }
    public decimal? AssignedQuantity { get; set; }
}

public sealed class MatchAvailabilityException(string message) : InvalidOperationException(message);
