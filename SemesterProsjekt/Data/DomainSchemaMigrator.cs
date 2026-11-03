using Dapper;
using System.Data;

namespace SemesterProsjekt.Data;

/// <summary>
/// Oppgraderer eldre Dapper-tabeller til dagens Identity-baserte bruker-ID-er.
/// Eldre lokale databaser lagret UserId som et tall, mens ASP.NET Identity
/// bruker tekstbaserte ID-er. Oppgraderingen kjøres trygt ved oppstart og er
/// uten effekt når kolonnene allerede har riktig type.
/// </summary>
public static class DomainSchemaMigrator
{
    private static readonly string[] TablesWithIdentityUserId = ["Needs", "Resources"];

    public static async Task MigrateAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var connectionFactory = scope.ServiceProvider
            .GetRequiredService<IDbConnectionFactory>();
        using var connection = connectionFactory.CreateConnection();

        foreach (var tableName in TablesWithIdentityUserId)
        {
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

            if (string.Equals(currentType, "varchar", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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
                // Tabellenavnene kommer fra den faste listen over, ikke fra brukerdata.
                await connection.ExecuteAsync(
                    $"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{foreignKeyName}`;");
            }

            // Tabellenavnene kommer fra den faste listen over, ikke fra brukerdata.
            await connection.ExecuteAsync(
                $"ALTER TABLE `{tableName}` MODIFY COLUMN UserId VARCHAR(255) NOT NULL;");
        }

        await MigrateMatchTableAsync(connection);
    }

    private static async Task MigrateMatchTableAsync(IDbConnection connection)
    {
        // Den tidligere Match-tabellen brukte CreatedAt og AssignedQuantity.
        // Match-funksjonen bruker nå MatchedAt, ResolvedAt og Notes.
        if (!await ColumnExistsAsync(connection, "Match", "MatchedAt"))
        {
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` ADD COLUMN MatchedAt DATETIME NULL;");
            await connection.ExecuteAsync(
                "UPDATE `Match` SET MatchedAt = CreatedAt WHERE MatchedAt IS NULL;");
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` MODIFY COLUMN MatchedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");
        }

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

        if (await ColumnExistsAsync(connection, "Match", "AssignedQuantity"))
        {
            // Behold feltet fra den gamle modellen, men gi det standardverdi
            // slik at dagens matchregistrering ikke stopper på eldre databaser.
            await connection.ExecuteAsync(
                "ALTER TABLE `Match` MODIFY COLUMN AssignedQuantity DECIMAL(10,2) NOT NULL DEFAULT 0;");
        }
    }

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

        return await connection.ExecuteScalarAsync<bool>(sql, new
        {
            TableName = tableName,
            ColumnName = columnName
        });
    }
}
