using Dapper;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public class NeedRepository : INeedRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NeedRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Need>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
                           SELECT Id, Type, Description, Location, Latitude, Longitude, GeometryType, GeometryData, Status
                           FROM Needs;
                           """;
        
        return await connection.QueryAsync<Need>(sql);
    }

    public async Task<Need?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
                           SELECT Id, Type, Description, Location, Latitude, Longitude, Status
                           FROM Needs
                           WHERE Id = @Id;
                           """;

        return await connection.QuerySingleOrDefaultAsync<Need>(
            sql,
            new { Id = id }
        );
    }

    public async Task<int> CreateAsync(Need need)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
                           INSERT INTO Needs
                               (Type, Description, Location, Latitude, Longitude, GeometryType, GeometryData, Status)
                           VALUES
                               (@Type, @Description, @Location, @Latitude, @Longitude, @GeometryType, @GeometryData, @Status);
                           """;

        return await connection.ExecuteAsync(sql, need);
    }
    
    public async Task UpdateStatusAsync(int id, string status)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
                           UPDATE Needs
                           SET Status = @Status
                           WHERE Id = @Id;
                           """;

        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status
        });
    }
}
