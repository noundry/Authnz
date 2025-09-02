using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noundry.Authnz.Models;
using Noundry.Authnz.Services;
using System.Security.Claims;

namespace Noundry.Authnz.Controllers;

[Route("oauth")]
public class OAuthController : Controller
{
    private readonly IOAuthService _oauthService;
    private readonly IOAuthStateService _stateService;
    private readonly OAuthSettings _settings;
    private readonly ILogger<OAuthController> _logger;

    public OAuthController(IOAuthService oauthService, IOAuthStateService stateService, IOptions<OAuthSettings> settings, ILogger<OAuthController> logger)
    {
        _oauthService = oauthService;
        _stateService = stateService;
        _settings = settings.Value;
        _logger = logger;
    }

    [HttpGet("login/{provider}")]
    public IActionResult Login(string provider, string? redirectUri = null)
    {
        if (!_oauthService.IsProviderConfigured(provider))
        {
            _logger.LogWarning("OAuth login attempted for unconfigured provider: {Provider}", provider);
            return BadRequest($"Provider '{provider}' is not configured");
        }

        try
        {
            var state = _stateService.GenerateState(provider, redirectUri ?? _settings.DefaultRedirectUri);
            var authUrl = _oauthService.GenerateAuthorizationUrl(provider, state, redirectUri);
            
            return Redirect(authUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OAuth authorization URL for provider {Provider}", provider);
            return BadRequest("Failed to initiate OAuth flow");
        }
    }

    [HttpGet("callback/{provider}")]
    public async Task<IActionResult> Callback(string provider, string? code = null, string? state = null, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("OAuth callback received error for provider {Provider}: {Error}", provider, error);
            return Redirect(_settings.DefaultRedirectUri + "?error=oauth_error");
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("OAuth callback missing authorization code for provider {Provider}", provider);
            return BadRequest("Authorization code is required");
        }

        try
        {
            if (string.IsNullOrEmpty(state) || !_stateService.ValidateState(provider, state, out var redirectUriFromState))
            {
                _logger.LogWarning("OAuth state validation failed for provider {Provider}", provider);
                return BadRequest("Invalid state parameter");
            }

            var userInfo = await _oauthService.HandleCallbackAsync(provider, code, state);
            if (userInfo == null)
            {
                _logger.LogWarning("Failed to retrieve user info for provider {Provider}", provider);
                return Redirect(_settings.DefaultRedirectUri + "?error=oauth_failed");
            }

            await SignInUserAsync(userInfo);
            
            var finalRedirectUri = redirectUriFromState ?? _settings.DefaultRedirectUri;
            return Redirect(finalRedirectUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback for provider {Provider}", provider);
            return Redirect(_settings.DefaultRedirectUri + "?error=oauth_error");
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout(string? redirectUri = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(redirectUri ?? _settings.DefaultRedirectUri);
    }

    private async Task SignInUserAsync(OAuthUserInfo userInfo)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userInfo.Id),
            new(ClaimTypes.Name, userInfo.Name),
            new(ClaimTypes.Email, userInfo.Email),
            new("provider", userInfo.Provider),
            new("avatar_url", userInfo.AvatarUrl)
        };

        if (!string.IsNullOrEmpty(userInfo.FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, userInfo.FirstName));

        if (!string.IsNullOrEmpty(userInfo.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, userInfo.LastName));

        foreach (var additionalClaim in userInfo.AdditionalClaims)
        {
            claims.Add(new Claim(additionalClaim.Key, additionalClaim.Value?.ToString() ?? ""));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            });
    }

}