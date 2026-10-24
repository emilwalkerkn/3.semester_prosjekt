using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

public class SettingsPageViewModel
{
    public ProfileSettingsViewModel Profile { get; set; } = new();

    public PasswordSettingsViewModel Password { get; set; } = new();
}

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

    public string Role { get; set; } = "Offentlig aktør";

    public string Username { get; set; } = "";
}

public class PasswordSettingsViewModel
{
    [Required(ErrorMessage = "Skriv inn nåværende passord.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nåværende passord")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "Skriv inn et nytt passord.")]
    [MinLength(8, ErrorMessage = "Nytt passord må være minst 8 tegn.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nytt passord")]
    public string NewPassword { get; set; } = "";
}
