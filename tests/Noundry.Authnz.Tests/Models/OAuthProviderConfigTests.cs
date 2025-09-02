using Noundry.Authnz.Models;
using NUnit.Framework;

namespace Noundry.Authnz.Tests.Models;

[TestFixture]
public class OAuthProviderConfigTests
{
    [Test]
    public void OAuthProviderDefaults_ContainsAllSupportedProviders()
    {
        var expectedProviders = new[]
        {
            OAuthProvider.Google,
            OAuthProvider.Microsoft,
            OAuthProvider.GitHub,
            OAuthProvider.Apple,
            OAuthProvider.Facebook,
            OAuthProvider.Twitter
        };

        foreach (var provider in expectedProviders)
        {
            Assert.That(OAuthProviderDefaults.ProviderConfigs.ContainsKey(provider), 
                Is.True, $"Provider {provider} should be configured in defaults");
        }
    }

    [Test]
    public void GoogleProvider_HasCorrectConfiguration()
    {
        var config = OAuthProviderDefaults.ProviderConfigs[OAuthProvider.Google];

        Assert.That(config.AuthorizationEndpoint, Is.EqualTo("https://accounts.google.com/o/oauth2/v2/auth"));
        Assert.That(config.TokenEndpoint, Is.EqualTo("https://oauth2.googleapis.com/token"));
        Assert.That(config.UserInfoEndpoint, Is.EqualTo("https://www.googleapis.com/oauth2/v2/userinfo"));
        Assert.That(config.Scopes, Contains.Item("openid"));
        Assert.That(config.Scopes, Contains.Item("profile"));
        Assert.That(config.Scopes, Contains.Item("email"));
    }

    [Test]
    public void GitHubProvider_HasCorrectConfiguration()
    {
        var config = OAuthProviderDefaults.ProviderConfigs[OAuthProvider.GitHub];

        Assert.That(config.AuthorizationEndpoint, Is.EqualTo("https://github.com/login/oauth/authorize"));
        Assert.That(config.TokenEndpoint, Is.EqualTo("https://github.com/login/oauth/access_token"));
        Assert.That(config.UserInfoEndpoint, Is.EqualTo("https://api.github.com/user"));
        Assert.That(config.Scopes, Contains.Item("user:email"));
    }

    [Test]
    public void MicrosoftProvider_HasCorrectConfiguration()
    {
        var config = OAuthProviderDefaults.ProviderConfigs[OAuthProvider.Microsoft];

        Assert.That(config.AuthorizationEndpoint, Is.EqualTo("https://login.microsoftonline.com/common/oauth2/v2.0/authorize"));
        Assert.That(config.TokenEndpoint, Is.EqualTo("https://login.microsoftonline.com/common/oauth2/v2.0/token"));
        Assert.That(config.UserInfoEndpoint, Is.EqualTo("https://graph.microsoft.com/v1.0/me"));
    }

    [Test]
    public void AllProviders_HaveValidEndpoints()
    {
        foreach (var provider in OAuthProviderDefaults.ProviderConfigs.Values)
        {
            Assert.That(provider.AuthorizationEndpoint, Is.Not.Null.And.Not.Empty);
            Assert.That(provider.TokenEndpoint, Is.Not.Null.And.Not.Empty);
            Assert.That(provider.Scopes, Is.Not.Null);
            
            Assert.That(Uri.IsWellFormedUriString(provider.AuthorizationEndpoint, UriKind.Absolute), 
                Is.True, $"Authorization endpoint should be valid URL: {provider.AuthorizationEndpoint}");
            Assert.That(Uri.IsWellFormedUriString(provider.TokenEndpoint, UriKind.Absolute), 
                Is.True, $"Token endpoint should be valid URL: {provider.TokenEndpoint}");
        }
    }
}