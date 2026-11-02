using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-post må fylles ut.")]
    [EmailAddress(ErrorMessage = "Skriv inn en gyldig e-postadresse.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Passord må fylles ut.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Husk meg")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}