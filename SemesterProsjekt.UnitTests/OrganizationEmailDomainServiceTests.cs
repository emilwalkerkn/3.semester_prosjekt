using SemesterProsjekt.Services;
using Xunit;

namespace SemesterProsjekt.UnitTests;

public class OrganizationEmailDomainServiceTests
{
    [Theory]
    [InlineData("kari@agderpoliti.no", true)]
    [InlineData("KARI@AGDERPOLITI.NO", true)]
    [InlineData("kari@gmail.com", false)]
    [InlineData("kari@falskagderpoliti.no", false)]
    [InlineData("kari@sub.agderpoliti.no", false)]
    public void IsAllowed_ChecksCorrectDomain(
        string email,
        bool expected)
    {
        var result =
            OrganizationEmailDomainService.IsAllowed(
                email,
                "agderpoliti.no");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsAllowed_RejectsMissingOrganizationDomain()
    {
        var result =
            OrganizationEmailDomainService.IsAllowed(
                "kari@agderpoliti.no",
                null);

        Assert.False(result);
    }
}