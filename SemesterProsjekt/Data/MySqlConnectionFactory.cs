using System.Data;
using MySqlConnector;

namespace SemesterProsjekt.Data;


/// Oppretter tilkoblinger til MariaDB ved hjelp av MySqlConnector.
/// Repositoryene bruker denne klassen gjennom IDbConnectionFactory.

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    
    /// Leser DefaultConnection fra applikasjonens konfigurasjon.
    /// Verdien kan komme fra appsettings.json eller miljøvariabler i Docker.
    /// Avbryter dersom tilkoblingsstrengen mangler.
    
    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException(
                                "Connection string not found.");
    }

    
    /// Oppretter et nytt tilkoblingsobjekt uten å åpne forbindelsen.
    /// Dapper kan åpne forbindelsen når en databasespørring utføres.
    
    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}