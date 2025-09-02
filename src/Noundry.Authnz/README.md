# Noundry.Authnz

A source-only OAuth 2.0 abstraction library for ASP.NET Core applications with support for multiple identity providers including Google, Microsoft, GitHub, Apple, Facebook, and Twitter.

## Features

- 🔐 Support for major OAuth 2.0 providers (Google, Microsoft, GitHub, Apple, Facebook, Twitter)
- 🏗️ Source-only NuGet package - no runtime dependencies
- ⚙️ Configuration via `appsettings.json`
- 🎨 Ready-to-use TagHelpers with Tailwind CSS styling
- 🔒 PKCE support for enhanced security
- 🧩 Easy integration with ASP.NET Core applications
- 🎯 Multi-framework support (.NET 6, 8, and 9)

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Noundry.Authnz
```

Or via Package Manager Console:

```powershell
Install-Package Noundry.Authnz
```

## Quick Start

### 1. Configure OAuth Providers

Add OAuth configuration to your `appsettings.json`:

```json
{
  "OAuth": {
    "DefaultRedirectUri": "/dashboard",
    "LoginPath": "/oauth/login",
    "LogoutPath": "/oauth/logout",
    "RequireHttpsMetadata": true,
    "Providers": {
      "google": {
        "ClientId": "your-google-client-id",
        "ClientSecret": "your-google-client-secret"
      },
      "microsoft": {
        "ClientId": "your-microsoft-client-id",
        "ClientSecret": "your-microsoft-client-secret"
      },
      "github": {
        "ClientId": "your-github-client-id",
        "ClientSecret": "your-github-client-secret"
      }
    }
  }
}
```

### 2. Configure Services

In your `Program.cs`, add the OAuth services:

```csharp
using Noundry.Authnz.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add Noundry OAuth services
builder.Services.AddNoundryOAuth(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
app.UseRouting();

// Add Noundry OAuth middleware
app.UseNoundryOAuth();

// Map controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### 3. Add TagHelper Registration

In your `Views/_ViewImports.cshtml`, add:

```csharp
@addTagHelper *, Noundry.Authnz
```

### 4. Use OAuth Components

#### Login Buttons

Single provider login:
```html
<noundry-oauth-login provider="google" button-text="Sign in with Google"></noundry-oauth-login>
```

All configured providers:
```html
<noundry-oauth-login show-all="true"></noundry-oauth-login>
```

#### User Status Display

```html
<noundry-oauth-status show-avatar="true" show-name="true" show-email="true"></noundry-oauth-status>
```

#### Logout Button

```html
<noundry-oauth-logout button-text="Sign Out" redirect-uri="/"></noundry-oauth-logout>
```

## Advanced Configuration

### Programmatic Configuration

You can also configure OAuth providers programmatically:

```csharp
builder.Services.AddNoundryOAuth(builder.Configuration, options =>
{
    options.ConfigureOAuthProvider(OAuthProvider.Google, "client-id", "client-secret");
    options.ConfigureCustomOAuthProvider("custom", "client-id", "client-secret", 
        "https://custom.com/auth", "https://custom.com/token", "https://custom.com/user");
});
```

### Custom Provider Configuration

For custom OAuth providers not included in the defaults:

```json
{
  "OAuth": {
    "Providers": {
      "custom": {
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "AuthorizationEndpoint": "https://custom.com/oauth/authorize",
        "TokenEndpoint": "https://custom.com/oauth/token",
        "UserInfoEndpoint": "https://custom.com/oauth/userinfo",
        "Scopes": ["profile", "email"],
        "CallbackPath": "/oauth/callback",
        "UsePkce": true
      }
    }
  }
}
```

### TagHelper Customization

#### Custom Button Styling

```html
<noundry-oauth-login 
    provider="google" 
    button-text="Continue with Google"
    button-class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg"
    icon-class="fab fa-google">
</noundry-oauth-login>
```

#### Conditional Display

```html
<noundry-oauth-status 
    show-when-authenticated="true" 
    show-when-anonymous="false"
    user-class="flex items-center space-x-2">
</noundry-oauth-status>
```

## OAuth Providers

### Supported Providers

| Provider | Identifier | Default Scopes |
|----------|------------|---------------|
| Google | `google` | `openid`, `profile`, `email` |
| Microsoft | `microsoft` | `openid`, `profile`, `email` |
| GitHub | `github` | `user:email` |
| Apple | `apple` | `name`, `email` |
| Facebook | `facebook` | `email`, `public_profile` |
| Twitter | `twitter` | `tweet.read`, `users.read` |

### Provider Setup

#### Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add your redirect URI: `https://yourdomain.com/oauth/callback/google`

#### Microsoft OAuth Setup

1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to Azure Active Directory > App registrations
3. Create a new registration
4. Add redirect URI: `https://yourdomain.com/oauth/callback/microsoft`
5. Generate client secret

#### GitHub OAuth Setup

1. Go to GitHub Settings > Developer settings > OAuth Apps
2. Create a new OAuth App
3. Set Authorization callback URL: `https://yourdomain.com/oauth/callback/github`

## Security Considerations

- Always use HTTPS in production
- Store client secrets securely (use Azure Key Vault, AWS Secrets Manager, etc.)
- Enable PKCE for enhanced security (enabled by default for supported providers)
- Regularly rotate client secrets
- Validate state parameters (handled automatically)

## Styling

The library uses Tailwind CSS classes by default, following the Noundry UI design system. You can customize the appearance by:

1. Overriding the default button classes
2. Providing custom CSS classes via TagHelper attributes
3. Using custom icon classes (Font Awesome, etc.)

## Troubleshooting

### Common Issues

1. **"Provider not configured" error**
   - Ensure the provider is properly configured in `appsettings.json`
   - Check that `ClientId` and `ClientSecret` are not empty

2. **"Invalid state parameter" error**
   - Ensure sessions are properly configured
   - Check that session middleware is added before OAuth middleware

3. **Authentication not persisting**
   - Verify that cookie authentication is properly configured
   - Check that `UseAuthentication()` is called before `UseAuthorization()`

### Logging

Enable detailed logging by adding to `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Noundry.Authnz": "Debug"
    }
  }
}
```

## API Reference

### Services

#### IOAuthService

```csharp
public interface IOAuthService
{
    string GenerateAuthorizationUrl(string provider, string? state = null, string? redirectUri = null);
    Task<OAuthUserInfo?> HandleCallbackAsync(string provider, string code, string? state = null);
    Task<string?> ExchangeCodeForTokenAsync(string provider, string code);
    Task<OAuthUserInfo?> GetUserInfoAsync(string provider, string accessToken);
    bool IsProviderConfigured(string provider);
    IEnumerable<string> GetConfiguredProviders();
}
```

### Models

#### OAuthUserInfo

```csharp
public class OAuthUserInfo
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string AvatarUrl { get; set; }
    public string Provider { get; set; }
    public Dictionary<string, object> AdditionalClaims { get; set; }
}
```

### Controllers

The library automatically registers an `OAuthController` with the following endpoints:

- `GET /oauth/login/{provider}` - Initiates OAuth flow
- `GET /oauth/callback/{provider}` - Handles OAuth callback
- `GET /oauth/logout` - Signs out the user

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues and questions, please visit the [GitHub repository](https://github.com/noundry/authnz).