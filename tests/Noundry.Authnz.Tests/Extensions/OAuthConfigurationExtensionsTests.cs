using Microsoft.Extensions.Configuration;
using Noundry.Authnz.Extensions;
using Noundry.Authnz.Models;
using NUnit.Framework;

namespace Noundry.Authnz.Tests.Extensions;

[TestFixture]
public class OAuthConfigurationExtensionsTests
{
    private OAuthSettings _settings;

    [SetUp]
    public void Setup()
    {
        _settings = new OAuthSettings();
    }

    [Test]
    public void ConfigureOAuthProvider_GoogleProvider_ConfiguresCorrectly()
    {
        _settings.ConfigureOAuthProvider(OAuthProvider.Google, "test-client-id", "test-client-secret");

        Assert.That(_settings.Providers.ContainsKey("google"), Is.True);
        
        var config = _settings.Providers["google"];
        Assert.That(config.ClientId, Is.EqualTo("test-client-id"));
        Assert.That(config.ClientSecret, Is.EqualTo("test-client-secret"));
        Assert.That(config.AuthorizationEndpoint, Is.EqualTo("https://accounts.google.com/o/oauth2/v2/auth"));
        Assert.That(config.Scopes, Contains.Item("openid"));
    }

    [Test]
    public void ConfigureCustomOAuthProvider_ConfiguresCorrectly()
    {
        var scopes = new List<string> { "read", "write" };
        
        _settings.ConfigureCustomOAuthProvider("custom", "client-id", "client-secret",
            "https://custom.com/auth", "https://custom.com/token", "https://custom.com/user", scopes);

        Assert.That(_settings.Providers.ContainsKey("custom"), Is.True);
        
        var config = _settings.Providers["custom"];
        Assert.That(config.ClientId, Is.EqualTo("client-id"));
        Assert.That(config.AuthorizationEndpoint, Is.EqualTo("https://custom.com/auth"));
        Assert.That(config.Scopes, Is.EqualTo(scopes));
    }

    [Test]
    public void LoadFromConfiguration_ValidConfiguration_LoadsCorrectly()
    {
        var configData = new Dictionary<string, string>
        {
            ["OAuth:DefaultRedirectUri"] = "/home",
            ["OAuth:Providers:google:ClientId"] = "config-client-id",
            ["OAuth:Providers:google:ClientSecret"] = "config-client-secret",
            ["OAuth:Providers:custom:ClientId"] = "custom-client-id",
            ["OAuth:Providers:custom:ClientSecret"] = "custom-client-secret",
            ["OAuth:Providers:custom:AuthorizationEndpoint"] = "https://custom.com/auth",
            ["OAuth:Providers:custom:TokenEndpoint"] = "https://custom.com/token",
            ["OAuth:Providers:custom:UserInfoEndpoint"] = "https://custom.com/user"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _settings.LoadFromConfiguration(configuration);

        Assert.That(_settings.DefaultRedirectUri, Is.EqualTo("/home"));
        Assert.That(_settings.Providers.ContainsKey("google"), Is.True);
        Assert.That(_settings.Providers["google"].ClientId, Is.EqualTo("config-client-id"));
        Assert.That(_settings.Providers["google"].AuthorizationEndpoint, 
            Is.EqualTo("https://accounts.google.com/o/oauth2/v2/auth"));
        
        Assert.That(_settings.Providers.ContainsKey("custom"), Is.True);
        Assert.That(_settings.Providers["custom"].AuthorizationEndpoint, Is.EqualTo("https://custom.com/auth"));
    }

    [Test]
    public void ConfigureOAuthProvider_UnknownProvider_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            _settings.ConfigureOAuthProvider((OAuthProvider)999, "client-id", "client-secret"));
    }
}