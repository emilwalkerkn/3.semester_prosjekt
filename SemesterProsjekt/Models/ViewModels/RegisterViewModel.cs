using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models.ViewModels;

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

    [Required(ErrorMessage = "Passord må fylles ut.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Passordet må ha minst 8 tegn.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$",
        ErrorMessage = "Passordet må inneholde stor bokstav, liten bokstav og tall.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Bekreft passordet.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
    public string ConfirmPassword { get; set; } = "";

    [Required(ErrorMessage = "Velg aktørtype.")]
    public string Role { get; set; } = AppRoles.NeedsActor;

    public string? ReturnUrl { get; set; }
}
