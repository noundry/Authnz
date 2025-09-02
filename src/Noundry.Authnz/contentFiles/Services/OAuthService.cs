using Microsoft.Extensions.Options;
using Noundry.Authnz.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Noundry.Authnz.Services;

public class OAuthService : IOAuthService
{
    private readonly OAuthSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(IOptions<OAuthSettings> settings, HttpClient httpClient, ILogger<OAuthService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public string GenerateAuthorizationUrl(string provider, string? state = null, string? redirectUri = null)
    {
        if (!_settings.Providers.TryGetValue(provider, out var config))
        {
            throw new ArgumentException($"Provider '{provider}' is not configured", nameof(provider));
        }

        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = config.ClientId,
            ["response_type"] = "code",
            ["scope"] = string.Join(" ", config.Scopes),
            ["redirect_uri"] = redirectUri ?? $"{GetBaseUrl()}{config.CallbackPath}",
            ["state"] = state ?? GenerateRandomString(32)
        };

        if (config.UsePkce)
        {
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            parameters["code_challenge"] = codeChallenge;
            parameters["code_challenge_method"] = "S256";
        }

        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
        return $"{config.AuthorizationEndpoint}?{queryString}";
    }

    public async Task<OAuthUserInfo?> HandleCallbackAsync(string provider, string code, string? state = null)
    {
        try
        {
            var accessToken = await ExchangeCodeForTokenAsync(provider, code);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Failed to exchange code for token for provider {Provider}", provider);
                return null;
            }

            return await GetUserInfoAsync(provider, accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback for provider {Provider}", provider);
            return null;
        }
    }

    public async Task<string?> ExchangeCodeForTokenAsync(string provider, string code)
    {
        if (!_settings.Providers.TryGetValue(provider, out var config))
        {
            throw new ArgumentException($"Provider '{provider}' is not configured", nameof(provider));
        }

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = $"{GetBaseUrl()}{config.CallbackPath}"
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(config.TokenEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Token exchange failed for provider {Provider}. Status: {Status}", 
                provider, response.StatusCode);
            return null;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

        return tokenResponse.TryGetProperty("access_token", out var token) ? token.GetString() : null;
    }

    public async Task<OAuthUserInfo?> GetUserInfoAsync(string provider, string accessToken)
    {
        if (!_settings.Providers.TryGetValue(provider, out var config))
        {
            throw new ArgumentException($"Provider '{provider}' is not configured", nameof(provider));
        }

        if (string.IsNullOrEmpty(config.UserInfoEndpoint))
        {
            _logger.LogWarning("UserInfo endpoint not configured for provider {Provider}", provider);
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, config.UserInfoEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("UserInfo request failed for provider {Provider}. Status: {Status}", 
                provider, response.StatusCode);
            return null;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var userInfoJson = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

        return MapUserInfo(provider, userInfoJson);
    }

    public bool IsProviderConfigured(string provider)
    {
        return _settings.Providers.ContainsKey(provider) &&
               !string.IsNullOrEmpty(_settings.Providers[provider].ClientId);
    }

    public IEnumerable<string> GetConfiguredProviders()
    {
        return _settings.Providers
            .Where(p => !string.IsNullOrEmpty(p.Value.ClientId))
            .Select(p => p.Key);
    }

    private OAuthUserInfo MapUserInfo(string provider, JsonElement userInfo)
    {
        var result = new OAuthUserInfo { Provider = provider };

        switch (provider.ToLowerInvariant())
        {
            case "google":
                result.Id = GetJsonProperty(userInfo, "sub") ?? GetJsonProperty(userInfo, "id") ?? "";
                result.Email = GetJsonProperty(userInfo, "email") ?? "";
                result.Name = GetJsonProperty(userInfo, "name") ?? "";
                result.FirstName = GetJsonProperty(userInfo, "given_name") ?? "";
                result.LastName = GetJsonProperty(userInfo, "family_name") ?? "";
                result.AvatarUrl = GetJsonProperty(userInfo, "picture") ?? "";
                break;

            case "microsoft":
                result.Id = GetJsonProperty(userInfo, "id") ?? "";
                result.Email = GetJsonProperty(userInfo, "mail") ?? GetJsonProperty(userInfo, "userPrincipalName") ?? "";
                result.Name = GetJsonProperty(userInfo, "displayName") ?? "";
                result.FirstName = GetJsonProperty(userInfo, "givenName") ?? "";
                result.LastName = GetJsonProperty(userInfo, "surname") ?? "";
                break;

            case "github":
                result.Id = GetJsonProperty(userInfo, "id") ?? "";
                result.Email = GetJsonProperty(userInfo, "email") ?? "";
                result.Name = GetJsonProperty(userInfo, "name") ?? GetJsonProperty(userInfo, "login") ?? "";
                result.AvatarUrl = GetJsonProperty(userInfo, "avatar_url") ?? "";
                break;

            case "facebook":
                result.Id = GetJsonProperty(userInfo, "id") ?? "";
                result.Email = GetJsonProperty(userInfo, "email") ?? "";
                result.Name = GetJsonProperty(userInfo, "name") ?? "";
                result.FirstName = GetJsonProperty(userInfo, "first_name") ?? "";
                result.LastName = GetJsonProperty(userInfo, "last_name") ?? "";
                var picture = userInfo.TryGetProperty("picture", out var pictureElement) && 
                             pictureElement.TryGetProperty("data", out var dataElement) && 
                             dataElement.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;
                result.AvatarUrl = picture ?? "";
                break;

            case "twitter":
                result.Id = GetJsonProperty(userInfo, "id") ?? "";
                result.Name = GetJsonProperty(userInfo, "name") ?? GetJsonProperty(userInfo, "username") ?? "";
                result.AvatarUrl = GetJsonProperty(userInfo, "profile_image_url") ?? "";
                break;

            default:
                result.Id = GetJsonProperty(userInfo, "sub") ?? GetJsonProperty(userInfo, "id") ?? "";
                result.Email = GetJsonProperty(userInfo, "email") ?? "";
                result.Name = GetJsonProperty(userInfo, "name") ?? "";
                break;
        }

        foreach (var property in userInfo.EnumerateObject())
        {
            result.AdditionalClaims[property.Name] = property.Value.ToString();
        }

        return result;
    }

    private static string? GetJsonProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private string GetBaseUrl()
    {
        return "https://localhost:5001";
    }
}