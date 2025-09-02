using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Noundry.Authnz.Example.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public IActionResult Dashboard()
    {
        var userInfo = new
        {
            Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Name = User.FindFirst(ClaimTypes.Name)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Provider = User.FindFirst("provider")?.Value,
            AvatarUrl = User.FindFirst("avatar_url")?.Value
        };

        return View(userInfo);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}