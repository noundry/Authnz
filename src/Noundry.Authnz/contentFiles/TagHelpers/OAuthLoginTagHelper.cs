using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Noundry.Authnz.Models;
using Noundry.Authnz.Services;

namespace Noundry.Authnz.TagHelpers;

[HtmlTargetElement("noundry-oauth-login")]
public class OAuthLoginTagHelper : TagHelper
{
    private readonly IOAuthService _oauthService;
    private readonly OAuthSettings _settings;

    public OAuthLoginTagHelper(IOAuthService oauthService, IOptions<OAuthSettings> settings)
    {
        _oauthService = oauthService;
        _settings = settings.Value;
    }

    [HtmlAttributeName("provider")]
    public string Provider { get; set; } = string.Empty;

    [HtmlAttributeName("button-text")]
    public string? ButtonText { get; set; }

    [HtmlAttributeName("button-class")]
    public string? ButtonClass { get; set; }

    [HtmlAttributeName("icon-class")]
    public string? IconClass { get; set; }

    [HtmlAttributeName("redirect-uri")]
    public string? RedirectUri { get; set; }

    [HtmlAttributeName("show-all")]
    public bool ShowAll { get; set; } = false;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "noundry-oauth-login");

        if (ShowAll)
        {
            var configuredProviders = _oauthService.GetConfiguredProviders().ToList();
            if (!configuredProviders.Any())
            {
                output.Content.SetContent("<p class=\"text-gray-500\">No OAuth providers configured</p>");
                return;
            }

            var buttonsHtml = new StringBuilder();
            foreach (var provider in configuredProviders)
            {
                buttonsHtml.Append(GenerateLoginButton(provider));
            }
            output.Content.SetHtmlContent(buttonsHtml.ToString());
        }
        else
        {
            if (!_oauthService.IsProviderConfigured(Provider))
            {
                output.Content.SetContent($"<p class=\"text-red-500\">Provider '{Provider}' is not configured</p>");
                return;
            }

            output.Content.SetHtmlContent(GenerateLoginButton(Provider));
        }
    }

    private string GenerateLoginButton(string provider)
    {
        var providerLower = provider.ToLowerInvariant();
        var displayName = GetProviderDisplayName(providerLower);
        var buttonText = ButtonText ?? $"Sign in with {displayName}";
        var iconHtml = GetProviderIcon(providerLower);
        var buttonClass = ButtonClass ?? GetDefaultButtonClass(providerLower);
        
        var loginUrl = _oauthService.GenerateAuthorizationUrl(provider, redirectUri: RedirectUri);

        return $@"
        <a href=""{loginUrl}"" 
           class=""{buttonClass} inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors duration-200 mb-2 w-full"">
            {iconHtml}
            <span class=""ml-2"">{buttonText}</span>
        </a>";
    }

    private static string GetProviderDisplayName(string provider) => provider switch
    {
        "google" => "Google",
        "microsoft" => "Microsoft",
        "github" => "GitHub",
        "apple" => "Apple",
        "facebook" => "Facebook",
        "twitter" => "Twitter",
        _ => provider
    };

    private string GetProviderIcon(string provider)
    {
        if (!string.IsNullOrEmpty(IconClass))
        {
            return $"<i class=\"{IconClass}\"></i>";
        }

        return provider switch
        {
            "google" => "<svg class=\"w-5 h-5\" viewBox=\"0 0 24 24\"><path fill=\"currentColor\" d=\"M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z\"/><path fill=\"currentColor\" d=\"M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z\"/><path fill=\"currentColor\" d=\"M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z\"/><path fill=\"currentColor\" d=\"M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z\"/></svg>",
            "microsoft" => "<svg class=\"w-5 h-5\" viewBox=\"0 0 24 24\"><path fill=\"#f25022\" d=\"M1 1h10v10H1z\"/><path fill=\"#00a4ef\" d=\"M13 1h10v10H13z\"/><path fill=\"#7fba00\" d=\"M1 13h10v10H1z\"/><path fill=\"#ffb900\" d=\"M13 13h10v10H13z\"/></svg>",
            "github" => "<svg class=\"w-5 h-5\" fill=\"currentColor\" viewBox=\"0 0 24 24\"><path d=\"M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z\"/></svg>",
            "apple" => "<svg class=\"w-5 h-5\" fill=\"currentColor\" viewBox=\"0 0 24 24\"><path d=\"M18.71 19.5c-.83 1.24-1.71 2.45-3.05 2.47-1.34.03-1.77-.79-3.29-.79-1.53 0-2 .77-3.27.82-1.31.05-2.3-1.32-3.14-2.53C4.25 17 2.94 12.45 4.7 9.39c.87-1.52 2.43-2.48 4.12-2.51 1.28-.02 2.5.87 3.29.87.78 0 2.26-1.07 3.81-.91.65.03 2.47.26 3.64 1.98-.09.06-2.17 1.28-2.15 3.81.03 3.02 2.65 4.03 2.68 4.04-.03.07-.42 1.44-1.38 2.83M13 3.5c.73-.83 1.94-1.46 2.94-1.5.13 1.17-.34 2.35-1.04 3.19-.69.85-1.83 1.51-2.95 1.42-.15-1.15.41-2.35 1.05-3.11z\"/></svg>",
            "facebook" => "<svg class=\"w-5 h-5\" fill=\"currentColor\" viewBox=\"0 0 24 24\"><path d=\"M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z\"/></svg>",
            "twitter" => "<svg class=\"w-5 h-5\" fill=\"currentColor\" viewBox=\"0 0 24 24\"><path d=\"M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z\"/></svg>",
            _ => "<svg class=\"w-5 h-5\" fill=\"currentColor\" viewBox=\"0 0 24 24\"><path d=\"M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z\"/></svg>"
        };
    }

    private static string GetDefaultButtonClass(string provider) => provider switch
    {
        "google" => "bg-white text-gray-700 border-gray-300 hover:bg-gray-50 focus:ring-blue-500",
        "microsoft" => "bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500",
        "github" => "bg-gray-900 text-white hover:bg-gray-800 focus:ring-gray-500",
        "apple" => "bg-black text-white hover:bg-gray-900 focus:ring-gray-500",
        "facebook" => "bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500",
        "twitter" => "bg-blue-400 text-white hover:bg-blue-500 focus:ring-blue-500",
        _ => "bg-gray-600 text-white hover:bg-gray-700 focus:ring-gray-500"
    };
}