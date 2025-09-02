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
    private Mock<ILogger<OAuthService>> _mockLogger;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private OAuthSettings _settings;
    private OAuthService _oauthService;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<OAuthService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        
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

    [Test]
    public void GenerateAuthorizationUrl_ValidProvider_ReturnsCorrectUrl()
    {
        var result = _oauthService.GenerateAuthorizationUrl("google");

        Assert.That(result, Does.StartWith("https://accounts.google.com/o/oauth2/v2/auth"));
        Assert.That(result, Does.Contain("client_id=test-google-client-id"));
        Assert.That(result, Does.Contain("response_type=code"));
        Assert.That(result, Does.Contain("scope=openid%20profile%20email"));
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
    public async Task ExchangeCodeForTokenAsync_ValidResponse_ReturnsAccessToken()
    {
        var tokenResponse = new { access_token = "test-access-token", token_type = "Bearer" };
        var jsonResponse = JsonSerializer.Serialize(tokenResponse);
        
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _oauthService.ExchangeCodeForTokenAsync("google", "test-code");

        Assert.That(result, Is.EqualTo("test-access-token"));
    }

    [Test]
    public async Task GetUserInfoAsync_GoogleProvider_ReturnsCorrectUserInfo()
    {
        var userResponse = new
        {
            sub = "123456789",
            email = "test@example.com",
            name = "Test User",
            given_name = "Test",
            family_name = "User",
            picture = "https://example.com/avatar.jpg"
        };
        var jsonResponse = JsonSerializer.Serialize(userResponse);
        
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _oauthService.GetUserInfoAsync("google", "test-token");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("123456789"));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.Name, Is.EqualTo("Test User"));
        Assert.That(result.FirstName, Is.EqualTo("Test"));
        Assert.That(result.LastName, Is.EqualTo("User"));
        Assert.That(result.AvatarUrl, Is.EqualTo("https://example.com/avatar.jpg"));
        Assert.That(result.Provider, Is.EqualTo("google"));
    }

    [Test]
    public async Task GetUserInfoAsync_GitHubProvider_ReturnsCorrectUserInfo()
    {
        var userResponse = new
        {
            id = "987654321",
            login = "testuser",
            name = "Test User",
            email = "test@example.com",
            avatar_url = "https://github.com/avatar.jpg"
        };
        var jsonResponse = JsonSerializer.Serialize(userResponse);
        
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _oauthService.GetUserInfoAsync("github", "test-token");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("987654321"));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.Name, Is.EqualTo("Test User"));
        Assert.That(result.AvatarUrl, Is.EqualTo("https://github.com/avatar.jpg"));
        Assert.That(result.Provider, Is.EqualTo("github"));
    }

    [Test]
    public async Task HandleCallbackAsync_ValidCode_ReturnsUserInfo()
    {
        var tokenResponse = new { access_token = "test-access-token" };
        var userResponse = new { sub = "123", name = "Test User", email = "test@example.com" };
        
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(userResponse));

        var result = await _oauthService.HandleCallbackAsync("google", "test-code");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Provider, Is.EqualTo("google"));
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}