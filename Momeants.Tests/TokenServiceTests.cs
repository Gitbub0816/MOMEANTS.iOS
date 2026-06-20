using Microsoft.Extensions.Options;
using Momeants.Api.Domain;
using Momeants.Api.Options;
using Momeants.Api.Services;
using Xunit;
using Xunit.Abstractions;

namespace Momeants.Tests;

public class TokenServiceTests
{
    private readonly ITestOutputHelper _out;
    public TokenServiceTests(ITestOutputHelper output) => _out = output;

    private const string TestSecret = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    private static TokenService CreateService() =>
        new(Options.Create(new JwtOptions
        {
            SecretKey = TestSecret,
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenMinutes = 60,
            RefreshTokenDays = 30
        }));

    [Fact]
    public void GenerateTokens_ReturnsNonEmptyPair()
    {
        var svc = CreateService();
        var user = new User { Id = Guid.NewGuid(), ClerkUserId = "clerk_123", DisplayName = "Test" };
        var pair = svc.GenerateTokens(user);

        Assert.NotEmpty(pair.AccessToken);
        Assert.NotEmpty(pair.RefreshToken);
        Assert.True(pair.AccessTokenExpiry > DateTimeOffset.UtcNow);
        Assert.True(pair.RefreshTokenExpiry > DateTimeOffset.UtcNow.AddDays(29));
    }

    [Fact]
    public void ValidateAccessToken_ValidToken_ReturnsTrue()
    {
        var svc = CreateService();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, ClerkUserId = "clerk_456", DisplayName = "Test" };
        var pair = svc.GenerateTokens(user);

        _out.WriteLine($"Generated token (first 30): {pair.AccessToken[..30]}");

        var valid = svc.ValidateAccessToken(pair.AccessToken, out var parsedUserId, out var clerkId);

        _out.WriteLine($"Valid: {valid}, UserId: {parsedUserId}, ClerkId: {clerkId}");

        Assert.True(valid);
        Assert.Equal(userId, parsedUserId);
        Assert.Equal("clerk_456", clerkId);
    }

    [Fact]
    public void ValidateAccessToken_InvalidToken_ReturnsFalse()
    {
        var svc = CreateService();
        Assert.False(svc.ValidateAccessToken("not.a.valid.jwt", out _, out _));
    }

    [Fact]
    public void HashRefreshToken_Deterministic()
    {
        var svc = CreateService();
        Assert.Equal(svc.HashRefreshToken("abc"), svc.HashRefreshToken("abc"));
    }

    [Fact]
    public void HashRefreshToken_DifferentInputs_DifferentHashes()
    {
        var svc = CreateService();
        Assert.NotEqual(svc.HashRefreshToken("token-a"), svc.HashRefreshToken("token-b"));
    }
}
