using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models.ViewModels;

/// <summary>
/// Samler informasjonen som vises på innstillingssiden.
/// </summary>
public class SettingsPageViewModel
{
    // Siden har to separate skjemaer: ett for profil og ett for passord.
    public ProfileSettingsViewModel Profile { get; set; } = new();

    public PasswordSettingsViewModel Password { get; set; } = new();
}

/// <summary>
/// Skjemaet for å endre navn og e-post.
/// </summary>
public class ProfileSettingsViewModel
{
    [Required(ErrorMessage = "Fornavn må fylles ut.")]
    [Display(Name = "Fornavn")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Etternavn må fylles ut.")]
    [Display(Name = "Etternavn")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "E-postadresse må fylles ut.")]
    [EmailAddress(ErrorMessage = "Skriv inn en gyldig e-postadresse.")]
    [Display(Name = "E-postadresse")]
    public string Email { get; set; } = "";

    // Brukerens rolle, kun for visning. "Bruker" hvis brukeren ikke har noen rolle.
    public string Role { get; set; } = "Bruker";
}

/// <summary>
/// Skjemaet for å bytte passord.
/// </summary>
public class PasswordSettingsViewModel
{
    // Nåværende passord kreves for å bekrefte at det er brukeren selv som bytter.
    [Required(ErrorMessage = "Skriv inn nåværende passord.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nåværende passord")]
    public string CurrentPassword { get; set; } = "";

    // Regex-en krever minst 8 tegn, minst én bokstav, ett tall og én stor bokstav.
    [Required(ErrorMessage = "Skriv inn et nytt passord.")]
    [MinLength(8, ErrorMessage = "Nytt passord må være minst 8 tegn.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[A-Z]).{8,}$",
        ErrorMessage = "Passordet må ha minst 8 tegn, minst én stor bokstav og inneholde både bokstaver og tall.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nytt passord")]
    public string NewPassword { get; set; } = "";

    // Compare sjekker at feltet er likt NewPassword.
    [Required(ErrorMessage = "Bekreft det nye passordet.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "De nye passordene er ikke like.")]
    [Display(Name = "Bekreft nytt passord")]
    public string ConfirmPassword { get; set; } = "";
}
