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
