using SemesterProsjekt.Models;
using Xunit;

namespace SemesterProsjekt.UnitTests;

public class RegistrationSecurityTests
{
    [Fact]
    public void ResourceOwner_CanSelfRegister()
    {
        Assert.Contains(
            AppRoles.ResourceOwner,
            AppRoles.SelfServiceRoles);
    }

    [Fact]
    public void NeedsActor_CannotSelfRegister()
    {
        Assert.DoesNotContain(
            AppRoles.NeedsActor,
            AppRoles.SelfServiceRoles);
    }

    [Fact]
    public void Admin_CannotSelfRegister()
    {
        Assert.DoesNotContain(
            AppRoles.Admin,
            AppRoles.SelfServiceRoles);
    }
}