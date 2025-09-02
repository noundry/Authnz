# Noundry.Authnz

A comprehensive OAuth 2.0 abstraction library for ASP.NET Core applications with built-in support for multiple identity providers including Google, Microsoft, GitHub, Apple, Facebook, and Twitter.

[![.NET](https://img.shields.io/badge/.NET-6%20%7C%208%20%7C%209-blue)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Noundry.Authnz)](https://www.nuget.org/packages/Noundry.Authnz)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## ✨ Features

- 🔐 **Multiple OAuth Providers**: Google, Microsoft, GitHub, Apple, Facebook, Twitter + Custom providers
- 🏗️ **Binary NuGet Package**: Easy integration with standard project references
- ⚙️ **JSON Configuration**: Simple setup via `appsettings.json`
- 🎨 **TagHelper Components**: Pre-built UI components with Tailwind CSS styling
- 🔒 **Enhanced Security**: PKCE support, state validation, secure cookies
- 🧩 **Modern ASP.NET Core**: Works with Razor Pages, MVC, and Web APIs
- 🎯 **Multi-Framework Support**: .NET 6, 8, and 9
- 📱 **Responsive Design**: Mobile-first UI components

## 🚀 Quick Start

### Step 1: Install the Package

```bash
# .NET CLI
dotnet add package Noundry.Authnz

# Package Manager Console
Install-Package Noundry.Authnz
```

### Step 2: Configure OAuth Providers

Add to your `appsettings.json`:

```json
{
  "OAuth": {
    "DefaultRedirectUri": "/Dashboard",
    "Providers": {
      "google": {
        "ClientId": "your-google-client-id.apps.googleusercontent.com",
        "ClientSecret": "your-google-client-secret"
      },
      "github": {
        "ClientId": "your-github-client-id",
        "ClientSecret": "your-github-client-secret"
      }
    }
  }
}
```

### Step 3: Register Services

In `Program.cs`:

```csharp
using Noundry.Authnz.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// 🔥 Add Noundry OAuth services
builder.Services.AddNoundryOAuth(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 🔥 Add OAuth middleware
app.UseNoundryOAuth();

// Map Razor Pages
app.MapRazorPages();

app.Run();
```

### Step 4: Add TagHelper Support

In `Pages/_ViewImports.cshtml`:

```csharp
@using YourApp.Pages
@namespace YourApp.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, Noundry.Authnz
```

### Step 5: Create Your First Razor Page

Create `Pages/Index.cshtml`:

```html
@page
@model IndexModel
@{
    ViewData["Title"] = "Home";
}

<!-- Login page with all configured providers -->
<div class="max-w-md mx-auto mt-8">
    <div class="bg-white shadow-md rounded-lg p-6">
        <h2 class="text-2xl font-bold text-center mb-6">Sign In</h2>
        
        <!-- Shows login buttons for all configured providers -->
        <noundry-oauth-login show-all="true"></noundry-oauth-login>
    </div>
</div>
```

And `Pages/Index.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        // Page logic here
    }
}
```

That's it! 🎉 Your OAuth authentication is now working.

## 📋 Complete Examples

### Example 1: Simple Login Page

Create `Pages/Login.cshtml`:

```html
@page
@model LoginModel
@{
    ViewData["Title"] = "Sign In";
}

<div class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="max-w-md w-full space-y-8">
        <div class="text-center">
            <h2 class="text-3xl font-bold text-gray-900">Sign in to your account</h2>
            <p class="mt-2 text-sm text-gray-600">Choose your preferred sign-in method</p>
        </div>
        
        <div class="space-y-3">
            <!-- All configured providers will show automatically -->
            <noundry-oauth-login show-all="true"></noundry-oauth-login>
        </div>
        
        <div class="text-center text-xs text-gray-500">
            By signing in, you agree to our Terms of Service and Privacy Policy
        </div>
    </div>
</div>
```

### Example 2: Dashboard with User Info

And `Pages/Login.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    public void OnGet()
    {
        // Redirect if already authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            Response.Redirect("/Dashboard");
        }
    }
}
```

Create `Pages/Dashboard.cshtml`:

```html
@page
@model DashboardModel
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize]
@{
    ViewData["Title"] = "Dashboard";
}

<div class="max-w-4xl mx-auto py-6">
    <!-- Welcome Section -->
    <div class="bg-white shadow rounded-lg p-6 mb-6">
        <div class="flex items-center justify-between">
            <div>
                <h1 class="text-2xl font-bold text-gray-900">Welcome back!</h1>
                <noundry-oauth-status 
                    show-name="true" 
                    show-email="true" 
                    user-class="mt-2 text-gray-600">
                </noundry-oauth-status>
            </div>
            <noundry-oauth-status 
                show-avatar="true" 
                show-when-authenticated="true">
            </noundry-oauth-status>
        </div>
    </div>
    
    <!-- User Claims Information -->
    <div class="bg-white shadow rounded-lg p-6">
        <h2 class="text-lg font-medium text-gray-900 mb-4">Your Profile Information</h2>
        <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                    <tr>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Claim</th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Value</th>
                    </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                    @foreach (var claim in User.Claims)
                    {
                        <tr>
                            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                                @claim.Type
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                @claim.Value
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>
```

And `Pages/Dashboard.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

[Authorize] // Require authentication for this page
public class DashboardModel : PageModel
{
    public UserInfo? UserInfo { get; set; }

    public void OnGet()
    {
        // Get user information from claims
        UserInfo = new UserInfo
        {
            Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            Name = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            Provider = User.FindFirst("provider")?.Value ?? ""
        };
    }
}

public class UserInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Provider { get; set; } = "";
}
```

### Example 3: Form Handling with Razor Pages

Create `Pages/Profile.cshtml`:

```html
@page
@model ProfileModel
@attribute [Authorize]
@{
    ViewData["Title"] = "Edit Profile";
}

<div class="max-w-2xl mx-auto">
    <div class="bg-white shadow rounded-lg p-6">
        <h1 class="text-2xl font-bold mb-6">Edit Profile</h1>
        
        <form method="post">
            <div class="space-y-4">
                <div>
                    <label asp-for="DisplayName" class="block text-sm font-medium text-gray-700"></label>
                    <input asp-for="DisplayName" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm" />
                    <span asp-validation-for="DisplayName" class="text-red-500 text-sm"></span>
                </div>
                
                <div>
                    <label asp-for="Email" class="block text-sm font-medium text-gray-700"></label>
                    <input asp-for="Email" readonly class="mt-1 block w-full rounded-md border-gray-300 bg-gray-50" />
                </div>
                
                <div class="flex space-x-4">
                    <button type="submit" class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
                        Update Profile
                    </button>
                    <a href="/Dashboard" class="bg-gray-600 text-white px-4 py-2 rounded hover:bg-gray-700">
                        Cancel
                    </a>
                </div>
            </div>
        </form>
    </div>
</div>
```

And `Pages/Profile.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

[Authorize]
public class ProfileModel : PageModel
{
    [BindProperty]
    [Required]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = "";
    
    [BindProperty]
    public string Email { get; set; } = "";

    public void OnGet()
    {
        // Load current user data
        DisplayName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        // Handle profile update logic here
        TempData["Message"] = "Profile updated successfully!";
        
        return RedirectToPage("/Dashboard");
    }
}
```

## 🔧 OAuth Provider Setup

### Google OAuth Setup

1. **Go to [Google Cloud Console](https://console.cloud.google.com/)**
2. **Create/Select Project** → Enable "Google+ API"
3. **Credentials** → Create "OAuth 2.0 Client ID"
4. **Application Type** → Web Application
5. **Authorized Redirect URIs** → Add:
   ```
   https://localhost:7234/oauth/callback/google
   https://yourdomain.com/oauth/callback/google
   ```
6. **Copy Client ID & Secret** to `appsettings.json`

### Microsoft OAuth Setup

1. **Go to [Azure Portal](https://portal.azure.com/)**
2. **Azure Active Directory** → App registrations → New registration
3. **Redirect URI** → Web → Add:
   ```
   https://localhost:7234/oauth/callback/microsoft
   https://yourdomain.com/oauth/callback/microsoft
   ```
4. **Certificates & Secrets** → New client secret
5. **Copy Application ID & Secret** to `appsettings.json`

### GitHub OAuth Setup

1. **Go to [GitHub Settings](https://github.com/settings/developers)**
2. **Developer settings** → OAuth Apps → New OAuth App
3. **Authorization callback URL**:
   ```
   https://localhost:7234/oauth/callback/github
   https://yourdomain.com/oauth/callback/github
   ```
4. **Copy Client ID & Secret** to `appsettings.json`

## ⚙️ Advanced Configuration

### Custom Button Styling

```html
<!-- Custom Google login button -->
<noundry-oauth-login 
    provider="google" 
    button-text="Continue with Google"
    button-class="bg-red-500 hover:bg-red-600 text-white px-6 py-3 rounded-full font-semibold shadow-lg transform hover:scale-105 transition-all duration-200"
    icon-class="fab fa-google text-xl">
</noundry-oauth-login>

<!-- Minimal GitHub button -->
<noundry-oauth-login 
    provider="github" 
    button-text="GitHub"
    button-class="bg-gray-800 hover:bg-gray-900 text-white px-4 py-2 rounded text-sm"
    icon-class="fab fa-github">
</noundry-oauth-login>
```

### Programmatic Configuration

```csharp
builder.Services.AddNoundryOAuth(builder.Configuration, options =>
{
    // Configure known providers
    options.ConfigureOAuthProvider(OAuthProvider.Google, "client-id", "client-secret");
    options.ConfigureOAuthProvider(OAuthProvider.Microsoft, "client-id", "client-secret");
    
    // Configure custom provider
    options.ConfigureCustomOAuthProvider(
        providerName: "discord",
        clientId: "your-discord-client-id",
        clientSecret: "your-discord-client-secret", 
        authorizationEndpoint: "https://discord.com/oauth2/authorize",
        tokenEndpoint: "https://discord.com/api/oauth2/token",
        userInfoEndpoint: "https://discord.com/api/users/@me",
        scopes: new List<string> { "identify", "email" }
    );
});
```

### Custom OAuth Provider

Add to `appsettings.json`:

```json
{
  "OAuth": {
    "Providers": {
      "discord": {
        "ClientId": "your-discord-client-id",
        "ClientSecret": "your-discord-client-secret",
        "AuthorizationEndpoint": "https://discord.com/oauth2/authorize",
        "TokenEndpoint": "https://discord.com/api/oauth2/token", 
        "UserInfoEndpoint": "https://discord.com/api/users/@me",
        "Scopes": ["identify", "email"],
        "UsePkce": true
      },
      "linkedin": {
        "ClientId": "your-linkedin-client-id",
        "ClientSecret": "your-linkedin-client-secret",
        "AuthorizationEndpoint": "https://www.linkedin.com/oauth/v2/authorization",
        "TokenEndpoint": "https://www.linkedin.com/oauth/v2/accessToken",
        "UserInfoEndpoint": "https://api.linkedin.com/v2/people/~",
        "Scopes": ["r_liteprofile", "r_emailaddress"]
      }
    }
  }
}
```

### Environment-Specific Configuration

**appsettings.Development.json:**
```json
{
  "OAuth": {
    "RequireHttpsMetadata": false,
    "Providers": {
      "google": {
        "ClientId": "dev-google-client-id",
        "ClientSecret": "dev-google-client-secret"
      }
    }
  }
}
```

**appsettings.Production.json:**
```json
{
  "OAuth": {
    "RequireHttpsMetadata": true,
    "Providers": {
      "google": {
        "ClientId": "prod-google-client-id",
        "ClientSecret": "prod-google-client-secret"
      }
    }
  }
}
```

## 🎨 TagHelper Reference

### `<noundry-oauth-login>`

Renders OAuth provider login buttons.

**Attributes:**
- `provider` (string): Specific provider to show button for
- `show-all` (bool): Show buttons for all configured providers
- `button-text` (string): Custom button text
- `button-class` (string): Custom CSS classes for button
- `icon-class` (string): Custom CSS classes for icon
- `redirect-uri` (string): Custom redirect URI after login

**Examples:**
```html
<!-- Single provider -->
<noundry-oauth-login provider="google"></noundry-oauth-login>

<!-- All providers -->
<noundry-oauth-login show-all="true"></noundry-oauth-login>

<!-- Custom styling -->
<noundry-oauth-login 
    provider="github"
    button-text="Login with GitHub" 
    button-class="btn btn-dark"
    icon-class="fab fa-github">
</noundry-oauth-login>
```

### `<noundry-oauth-status>`

Displays user authentication status and information.

**Attributes:**
- `show-when-authenticated` (bool): Show only when user is logged in
- `show-when-anonymous` (bool): Show only when user is not logged in  
- `show-avatar` (bool): Display user avatar image
- `show-name` (bool): Display user name
- `show-email` (bool): Display user email
- `user-class` (string): Custom CSS classes for the container

**Examples:**
```html
<!-- Basic user info -->
<noundry-oauth-status show-name="true" show-email="true"></noundry-oauth-status>

<!-- Avatar only -->
<noundry-oauth-status show-avatar="true" show-when-authenticated="true"></noundry-oauth-status>

<!-- Custom styling -->
<noundry-oauth-status 
    show-avatar="true" 
    show-name="true"
    user-class="flex items-center space-x-3 p-4 bg-gray-100 rounded-lg">
</noundry-oauth-status>
```

### `<noundry-oauth-logout>`

Renders a logout button.

**Attributes:**
- `button-text` (string): Text for the logout button
- `button-class` (string): Custom CSS classes for button
- `redirect-uri` (string): Where to redirect after logout

**Examples:**
```html
<!-- Basic logout -->
<noundry-oauth-logout></noundry-oauth-logout>

<!-- Custom logout -->
<noundry-oauth-logout 
    button-text="Sign Out" 
    button-class="btn btn-outline-danger btn-sm"
    redirect-uri="/goodbye">
</noundry-oauth-logout>
```

## 🔒 Security Best Practices

### Production Checklist

- ✅ **Use HTTPS**: Always use SSL/TLS in production
- ✅ **Secure Secrets**: Store client secrets in Azure Key Vault, AWS Secrets Manager, or environment variables
- ✅ **Regular Rotation**: Rotate OAuth client secrets regularly
- ✅ **Scope Minimization**: Request only necessary OAuth scopes
- ✅ **PKCE Enabled**: Use Proof Key for Code Exchange (enabled by default)
- ✅ **State Validation**: Validate state parameters (handled automatically)
- ✅ **Secure Cookies**: Use secure, HTTP-only cookies

### Environment Variables

Instead of storing secrets in `appsettings.json`:

```bash
# Set environment variables
export OAuth__Providers__google__ClientSecret="your-secret-here"
export OAuth__Providers__github__ClientSecret="your-secret-here"
```

Or use User Secrets in development:
```bash
dotnet user-secrets set "OAuth:Providers:google:ClientSecret" "your-secret-here"
```

### Azure Key Vault Integration

```csharp
builder.Configuration.AddAzureKeyVault(/* key vault config */);

// Secrets will be automatically loaded:
// OAuth:Providers:google:ClientSecret -> OAuth--Providers--google--ClientSecret
```

## 🐛 Troubleshooting

### Common Issues & Solutions

**❌ "Provider 'google' is not configured"**
```json
// ✅ Ensure ClientId and ClientSecret are set
{
  "OAuth": {
    "Providers": {
      "google": {
        "ClientId": "must-not-be-empty",
        "ClientSecret": "must-not-be-empty"
      }
    }
  }
}
```

**❌ "Invalid state parameter"**
```csharp
// ✅ Ensure session is configured BEFORE OAuth middleware
builder.Services.AddSession(); // Add this
app.UseSession(); // Before UseNoundryOAuth()
app.UseNoundryOAuth();
```

**❌ Authentication not persisting**
```csharp
// ✅ Correct middleware order
app.UseRouting();
app.UseSession();        // 1. Session first
app.UseAuthentication(); // 2. Then authentication  
app.UseAuthorization();  // 3. Then authorization
```

**❌ HTTPS redirect errors in development**
```json
// ✅ Disable HTTPS requirement in development
{
  "OAuth": {
    "RequireHttpsMetadata": false  // Only for development!
  }
}
```

### Enable Debug Logging

Add to `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Noundry.Authnz": "Debug",
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

### Test OAuth Flow

1. **Check Provider Configuration**: Verify redirect URIs match exactly
2. **Test Authorization URL**: Visit `/oauth/login/{provider}` directly
3. **Check Browser Network**: Look for 400/401/403 responses
4. **Verify Scopes**: Ensure requested scopes are allowed by provider
5. **Test Callback**: Check `/oauth/callback/{provider}` receives parameters

## 📚 API Reference

### IOAuthService Interface

```csharp
public interface IOAuthService
{
    // Generate OAuth authorization URL
    string GenerateAuthorizationUrl(string provider, string? state = null, string? redirectUri = null);
    
    // Handle OAuth callback and return user info
    Task<OAuthUserInfo?> HandleCallbackAsync(string provider, string code, string? state = null);
    
    // Exchange authorization code for access token
    Task<string?> ExchangeCodeForTokenAsync(string provider, string code);
    
    // Get user information using access token
    Task<OAuthUserInfo?> GetUserInfoAsync(string provider, string accessToken);
    
    // Check if provider is properly configured
    bool IsProviderConfigured(string provider);
    
    // Get list of all configured providers
    IEnumerable<string> GetConfiguredProviders();
}
```

### OAuthUserInfo Model

```csharp
public class OAuthUserInfo
{
    public string Id { get; set; }              // Provider user ID
    public string Email { get; set; }           // User email
    public string Name { get; set; }            // Full name
    public string FirstName { get; set; }       // First name
    public string LastName { get; set; }        // Last name  
    public string AvatarUrl { get; set; }       // Profile picture URL
    public string Provider { get; set; }        // OAuth provider name
    public Dictionary<string, object> AdditionalClaims { get; set; } // Extra claims
}
```

### Available Endpoints

The library automatically registers these endpoints:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/oauth/login/{provider}` | GET | Initiates OAuth flow for provider |
| `/oauth/callback/{provider}` | GET | Handles OAuth callback from provider |
| `/oauth/logout` | GET | Signs out user and redirects |

## 🤝 Contributing

We welcome contributions! Here's how to get started:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Make your changes** and add tests
4. **Run tests**: `dotnet test`
5. **Commit changes**: `git commit -m 'Add amazing feature'`
6. **Push to branch**: `git push origin feature/amazing-feature`
7. **Open a Pull Request**

### Development Setup

```bash
# Clone the repo
git clone https://github.com/noundry/Authnz.git
cd Authnz

# Build the solution
dotnet build

# Run tests
dotnet test

# Run example app
cd example/Noundry.Authnz.Example
dotnet run
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/noundry/Authnz/blob/master/LICENSE) file for details.

## 🙋‍♂️ Support & Community

- 📖 **Documentation**: [GitHub Wiki](https://github.com/noundry/Authnz/wiki)
- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/noundry/Authnz/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/noundry/Authnz/discussions)
- 📦 **NuGet Package**: [Noundry.Authnz](https://www.nuget.org/packages/Noundry.Authnz)

---

**Made with ❤️ by the Noundry team**