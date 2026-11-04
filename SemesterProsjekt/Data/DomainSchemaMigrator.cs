using Dapper;
using System.Data;

namespace SemesterProsjekt.Data;


/// Tilpasser eldre fagtabeller til datatypene og feltene applikasjonen bruker.
/// Endrer UserId til tekstformat og legger til manglende felt i Match-tabellen.
/// Bruker direkte SQL gjennom Dapper, separat fra Identity-migreringene i EF Core.

public static class DomainSchemaMigrator
{
    // Bare disse tabellene undersøkes for eldre UserId-kolonner.
    private static readonly string[] TablesWithIdentityUserId = ["Needs", "Resources"];

   
    /// Undersøker eksisterende fagtabeller og utfører nødvendige tilpasninger.
    /// Forutsetter at tabellene allerede finnes.
    /// Konverterer datatypen til UserId, men kobler ikke gamle bruker-ID-er
    /// til nye Identity-kontoer.
   
    public static async Task MigrateAsync(IServiceProvider services)
    {
        // Oppretter et eget tjenesteområde for oppstartsjobben.
        using var scope = services.CreateScope();

        var connectionFactory = scope.ServiceProvider
            .GetRequiredService<IDbConnectionFactory>();

        using var connection = connectionFactory.CreateConnection();

        foreach (var tableName in TablesWithIdentityUserId)
        {
            // INFORMATION_SCHEMA inneholder metadata om databasens struktur.
            // DATABASE() begrenser oppslaget til databasen i den aktive tilkoblingen.
            const string columnTypeSql = """
                SELECT DATA_TYPE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = @TableName
                  AND COLUMN_NAME = 'UserId';
                """;

            var currentType = await connection.QuerySingleOrDefaultAsync<string>(
                columnTypeSql,
                new { TableName = tableName });

            if (currentType is null)
            {
                throw new InvalidOperationException(
                    $"Fant ikke UserId-kolonnen i tabellen {tableName}.");
            }

            // Kolonner som allerede bruker varchar, hoppes over.
            if (string.Equals(
                currentType,
                "varchar",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Finner fremmednøkler på UserId som må fjernes før typen endres.
            const string foreignKeySql = """
                SELECT kcu.CONSTRAINT_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    ON tc.CONSTRAINT_SCHEMA = kcu.CONSTRAINT_SCHEMA
                   AND tc.TABLE_NAME = kcu.TABLE_NAME
                   AND tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
                WHERE kcu.TABLE_SCHEMA = DATABASE()
                  AND kcu.TABLE_NAME = @TableName
                  AND kcu.COLUMN_NAME = 'UserId'
                  AND tc.CONSTRAINT_TYPE = 'FOREIGN KEY';
                """;

            var foreignKeys = await connection.QueryAsync<string>(
                foreignKeySql,
                new { TableName = tableName });

            foreach (var foreignKeyName in foreignKeys)
            {
                // Tabellenavnet kommer fra den faste listen.
                // Fremmednøkkelnavnet hentes fra databasens metadata.
                // Disse navnene kommer ikke fra et brukerskjema.
                await connection.ExecuteAsync(
                    $"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{foreignKeyName}`;");
            }

            // Endrer kolonnen til tekstformat for Identity-baserte bruker-ID-er.
            // Fremmednøklene som ble fjernet, opprettes ikke på nytt her.
            await connection.ExecuteAsync(
                $"ALTER TABLE `{tableName}` MODIFY COLUMN UserId VARCHAR(255) NOT NULL;");
        }

        await MigrateMatchTableAsync(connection);
    }

   
    /// Tilpasser eldre Match-tabeller til feltene MatchedAt, ResolvedAt og Notes.
    /// Beholder det eldre feltet AssignedQuantity dersom det finnes.
   
    private static async Task MigrateMatchTableAsync(IDbConnection connection)
    {
        // Legger til forslagsstiller uten å gjette hvem som opprettet eldre matcher.
        if (!await ColumnExistsAsync(connection, "Match", "ProposedByUserId"))
        {
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` ADD COLUMN ProposedByUserId VARCHAR(255) NULL;");
        }

        if (!await ColumnExistsAsync(connection, "Match", "MatchedAt"))
        {
            // Oppretter først feltet som valgfritt, slik at eldre rader kan fylles ut.
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` ADD COLUMN MatchedAt DATETIME NULL;");

            // Forutsetter at den eldre tabellen har CreatedAt.
            // Tidspunktene kopieres før det nye feltet gjøres obligatorisk.
            await connection.ExecuteAsync(
                "UPDATE `Match` SET MatchedAt = CreatedAt WHERE MatchedAt IS NULL;");

            await connection.ExecuteAsync(
                "ALTER TABLE `Match` MODIFY COLUMN MatchedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");
        }

        // Avslutningstidspunktet kan være tomt for aktive matcher.
        if (!await ColumnExistsAsync(connection, "Match", "ResolvedAt"))
        {
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` ADD COLUMN ResolvedAt DATETIME NULL;");
        }

        if (!await ColumnExistsAsync(connection, "Match", "Notes"))
        {
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` ADD COLUMN Notes VARCHAR(500) NULL;");
        }

        if (!await ColumnExistsAsync(connection, "Match", "AssignedQuantity"))
        {
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` ADD COLUMN AssignedQuantity DECIMAL(10,2) NULL;");
        }
        else
        {
            // Eldre standardverdi 0 betyr ukjent mengde. Ingen beholdning trekkes
            // retroaktivt; eldre godkjente matcher beregnes ved fullføring.
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` MODIFY COLUMN AssignedQuantity DECIMAL(10,2) NULL DEFAULT NULL;");
            await connection.ExecuteAsync(
                "UPDATE `Match` SET AssignedQuantity = NULL WHERE AssignedQuantity = 0;");
        }

        if (!await ColumnExistsAsync(connection, "Match", "AssignedUnit"))
        {
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` ADD COLUMN AssignedUnit VARCHAR(50) NULL;");
        }
    }

   
    /// Undersøker om en kolonne finnes i en tabell i den aktive databasen.
    /// Brukes før opprettelse eller tilpasning av felter.
   
    private static async Task<bool> ColumnExistsAsync(
        IDbConnection connection,
        string tableName,
        string columnName)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = @TableName
              AND COLUMN_NAME = @ColumnName;
            """;

        // Tabell- og kolonnenavn brukes som søkeverdier i metadataoppslaget.
        // Dapper konverterer antallet treff til en boolsk verdi.
        return await connection.ExecuteScalarAsync<bool>(sql, new
        {
            TableName = tableName,
            ColumnName = columnName
        });
    }
}
