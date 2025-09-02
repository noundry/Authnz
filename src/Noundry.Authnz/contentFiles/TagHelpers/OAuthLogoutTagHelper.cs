using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Noundry.Authnz.Models;

namespace Noundry.Authnz.TagHelpers;

[HtmlTargetElement("noundry-oauth-logout")]
public class OAuthLogoutTagHelper : TagHelper
{
    private readonly OAuthSettings _settings;

    public OAuthLogoutTagHelper(IOptions<OAuthSettings> settings)
    {
        _settings = settings.Value;
    }

    [HtmlAttributeName("button-text")]
    public string ButtonText { get; set; } = "Sign Out";

    [HtmlAttributeName("button-class")]
    public string? ButtonClass { get; set; }

    [HtmlAttributeName("redirect-uri")]
    public string? RedirectUri { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "noundry-oauth-logout");

        var buttonClass = ButtonClass ?? "bg-red-600 text-white hover:bg-red-700 focus:ring-red-500";
        var logoutUrl = $"{_settings.LogoutPath}?redirectUri={RedirectUri ?? _settings.DefaultRedirectUri}";

        var html = $@"
        <a href=""{logoutUrl}"" 
           class=""{buttonClass} inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors duration-200"">
            <svg class=""w-4 h-4 mr-2"" fill=""currentColor"" viewBox=""0 0 24 24"">
                <path d=""M16 17v-3H9v-4h7V7l5 5-5 5M14 2a2 2 0 0 1 2 2v2h-2V4H5v16h9v-2h2v2a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9Z""/>
            </svg>
            {ButtonText}
        </a>";

        output.Content.SetHtmlContent(html);
    }
}