using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public static class CategorySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        using var connection = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        // Tabellenavnene er faste programverdier; navn og ID sendes som SQL-parametere.
        foreach (var (table, idColumn, categories) in new[]
        {
            ("NeedType", "NeedTypeId", CategoryCatalog.Needs),
            ("ResourceType", "ResourceTypeId", CategoryCatalog.Resources)
        })
        {
            foreach (var category in categories)
            {
                var existing = await connection.QuerySingleOrDefaultAsync<string>(
                    $"SELECT Name FROM {table} WHERE {idColumn} = @Id;", category, transaction);
                if (existing is not null)
                {
                    // Oppdater bare kjente, tidligere navn til de felles kategorinavnene.
                    // ID-ene beholdes, så eksisterende ressurser og avtaler flyttes ikke.
                    var legacyName =
                        (table == "NeedType" && category.Id == 2 && existing == "Transport") ||
                        (table == "ResourceType" && category.Id == 1 && existing == "Transport") ||
                        (table == "ResourceType" && category.Id == 3 && existing == "Matforsyning");
                    if (legacyName)
                    {
                        await connection.ExecuteAsync($"UPDATE {table} SET Name = @Name WHERE {idColumn} = @Id;", category, transaction);
                        continue;
                    }
                    if (!string.Equals(existing, category.Name, StringComparison.Ordinal))
                        throw new InvalidOperationException($"Kategori {table}/{category.Id} er allerede brukt av '{existing}'. Ingen kategorier ble overskrevet.");
                    continue;
                }
                await connection.ExecuteAsync($"INSERT INTO {table} ({idColumn}, Name) VALUES (@Id, @Name);", category, transaction);
            }
        }
        transaction.Commit();
    }
}
