using Noundry.Authnz.Models;

namespace Noundry.Authnz.Services;

public interface IOAuthService
{
    string GenerateAuthorizationUrl(string provider, string? state = null, string? redirectUri = null);
    Task<OAuthUserInfo?> HandleCallbackAsync(string provider, string code, string? state = null);
    Task<string?> ExchangeCodeForTokenAsync(string provider, string code);
    Task<OAuthUserInfo?> GetUserInfoAsync(string provider, string accessToken);
    bool IsProviderConfigured(string provider);
    IEnumerable<string> GetConfiguredProviders();
}