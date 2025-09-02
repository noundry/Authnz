using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Noundry.Authnz.Controllers;
using Noundry.Authnz.Models;
using Noundry.Authnz.Services;
using NUnit.Framework;

namespace Noundry.Authnz.Tests.Controllers;

[TestFixture]
public class OAuthControllerTests
{
    private Mock<IOAuthService> _mockOAuthService;
    private Mock<IOAuthStateService> _mockStateService;
    private Mock<ILogger<OAuthController>> _mockLogger;
    private OAuthSettings _settings;
    private OAuthController _controller = null!;
    private Mock<HttpContext> _mockHttpContext;
    private Mock<IAuthenticationService> _mockAuthService;
    private Mock<IServiceProvider> _mockServiceProvider;

    [SetUp]
    public void Setup()
    {
        _mockOAuthService = new Mock<IOAuthService>();
        _mockStateService = new Mock<IOAuthStateService>();
        _mockLogger = new Mock<ILogger<OAuthController>>();
        _settings = new OAuthSettings
        {
            DefaultRedirectUri = "/dashboard",
            LoginPath = "/oauth/login",
            LogoutPath = "/oauth/logout"
        };

        var options = Options.Create(_settings);
        _controller = new OAuthController(_mockOAuthService.Object, _mockStateService.Object, options, _mockLogger.Object);

        _mockHttpContext = new Mock<HttpContext>();
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        
        _mockHttpContext.Setup(x => x.RequestServices).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(_mockAuthService.Object);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public void Login_ConfiguredProvider_RedirectsToAuthUrl()
    {
        _mockOAuthService.Setup(x => x.IsProviderConfigured("google")).Returns(true);
        _mockStateService.Setup(x => x.GenerateState("google", "/dashboard")).Returns("test-state");
        _mockOAuthService.Setup(x => x.GenerateAuthorizationUrl("google", "test-state", null))
            .Returns("https://accounts.google.com/oauth/authorize?client_id=test");

        var result = _controller.Login("google") as RedirectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Url, Is.EqualTo("https://accounts.google.com/oauth/authorize?client_id=test"));
    }

    [Test]
    public void Login_UnconfiguredProvider_ReturnsBadRequest()
    {
        _mockOAuthService.Setup(x => x.IsProviderConfigured("invalid")).Returns(false);

        var result = _controller.Login("invalid") as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo("Provider 'invalid' is not configured"));
    }

    [Test]
    public async Task Callback_WithError_RedirectsWithError()
    {
        var result = await _controller.Callback("google", error: "access_denied");

        Assert.That(result, Is.InstanceOf<RedirectResult>());
        var redirectResult = result as RedirectResult;
        Assert.That(redirectResult!.Url, Does.Contain("error=oauth_error"));
    }

    [Test]
    public async Task Callback_MissingCode_ReturnsBadRequest()
    {
        var result = await _controller.Callback("google");

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult!.Value, Is.EqualTo("Authorization code is required"));
    }

    [Test]
    public async Task Callback_ValidCode_RedirectsToDefaultUri()
    {
        var userInfo = new OAuthUserInfo
        {
            Id = "123",
            Name = "Test User",
            Email = "test@example.com",
            Provider = "google"
        };

        var testState = "valid-state";
        var redirectUri = "/dashboard";
        _mockStateService.Setup(x => x.ValidateState("google", testState, out redirectUri))
            .Returns(true);
        _mockOAuthService.Setup(x => x.HandleCallbackAsync("google", "test-code", testState))
            .ReturnsAsync(userInfo);

        var result = await _controller.Callback("google", "test-code", "valid-state");

        Assert.That(result, Is.InstanceOf<RedirectResult>());
    }

    [Test]
    public async Task Logout_RedirectsToDefaultUri()
    {
        var result = await _controller.Logout();

        Assert.That(result, Is.InstanceOf<RedirectResult>());
        var redirectResult = result as RedirectResult;
        Assert.That(redirectResult!.Url, Is.EqualTo(_settings.DefaultRedirectUri));
    }

    [Test]
    public async Task Logout_WithCustomRedirectUri_RedirectsToCustomUri()
    {
        var customUri = "/custom";
        var result = await _controller.Logout(customUri);

        Assert.That(result, Is.InstanceOf<RedirectResult>());
        var redirectResult = result as RedirectResult;
        Assert.That(redirectResult!.Url, Is.EqualTo(customUri));
    }
}