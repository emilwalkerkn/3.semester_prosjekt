using SemesterProsjekt.Services;
using Xunit;

namespace SemesterProsjekt.UnitTests;

public class InvitationTokenServiceTests
{
    [Fact]
    public void GenerateToken_Creates64CharacterToken()
    {
        var token = InvitationTokenService.GenerateToken();

        Assert.Equal(64, token.Length);
    }

    [Fact]
    public void GenerateToken_CreatesDifferentTokens()
    {
        var firstToken = InvitationTokenService.GenerateToken();
        var secondToken = InvitationTokenService.GenerateToken();

        Assert.NotEqual(firstToken, secondToken);
    }

    [Fact]
    public void HashToken_CreatesConsistentHash()
    {
        var token = InvitationTokenService.GenerateToken();

        var hash1 = InvitationTokenService.HashToken(token);
        var hash2 = InvitationTokenService.HashToken(token);

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length);
        Assert.NotEqual(token, hash1);
    }
}