using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.ViewModels;

public class InvitedEmployeeRegisterViewModel
{
    [Required(ErrorMessage = "Fornavn er påkrevd.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Etternavn er påkrevd.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passord er påkrevd.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bekreft passordet.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password),
        ErrorMessage = "Passordene må være like.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}