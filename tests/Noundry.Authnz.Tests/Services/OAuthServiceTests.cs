using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Noundry.Authnz.Models;
using Noundry.Authnz.Services;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Noundry.Authnz.Tests.Services;

[TestFixture]
public class OAuthServiceTests
{
    private Mock<ILogger<OAuthService>> _mockLogger = null!;
    private HttpClient _httpClient = null!;
    private OAuthSettings _settings = null!;
    private OAuthService _oauthService = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<OAuthService>>();
        _httpClient = new HttpClient();
        
        _settings = new OAuthSettings
        {
            Providers = new Dictionary<string, OAuthConfiguration>
            {
                ["google"] = new()
                {
                    ClientId = "test-google-client-id",
                    ClientSecret = "test-google-client-secret",
                    AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
                    TokenEndpoint = "https://oauth2.googleapis.com/token",
                    UserInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo",
                    Scopes = new List<string> { "openid", "profile", "email" }
                },
                ["github"] = new()
                {
                    ClientId = "test-github-client-id",
                    ClientSecret = "test-github-client-secret",
                    AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
                    TokenEndpoint = "https://github.com/login/oauth/access_token",
                    UserInfoEndpoint = "https://api.github.com/user",
                    Scopes = new List<string> { "user:email" }
                }
            }
        };

        var options = Options.Create(_settings);
        _oauthService = new OAuthService(options, _httpClient, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [Test]
    public void GenerateAuthorizationUrl_ValidProvider_ReturnsCorrectUrl()
    {
        var result = _oauthService.GenerateAuthorizationUrl("google");

        Assert.That(result, Does.StartWith("https://accounts.google.com/o/oauth2/v2/auth"));
        Assert.That(result, Does.Contain("client_id=test-google-client-id"));
        Assert.That(result, Does.Contain("response_type=code"));
        Assert.That(result, Does.Contain("scope=openid").Or.Contain("scope=openid+profile+email"));
    }

    [Test]
    public void GenerateAuthorizationUrl_InvalidProvider_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _oauthService.GenerateAuthorizationUrl("invalid"));
    }

    [Test]
    public void IsProviderConfigured_ConfiguredProvider_ReturnsTrue()
    {
        var result = _oauthService.IsProviderConfigured("google");
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsProviderConfigured_UnconfiguredProvider_ReturnsFalse()
    {
        var result = _oauthService.IsProviderConfigured("facebook");
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetConfiguredProviders_ReturnsOnlyConfiguredProviders()
    {
        var result = _oauthService.GetConfiguredProviders().ToList();
        
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Contains.Item("google"));
        Assert.That(result, Contains.Item("github"));
    }

    [Test]
    public void ExchangeCodeForTokenAsync_InvalidProvider_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => 
            await _oauthService.ExchangeCodeForTokenAsync("invalid", "test-code"));
    }

    [Test]
    public void GetUserInfoAsync_InvalidProvider_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => 
            await _oauthService.GetUserInfoAsync("invalid", "test-token"));
    }
}