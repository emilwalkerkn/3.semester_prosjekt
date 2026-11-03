using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SemesterProsjekt.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}
