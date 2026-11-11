using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SemesterProsjekt.Models;

/// <summary>
/// Brukeren i portalen. Arver fra IdentityUser, som allerede gir
/// e-post, passord-hash, ID osv. Her legger vi bare til navn.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // MaxLength begrenser kolonnelengden i databasen (via EF-migreringene).
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    // OrganizationId og Organization er nullable, fordi ikke alle brukere må tilhøre en organisasjon.
    public int? OrganizationId { get; set; }

    public Organization? Organization { get; set; }

}
