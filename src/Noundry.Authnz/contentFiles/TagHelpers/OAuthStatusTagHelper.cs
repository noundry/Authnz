using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Noundry.Authnz.TagHelpers;

[HtmlTargetElement("noundry-oauth-status")]
public class OAuthStatusTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OAuthStatusTagHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("show-when-authenticated")]
    public bool ShowWhenAuthenticated { get; set; } = true;

    [HtmlAttributeName("show-when-anonymous")]
    public bool ShowWhenAnonymous { get; set; } = true;

    [HtmlAttributeName("user-class")]
    public string? UserClass { get; set; }

    [HtmlAttributeName("show-avatar")]
    public bool ShowAvatar { get; set; } = true;

    [HtmlAttributeName("show-name")]
    public bool ShowName { get; set; } = true;

    [HtmlAttributeName("show-email")]
    public bool ShowEmail { get; set; } = false;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAuthenticated = user?.Identity?.IsAuthenticated == true;

        if ((isAuthenticated && !ShowWhenAuthenticated) || (!isAuthenticated && !ShowWhenAnonymous))
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "div";
        output.Attributes.SetAttribute("class", $"noundry-oauth-status {UserClass ?? ""}");

        if (isAuthenticated && user != null)
        {
            var name = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("name")?.Value ?? "";
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value ?? "";
            var avatarUrl = user.FindFirst("avatar_url")?.Value ?? user.FindFirst("picture")?.Value ?? "";

            var html = "<div class=\"flex items-center space-x-3\">";

            if (ShowAvatar && !string.IsNullOrEmpty(avatarUrl))
            {
                html += $"<img src=\"{avatarUrl}\" alt=\"User Avatar\" class=\"w-8 h-8 rounded-full\" />";
            }

            html += "<div>";

            if (ShowName && !string.IsNullOrEmpty(name))
            {
                html += $"<div class=\"text-sm font-medium text-gray-900\">{name}</div>";
            }

            if (ShowEmail && !string.IsNullOrEmpty(email))
            {
                html += $"<div class=\"text-xs text-gray-500\">{email}</div>";
            }

            html += "</div></div>";

            output.Content.SetHtmlContent(html);
        }
        else
        {
            output.Content.SetHtmlContent("<div class=\"text-sm text-gray-500\">Not signed in</div>");
        }
    }
}