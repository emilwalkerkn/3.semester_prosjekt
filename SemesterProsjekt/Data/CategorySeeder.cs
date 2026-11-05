using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

/// Sørger for at databasen inneholder riktige kategorier
/// for både behov og ressurser når applikasjonen starter.
public static class CategorySeeder
{
    /// Legger inn manglende kategorier og oppdaterer enkelte gamle kategorinavn.
    public static async Task SeedAsync(IServiceProvider services)
    {
        // Oppretter en scope og kobler til databasen.
        using var scope = services.CreateScope();
        using var connection = scope.ServiceProvider
            .GetRequiredService<IDbConnectionFactory>()
            .CreateConnection();

        connection.Open();

        // Bruker en transaksjon slik at alle endringer lagres samlet.
        using var transaction = connection.BeginTransaction();

        // Går gjennom kategoriene for både behov og ressurser.
        // Tabellenavnene er faste, mens ID og navn sendes som SQL-parametere.
        foreach (var (table, idColumn, categories) in new[]
        {
            ("NeedType", "NeedTypeId", CategoryCatalog.Needs),
            ("ResourceType", "ResourceTypeId", CategoryCatalog.Resources)
        })
        {
            foreach (var category in categories)
            {
                // Sjekker om kategorien allerede finnes i databasen.
                var existing = await connection.QuerySingleOrDefaultAsync<string>(
                    $"SELECT Name FROM {table} WHERE {idColumn} = @Id;",
                    category, transaction);

                if (existing is not null)
                {
                    // Sjekker om kategorien har et eldre navn som skal oppdateres.
                    // ID-en beholdes slik at eksisterende data ikke mister koblingene sine.
                    var legacyName =
                        (table == "NeedType" && category.Id == 2 && existing == "Transport") ||
                        (table == "ResourceType" && category.Id == 1 && existing == "Transport") ||
                        (table == "ResourceType" && category.Id == 3 && existing == "Matforsyning");

                    if (legacyName)
                    {
                        // Oppdaterer kategorien til det nye navnet.
                        await connection.ExecuteAsync(
                            $"UPDATE {table} SET Name = @Name WHERE {idColumn} = @Id;",
                            category, transaction);

                        continue;
                    }

                    // Stopper prosessen dersom samme ID brukes av en annen kategori.
                    if (!string.Equals(existing, category.Name, StringComparison.Ordinal))
                        throw new InvalidOperationException(
                            $"Kategori {table}/{category.Id} er allerede brukt av '{existing}'. Ingen kategorier ble overskrevet.");

                    continue;
                }

                // Legger til kategorien dersom den ikke finnes fra før.
                await connection.ExecuteAsync(
                    $"INSERT INTO {table} ({idColumn}, Name) VALUES (@Id, @Name);",
                    category, transaction);
            }
        }

        // Lagrer alle endringene når kategoriene er ferdig behandlet.
        transaction.Commit();
    }
}