using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Noundry.Authnz.Services;
using NUnit.Framework;

namespace Noundry.Authnz.Tests.Services;

[TestFixture]
public class OAuthStateServiceTests
{
    private IOAuthStateService _stateService = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        var serviceProvider = services.BuildServiceProvider();
        
        var dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
        _stateService = new OAuthStateService(dataProtectionProvider);
    }

    [Test]
    public void GenerateState_ValidProvider_ReturnsNonEmptyString()
    {
        var result = _stateService.GenerateState("google", "/dashboard");

        Assert.That(result, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void ValidateState_ValidState_ReturnsTrue()
    {
        var state = _stateService.GenerateState("google", "/dashboard");

        var isValid = _stateService.ValidateState("google", state, out var redirectUri);

        Assert.That(isValid, Is.True);
        Assert.That(redirectUri, Is.EqualTo("/dashboard"));
    }

    [Test]
    public void ValidateState_DifferentProvider_ReturnsFalse()
    {
        var state = _stateService.GenerateState("google", "/dashboard");

        var isValid = _stateService.ValidateState("github", state, out var redirectUri);

        Assert.That(isValid, Is.False);
        Assert.That(redirectUri, Is.Null);
    }

    [Test]
    public void ValidateState_InvalidState_ReturnsFalse()
    {
        var isValid = _stateService.ValidateState("google", "invalid-state", out var redirectUri);

        Assert.That(isValid, Is.False);
        Assert.That(redirectUri, Is.Null);
    }

    [Test]
    public void ValidateState_EmptyState_ReturnsFalse()
    {
        var isValid = _stateService.ValidateState("google", "", out var redirectUri);

        Assert.That(isValid, Is.False);
        Assert.That(redirectUri, Is.Null);
    }

    [Test]
    public void ValidateState_NullState_ReturnsFalse()
    {
        var isValid = _stateService.ValidateState("google", null!, out var redirectUri);

        Assert.That(isValid, Is.False);
        Assert.That(redirectUri, Is.Null);
    }
}