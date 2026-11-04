using System.Data;

namespace SemesterProsjekt.Data;


/// Definerer hvordan repositoryene kan opprette en databasetilkobling.
/// Grensesnittet skiller repositoryene fra den konkrete databaseleverandøren.

public interface IDbConnectionFactory
{

    /// Oppretter en ny databasetilkobling.
    /// Koden som bruker tilkoblingen, har ansvar for å frigjøre den etter bruk.
    
    IDbConnection CreateConnection();
}