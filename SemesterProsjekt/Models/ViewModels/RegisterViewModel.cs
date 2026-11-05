using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models.ViewModels;

/// <summary>
/// Feltene i registreringsskjemaet for nye brukere.
/// </summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "Fornavn må fylles ut.")]
    [StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Etternavn må fylles ut.")]
    [StringLength(100)]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "E-post må fylles ut.")]
    [EmailAddress(ErrorMessage = "Skriv inn en gyldig e-postadresse.")]
    public string Email { get; set; } = "";

    // Passordkrav: 8–100 tegn, og regex-en krever minst én stor bokstav,
    // én liten bokstav og ett tall.
    [Required(ErrorMessage = "Passord må fylles ut.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Passordet må ha minst 8 tegn.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$",
        ErrorMessage = "Passordet må inneholde stor bokstav, liten bokstav og tall.")]
    public string Password { get; set; } = "";

    // Compare sjekker at feltet er likt Password.
    [Required(ErrorMessage = "Bekreft passordet.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
    public string ConfirmPassword { get; set; } = "";

    // Rollen brukeren velger. Kontrolleren sjekker at den er i AppRoles.SelfServiceRoles.
    [Required(ErrorMessage = "Velg aktørtype.")]
    public string Role { get; set; } = AppRoles.ResourceOwner;

    // Siden brukeren skal sendes tilbake til etter registrering.
    public string? ReturnUrl { get; set; }
}
