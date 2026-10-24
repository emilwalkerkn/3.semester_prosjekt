using System.ComponentModel.DataAnnotations;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.UnitTests;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Trygg123", true)]
    [InlineData("trygg123", false)]
    [InlineData("Tryggpassord", false)]
    [InlineData("Trygg12", false)]
    public void RegisterPassword_FollowsPasswordPolicy(string password, bool expectedValid)
    {
        var model = new RegisterViewModel { Password = password };

        Assert.Equal(expectedValid, IsPropertyValid(model, nameof(RegisterViewModel.Password), password));
    }

    [Theory]
    [InlineData("Trygg123", true)]
    [InlineData("trygg123", false)]
    [InlineData("Tryggpassord", false)]
    [InlineData("Trygg12", false)]
    public void ChangedPassword_FollowsPasswordPolicy(string password, bool expectedValid)
    {
        var model = new PasswordSettingsViewModel { NewPassword = password };

        Assert.Equal(expectedValid, IsPropertyValid(model, nameof(PasswordSettingsViewModel.NewPassword), password));
    }

    private static bool IsPropertyValid(object model, string propertyName, string value)
    {
        var validationResults = new List<ValidationResult>();
        return Validator.TryValidateProperty(value,
            new ValidationContext(model) { MemberName = propertyName }, validationResults);
    }
}
