using Microsoft.Extensions.Configuration;
using Noundry.Authnz.Models;

namespace Noundry.Authnz.Extensions;

public static class OAuthConfigurationExtensions
{
    public static void ConfigureOAuthProvider(this OAuthSettings settings, OAuthProvider provider, string clientId, string clientSecret)
    {
        var providerName = provider.ToString().ToLowerInvariant();
        
        if (!OAuthProviderDefaults.ProviderConfigs.TryGetValue(provider, out var defaultConfig))
        {
            throw new ArgumentException($"Unknown OAuth provider: {provider}");
        }

        var config = new OAuthConfiguration
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            AuthorizationEndpoint = defaultConfig.AuthorizationEndpoint,
            TokenEndpoint = defaultConfig.TokenEndpoint,
            UserInfoEndpoint = defaultConfig.UserInfoEndpoint,
            Scopes = new List<string>(defaultConfig.Scopes),
            CallbackPath = defaultConfig.CallbackPath,
            UsePkce = defaultConfig.UsePkce
        };

        settings.Providers[providerName] = config;
    }

    public static void ConfigureCustomOAuthProvider(this OAuthSettings settings, string providerName, 
        string clientId, string clientSecret, string authorizationEndpoint, string tokenEndpoint, 
        string userInfoEndpoint, List<string>? scopes = null, bool usePkce = true)
    {
        var config = new OAuthConfiguration
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            AuthorizationEndpoint = authorizationEndpoint,
            TokenEndpoint = tokenEndpoint,
            UserInfoEndpoint = userInfoEndpoint,
            Scopes = scopes ?? new List<string>(),
            CallbackPath = "/oauth/callback",
            UsePkce = usePkce
        };

        settings.Providers[providerName.ToLowerInvariant()] = config;
    }

    public static OAuthSettings LoadFromConfiguration(this OAuthSettings settings, IConfiguration configuration)
    {
        var oauthSection = configuration.GetSection(OAuthSettings.SectionName);
        if (!oauthSection.Exists())
        {
            return settings;
        }

        oauthSection.Bind(settings);

        foreach (var providerSection in oauthSection.GetSection("Providers").GetChildren())
        {
            var providerName = providerSection.Key.ToLowerInvariant();
            var config = new OAuthConfiguration();
            providerSection.Bind(config);

            if (Enum.TryParse<OAuthProvider>(providerName, ignoreCase: true, out var knownProvider) &&
                OAuthProviderDefaults.ProviderConfigs.TryGetValue(knownProvider, out var defaults))
            {
                config.AuthorizationEndpoint = string.IsNullOrEmpty(config.AuthorizationEndpoint) 
                    ? defaults.AuthorizationEndpoint 
                    : config.AuthorizationEndpoint;
                config.TokenEndpoint = string.IsNullOrEmpty(config.TokenEndpoint) 
                    ? defaults.TokenEndpoint 
                    : config.TokenEndpoint;
                config.UserInfoEndpoint = string.IsNullOrEmpty(config.UserInfoEndpoint) 
                    ? defaults.UserInfoEndpoint 
                    : config.UserInfoEndpoint;
                
                if (config.Scopes?.Count == 0)
                    config.Scopes = new List<string>(defaults.Scopes);
            }

            settings.Providers[providerName] = config;
        }

        return settings;
    }
}