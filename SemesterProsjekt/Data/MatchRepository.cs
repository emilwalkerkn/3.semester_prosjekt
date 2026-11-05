using Dapper;
using System.Data;
using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Data;


/// Håndterer databasetilgang for matcher med Dapper.
/// Brukeroversikter filtreres etter eierskapet til behovet eller ressursen.

public class MatchRepository : IMatchRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MatchRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }


    /// Henter åpne matchforslag til administratorens oversikt.

    public Task<IEnumerable<MatchListItemViewModel>> GetActiveAsync() =>
        GetByHistoryAsync(isHistory: false);


    /// Henter fullførte, avslåtte og avbrutte matcher til administratorens historikk.

    public Task<IEnumerable<MatchListItemViewModel>> GetHistoryAsync() =>
        GetByHistoryAsync(isHistory: true);


    /// Henter aktive matcher som gjelder brukerens egne registreringer.

    public Task<IEnumerable<MatchListItemViewModel>> GetActiveForUserAsync(
        string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return GetByHistoryAsync(isHistory: false, userId: userId);
    }


    /// Henter behandlede matcher som gjelder brukerens egne registreringer.

    public Task<IEnumerable<MatchListItemViewModel>> GetHistoryForUserAsync(
        string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return GetByHistoryAsync(isHistory: true, userId: userId);
    }


    /// Felles oppslag for aktive matcher og historikk.
    /// Når userId er oppgitt, returneres bare matcher der brukeren
    /// eier behovet eller ressursen. Uten userId returneres alle.

    private async Task<IEnumerable<MatchListItemViewModel>> GetByHistoryAsync(
        bool isHistory,
        string? userId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT m.MatchId, m.NeedId, COALESCE(m.RequestNeedTitle, n.Title) AS NeedTitle,
                   m.ResourceId, COALESCE(m.RequestResourceTitle, r.Title) AS ResourceTitle, m.Status,
                   m.MatchedAt, m.ResolvedAt, m.Notes, m.ProposedByUserId,
                   m.DeliveryAt, m.DeliveredAt, m.DeliveryLocation, m.DeliveryContact,
                   COALESCE(NULLIF(TRIM(CONCAT(nu.FirstName, ' ', nu.LastName)), ''), nu.Email) AS RequesterName,
                   COALESCE(NULLIF(TRIM(CONCAT(ru.FirstName, ' ', ru.LastName)), ''), ru.Email) AS SupplierName,
                   m.AssignedQuantity, COALESCE(m.AssignedUnit, n.Unit, r.Unit) AS AssignedUnit,
                   n.UserId AS NeedUserId, r.UserId AS ResourceUserId
            FROM `Match` m
            INNER JOIN Needs n ON n.NeedId = m.NeedId
            INNER JOIN Resources r ON r.ResourceId = m.ResourceId
            LEFT JOIN AspNetUsers nu ON nu.Id = n.UserId
            LEFT JOIN AspNetUsers ru ON ru.Id = r.UserId
            WHERE (
                (@IsHistory = 1 AND m.Status IN (@CompletedStatus, @DeclinedStatus, @CancelledStatus))
                OR
                (@IsHistory = 0 AND m.Status NOT IN (@CompletedStatus, @DeclinedStatus, @CancelledStatus))
            )
            AND (
                @UserId IS NULL
                OR n.UserId = @UserId
                OR r.UserId = @UserId
            )
            ORDER BY m.MatchedAt DESC;
            """;

        // Filtreringen skjer i databasen før resultatet sendes til viewet.
        // Bruker-ID-en skal komme fra innloggingen i controlleren.
        return await connection.QueryAsync<MatchListItemViewModel>(sql, new
        {
            IsHistory = isHistory,
            UserId = userId,
            CompletedStatus = MatchStatuses.Completed,
            DeclinedStatus = MatchStatuses.Declined,
            CancelledStatus = MatchStatuses.Cancelled
        });
    }


    /// Henter én match, eller null dersom den ikke finnes.

    public async Task<Match?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Match>(
            "SELECT * FROM `Match` WHERE MatchId = @MatchId;",
            new { MatchId = id });
    }

    public async Task<IEnumerable<MatchReservation>> GetReservationsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await ReadReservationsAsync(connection);
    }

    private static Task<IEnumerable<MatchReservation>> ReadReservationsAsync(
        IDbConnection connection, IDbTransaction? transaction = null, int exceptMatchId = 0) =>
        connection.QueryAsync<MatchReservation>(
            """
            SELECT NeedId, ResourceId, AssignedQuantity FROM `Match`
            WHERE Status IN (@Status, @Delivered) AND MatchId <> @ExceptMatchId;
            """,
            new { Status = MatchStatuses.Approved, Delivered = MatchStatuses.Delivered, ExceptMatchId = exceptMatchId }, transaction);


    /// Kontrollerer om kombinasjonen av behov og ressurs allerede finnes.

    public async Task<bool> ExistsAsync(int needId, int resourceId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(
            """
            SELECT COUNT(*) > 0
            FROM `Match`
            WHERE NeedId = @NeedId
              AND ResourceId = @ResourceId
              AND Status NOT IN (@Declined, @Cancelled, @Completed);
            """,
            new
            {
                NeedId = needId,
                ResourceId = resourceId, Declined = MatchStatuses.Declined, Cancelled = MatchStatuses.Cancelled, Completed = MatchStatuses.Completed
            });
    }


    /// Lagrer en ny match og returnerer den genererte ID-en.

    public async Task<int> CreateAsync(Match match)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        // Samme låserekkefølge som ved godkjenning/fullføring hindrer at
        // en registrering blir brukt mens en annen forespørsel løser den.
        var need = await connection.QuerySingleOrDefaultAsync<Need>(
            "SELECT * FROM Needs WHERE NeedId = @NeedId FOR UPDATE;", match, transaction);
        var resource = await connection.QuerySingleOrDefaultAsync<Resource>(
            "SELECT * FROM Resources WHERE ResourceId = @ResourceId FOR UPDATE;", match, transaction);
        if (need is null || resource is null)
            throw new MatchAvailabilityException("Behovet eller ressursen finnes ikke lenger.");
        var reservations = await ReadReservationsAsync(connection, transaction);
        match.AssignedQuantity = MatchAvailability.Allocate(need, resource, reservations, match.AssignedQuantity);
        match.AssignedUnit = need.Unit?.Trim() ?? resource.Unit?.Trim();
        match.RequestNeedTitle = need.Title;
        match.RequestResourceTitle = resource.Title;

        if (await connection.ExecuteScalarAsync<bool>(
            "SELECT COUNT(*) > 0 FROM `Match` WHERE NeedId = @NeedId AND ResourceId = @ResourceId AND Status NOT IN (@Declined, @Cancelled, @Completed);",
            new { match.NeedId, match.ResourceId, Declined = MatchStatuses.Declined, Cancelled = MatchStatuses.Cancelled, Completed = MatchStatuses.Completed }, transaction))
            throw new MatchAvailabilityException("Denne kombinasjonen er allerede matchet.");

        const string sql = """
            INSERT INTO `Match` (NeedId, ResourceId, ProposedByUserId, Status, MatchedAt, Notes, AssignedQuantity, AssignedUnit, DeliveryAt, DeliveryLocation, DeliveryContact, RequestNeedTitle, RequestResourceTitle)
            VALUES (@NeedId, @ResourceId, @ProposedByUserId, @Status, @MatchedAt, @Notes, @AssignedQuantity, @AssignedUnit, @DeliveryAt, @DeliveryLocation, @DeliveryContact, @RequestNeedTitle, @RequestResourceTitle);
            SELECT LAST_INSERT_ID();
            """;

        var id = await connection.ExecuteScalarAsync<int>(sql, match, transaction);
        transaction.Commit();
        return id;
    }


    /// Oppdaterer status og avslutningstidspunkt.
    /// Controlleren kontrollerer tilgang og statusovergang før lagring.

    public async Task UpdateStatusAsync(
        int id,
        string status,
        DateTime? resolvedAt)
    {
        var match = await GetByIdAsync(id);
        if (match is null || match.Status == status) return;
        if (!await TryUpdateStatusAsync(id, match.Status, status, resolvedAt))
            throw new MatchAvailabilityException("Matchen er allerede endret. Last oversikten på nytt.");
    }

    
    /// Endrer match, behov og beholdning i én transaksjon: enten lagres alt,
    /// eller ingenting. Radlåsene hindrer dobbel levering og overbooking.
   
    public async Task<bool> TryUpdateStatusAsync(
        int id,
        string expectedStatus,
        string status,
        DateTime? resolvedAt)
    {
        var original = await GetByIdAsync(id);
        if (original is null) return false;
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        var need = await connection.QuerySingleOrDefaultAsync<Need>(
            "SELECT * FROM Needs WHERE NeedId = @NeedId FOR UPDATE;", original, transaction);
        var resource = await connection.QuerySingleOrDefaultAsync<Resource>(
            "SELECT * FROM Resources WHERE ResourceId = @ResourceId FOR UPDATE;", original, transaction);
        var match = await connection.QuerySingleOrDefaultAsync<Match>(
            "SELECT * FROM `Match` WHERE MatchId = @Id FOR UPDATE;", new { Id = id }, transaction);
        if (match is null || need is null || resource is null || match.Status != expectedStatus)
            return false;

        var approving = expectedStatus == MatchStatuses.Proposed && status == MatchStatuses.Approved;
        var completing = expectedStatus == MatchStatuses.Delivered && status == MatchStatuses.Completed;
        var delivering = expectedStatus == MatchStatuses.Approved && status == MatchStatuses.Delivered;
        if (!MatchFlow.IsTransition(expectedStatus, status)) return false;

        if (approving || completing)
        {
            if (approving && match.DeliveryAt.HasValue &&
                ((resource.AvailableFrom.HasValue && match.DeliveryAt < resource.AvailableFrom) ||
                 (resource.AvailableTo.HasValue && match.DeliveryAt > resource.AvailableTo)))
                throw new MatchAvailabilityException("Ressursens tilgjengelighet er endret. Avklar et nytt tidspunkt før godkjenning.");
            var reservations = await ReadReservationsAsync(connection, transaction, id);
            var quantity = MatchAvailability.Allocate(
                need, resource, reservations, match.AssignedQuantity, completing);
            if (!string.IsNullOrWhiteSpace(match.AssignedUnit) &&
                ((!string.IsNullOrWhiteSpace(need.Unit) &&
                  !string.Equals(match.AssignedUnit, need.Unit.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrWhiteSpace(resource.Unit) &&
                  !string.Equals(match.AssignedUnit, resource.Unit.Trim(), StringComparison.OrdinalIgnoreCase))))
                throw new MatchAvailabilityException("Mengdeenheten er endret siden forespørselen. Kontroller registreringene før levering.");

            match.AssignedQuantity = quantity;
            match.AssignedUnit ??= need.Unit?.Trim() ?? resource.Unit?.Trim();
            if (approving)
            {
                await connection.ExecuteAsync("UPDATE Needs SET Status = @Status WHERE NeedId = @NeedId;",
                    new { Status = NeedStatuses.InProgress, need.NeedId }, transaction);
            }
            else
            {
                // Bare målbare registreringer får mengden redusert. Et behov
                // uten mengde regnes som løst når den avtalte leveringen er fullført.
                var needLeft = need.Quantity - quantity;
                var resourceLeft = resource.Quantity - quantity;
                var resolved = !need.Quantity.HasValue || needLeft <= 0;
                var depleted = resource.Quantity.HasValue && resourceLeft <= 0;
                await connection.ExecuteAsync(
                    "UPDATE Needs SET Quantity = @Quantity, Status = @Status WHERE NeedId = @NeedId;",
                    new { need.NeedId, Quantity = needLeft,
                        Status = resolved ? NeedStatuses.Resolved : NeedStatuses.InProgress }, transaction);
                await connection.ExecuteAsync(
                    "UPDATE Resources SET Quantity = @Quantity, Status = @Status WHERE ResourceId = @ResourceId;",
                    new { resource.ResourceId, Quantity = resourceLeft,
                        Status = depleted ? "Unavailable" : resource.Status }, transaction);

                // Andre åpne forslag blir uaktuelle når behovet er løst eller
                // ressursen er oppbrukt. Godkjente avtaler beholdes.
                await connection.ExecuteAsync(
                    """
                    UPDATE `Match` SET Status = @Declined, ResolvedAt = @ResolvedAt,
                        Notes = LEFT(CONCAT(COALESCE(Notes, ''),
                            ' [Automatisk avsluttet: behovet er løst eller ressursen er oppbrukt.]'), 500)
                    WHERE Status = @Proposed AND MatchId <> @Id
                      AND ((NeedId = @NeedId AND @Resolved) OR (ResourceId = @ResourceId AND @Depleted));
                    """,
                    new { Id = id, need.NeedId, resource.ResourceId, Resolved = resolved, Depleted = depleted,
                        Declined = MatchStatuses.Declined, Proposed = MatchStatuses.Proposed, ResolvedAt = resolvedAt }, transaction);
            }
        }

        await connection.ExecuteAsync(
            """
            UPDATE `Match`
            SET Status = @Status, ResolvedAt = @ResolvedAt,
                DeliveredAt = CASE WHEN @Delivering THEN UTC_TIMESTAMP() ELSE DeliveredAt END,
                AssignedQuantity = @AssignedQuantity, AssignedUnit = @AssignedUnit
            WHERE MatchId = @MatchId;
            """,
            new
            {
                MatchId = id,
                Status = status,
                ResolvedAt = resolvedAt,
                match.AssignedQuantity,
                match.AssignedUnit,
                Delivering = delivering
            }, transaction);

        transaction.Commit();
        return true;
    }
}
