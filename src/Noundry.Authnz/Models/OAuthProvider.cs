namespace Noundry.Authnz.Models;

public enum OAuthProvider
{
    Google,
    Microsoft,
    GitHub,
    Apple,
    Facebook,
    Twitter,
    Custom
}

public class OAuthConfiguration
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserInfoEndpoint { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public string CallbackPath { get; set; } = "/oauth/callback";
    public bool UsePkce { get; set; } = true;
}

public class OAuthSettings
{
    public const string SectionName = "OAuth";
    
    public Dictionary<string, OAuthConfiguration> Providers { get; set; } = new();
    public string DefaultRedirectUri { get; set; } = "/";
    public string LoginPath { get; set; } = "/oauth/login";
    public string LogoutPath { get; set; } = "/oauth/logout";
    public bool RequireHttpsMetadata { get; set; } = true;
}

public class OAuthUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalClaims { get; set; } = new();
}