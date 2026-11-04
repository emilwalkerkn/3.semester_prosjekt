using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;


/// Representerer Identity-delen av databasen i Entity Framework Core.
/// Arver standardoppsettet for brukere, roller, påstander og innlogging
/// fra IdentityDbContext, med ApplicationUser som brukermodell.
/// Behov, ressurser og matcher håndteres separat gjennom Dapper-repositoryene.

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{

    /// Mottar databaseinnstillingene gjennom dependency injection.
    /// Innstillingene konfigureres i Program.cs og sendes til baseklassen,
    /// som bruker dem til å koble Identity til databasen.
  
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}