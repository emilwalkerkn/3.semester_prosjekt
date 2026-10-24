using Microsoft.AspNetCore.Identity;

namespace SemesterProsjekt.Models;

public class PortalUser : IdentityUser<int>
{
    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";
}
