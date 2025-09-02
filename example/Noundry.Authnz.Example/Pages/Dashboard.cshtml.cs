using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Noundry.Authnz.Example.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(ILogger<DashboardModel> logger)
    {
        _logger = logger;
    }

    public UserInfo? UserInfo { get; set; }

    public void OnGet()
    {
        // Extract user information from claims
        UserInfo = new UserInfo
        {
            Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            Name = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            Provider = User.FindFirst("provider")?.Value ?? "",
            AvatarUrl = User.FindFirst("avatar_url")?.Value ?? ""
        };

        _logger.LogInformation("Dashboard accessed by user {UserId}", UserInfo.Id);
    }
}

public class UserInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Provider { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
}