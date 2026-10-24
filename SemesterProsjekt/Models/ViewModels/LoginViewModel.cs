using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Skriv inn brukernavn.")]
    [Display(Name = "Brukernavn")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Skriv inn passord.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passord")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Skriv inn fornavn.")]
    [StringLength(100)]
    [Display(Name = "Fornavn")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Skriv inn etternavn.")]
    [StringLength(100)]
    [Display(Name = "Etternavn")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Velg et brukernavn.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "Brukernavn må være mellom 3 og 80 tegn.")]
    [Display(Name = "Brukernavn")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Skriv inn e-postadresse.")]
    [EmailAddress(ErrorMessage = "Skriv inn en gyldig e-postadresse.")]
    [StringLength(254)]
    [Display(Name = "E-postadresse")]
    public string Email { get; set; } = "";

    [StringLength(30)]
    [Phone(ErrorMessage = "Skriv inn et gyldig telefonnummer.")]
    [Display(Name = "Telefonnummer")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Velg hvilken type bruker du er.")]
    [Display(Name = "Jeg skal")]
    public string Role { get; set; } = "";

    [Required(ErrorMessage = "Velg et passord.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Passordet må være minst 8 tegn.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[A-Z]).{8,}$",
        ErrorMessage = "Passordet må ha minst 8 tegn, minst én stor bokstav og inneholde både bokstaver og tall.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passord")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Bekreft passordet.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
    [Display(Name = "Bekreft passord")]
    public string ConfirmPassword { get; set; } = "";
}
