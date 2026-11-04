using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models.ViewModels;

/// <summary>
/// Feltene i innloggingsskjemaet. Attributtene gir validering
/// og feilmeldinger som vises direkte i skjemaet.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "E-post må fylles ut.")]
    [EmailAddress(ErrorMessage = "Skriv inn en gyldig e-postadresse.")]
    public string Email { get; set; } = "";

    // DataType.Password gjør at feltet vises som et skjult passordfelt.
    [Required(ErrorMessage = "Passord må fylles ut.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    // Hvis avkrysset, huskes innloggingen etter at nettleseren lukkes.
    [Display(Name = "Husk meg")]
    public bool RememberMe { get; set; }

    // Siden brukeren skal sendes tilbake til etter innlogging.
    public string? ReturnUrl { get; set; }
}
