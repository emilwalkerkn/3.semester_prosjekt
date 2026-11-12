using System;
using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

public class Organization
{
    public int OrganizationId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(9)]
    public string? OrganizationNumber { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationUser> Users { get; set; }
        = new List<ApplicationUser>();
    
    [StringLength(253)]
    public string? AllowedEmailDomain { get; set; }
    
    
    [StringLength(200)]
    public string? ContactPersonName { get; set; }

    [EmailAddress]
    [StringLength(256)]
    public string? ContactEmail { get; set; }


}