using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models.ViewModels;

public class OrganizationApplicationViewModel
{
    [Required(ErrorMessage = "Organisasjonsnavn må fylles ut.")]
    [StringLength(200)]
    public string OrganizationName { get; set; } = "";

    [Required(ErrorMessage = "Organisasjonsnummer må fylles ut.")]
    [RegularExpression(@"^\d{9}$",
        ErrorMessage = "Organisasjonsnummeret må bestå av 9 siffer.")]
    public string OrganizationNumber { get; set; } = "";

    [Required(ErrorMessage = "E-postdomene må fylles ut.")]
    [StringLength(253)]
    public string EmailDomain { get; set; } = "";

    [Required(ErrorMessage = "Navn på kontaktperson må fylles ut.")]
    [StringLength(200)]
    public string ContactPersonName { get; set; } = "";

    [Required(ErrorMessage = "E-post til kontaktperson må fylles ut.")]
    [EmailAddress(ErrorMessage = "Skriv inn en gyldig e-postadresse.")]
    public string ContactEmail { get; set; } = "";
}