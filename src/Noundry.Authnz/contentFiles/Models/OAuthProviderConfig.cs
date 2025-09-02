namespace Noundry.Authnz.Models;

public static class OAuthProviderDefaults
{
    public static readonly Dictionary<OAuthProvider, OAuthConfiguration> ProviderConfigs = new()
    {
        [OAuthProvider.Google] = new()
        {
            AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
            TokenEndpoint = "https://oauth2.googleapis.com/token",
            UserInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo",
            Scopes = new List<string> { "openid", "profile", "email" }
        },
        [OAuthProvider.Microsoft] = new()
        {
            AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize",
            TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            UserInfoEndpoint = "https://graph.microsoft.com/v1.0/me",
            Scopes = new List<string> { "openid", "profile", "email" }
        },
        [OAuthProvider.GitHub] = new()
        {
            AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
            TokenEndpoint = "https://github.com/login/oauth/access_token",
            UserInfoEndpoint = "https://api.github.com/user",
            Scopes = new List<string> { "user:email" }
        },
        [OAuthProvider.Apple] = new()
        {
            AuthorizationEndpoint = "https://appleid.apple.com/auth/authorize",
            TokenEndpoint = "https://appleid.apple.com/auth/token",
            UserInfoEndpoint = "",
            Scopes = new List<string> { "name", "email" }
        },
        [OAuthProvider.Facebook] = new()
        {
            AuthorizationEndpoint = "https://www.facebook.com/v18.0/dialog/oauth",
            TokenEndpoint = "https://graph.facebook.com/v18.0/oauth/access_token",
            UserInfoEndpoint = "https://graph.facebook.com/v18.0/me",
            Scopes = new List<string> { "email", "public_profile" }
        },
        [OAuthProvider.Twitter] = new()
        {
            AuthorizationEndpoint = "https://twitter.com/i/oauth2/authorize",
            TokenEndpoint = "https://api.twitter.com/2/oauth2/token",
            UserInfoEndpoint = "https://api.twitter.com/2/users/me",
            Scopes = new List<string> { "tweet.read", "users.read" },
            UsePkce = true
        }
    };
}