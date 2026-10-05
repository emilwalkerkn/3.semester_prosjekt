using System.Data;

namespace SemesterProsjekt.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}