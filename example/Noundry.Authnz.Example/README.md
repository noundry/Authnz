# Noundry.Authnz Example Application

This is a modern **Razor Pages** ASP.NET Core application demonstrating how to integrate and use the Noundry.Authnz OAuth library.

## 🚀 Features Demonstrated

- **Modern Razor Pages** architecture with C# code-behind
- **Multiple OAuth provider** login buttons
- **User authentication status** display with avatars
- **Protected dashboard** with user information and claims
- **Logout functionality** with custom redirect
- **Responsive design** with Tailwind CSS

## 📁 Project Structure

```
Pages/
├── Index.cshtml + Index.cshtml.cs          # Home page with login options
├── Dashboard.cshtml + Dashboard.cshtml.cs  # Protected user dashboard  
├── Privacy.cshtml + Privacy.cshtml.cs      # Privacy policy page
├── Error.cshtml + Error.cshtml.cs          # Error handling page
└── Shared/
    ├── _Layout.cshtml                       # Main layout template
    ├── _ViewImports.cshtml                  # Global imports and TagHelpers
    └── _ViewStart.cshtml                    # Default layout assignment
```

## 🏃‍♂️ Running the Example

### 1. Configure OAuth Providers
   
Update `appsettings.json` with your OAuth provider credentials:

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
      },
      "microsoft": {
        "ClientId": "your-microsoft-client-id",
        "ClientSecret": "your-microsoft-client-secret"
      }
    }
  }
}
```

### 2. Configure OAuth Provider Redirect URIs
   
Set up redirect URIs in your OAuth provider console:
- **Google**: `https://localhost:7234/oauth/callback/google`
- **GitHub**: `https://localhost:7234/oauth/callback/github`
- **Microsoft**: `https://localhost:7234/oauth/callback/microsoft`

### 3. Run the Application
   
```bash
cd example/Noundry.Authnz.Example
dotnet run
```

### 4. Navigate to the Application
   
Open your browser to `https://localhost:7234`

## 📄 Pages

- **Home (`/`)** - Landing page with OAuth login options
- **Dashboard (`/Dashboard`)** - Protected page showing user profile and claims
- **Privacy (`/Privacy`)** - Sample privacy policy page  
- **Error (`/Error`)** - Error handling with OAuth troubleshooting tips

## 🔗 OAuth Endpoints

The following endpoints are automatically configured by the library:

- `GET /oauth/login/{provider}` - Initiate OAuth flow for specific provider
- `GET /oauth/callback/{provider}` - Handle OAuth callback from provider
- `GET /oauth/logout` - Sign out user and redirect

## 🎨 TagHelpers Used

The example demonstrates all available TagHelpers:

```html
<!-- Login buttons for all configured providers -->
<noundry-oauth-login show-all="true"></noundry-oauth-login>

<!-- Single provider login with custom styling -->
<noundry-oauth-login 
    provider="google" 
    button-text="Sign in with Google"
    button-class="custom-button-class">
</noundry-oauth-login>

<!-- User status display with avatar and name -->
<noundry-oauth-status 
    show-avatar="true" 
    show-name="true" 
    show-email="true">
</noundry-oauth-status>

<!-- Logout button with custom redirect -->
<noundry-oauth-logout 
    button-text="Sign Out"
    redirect-uri="/">
</noundry-oauth-logout>
```

## 💡 Modern Razor Pages Features

### Code-Behind Pattern

Each page uses the modern code-behind pattern:

```csharp
// Dashboard.cshtml.cs
[Authorize]
public class DashboardModel : PageModel
{
    public UserInfo? UserInfo { get; set; }

    public void OnGet()
    {
        UserInfo = new UserInfo
        {
            Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            Name = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            Provider = User.FindFirst("provider")?.Value ?? ""
        };
    }
}
```

### Authorization Attributes

Pages can be protected using attributes:

```csharp
[Authorize] // Require authentication for this page
public class DashboardModel : PageModel
```

### Dependency Injection

Services are injected via constructor:

```csharp
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
}
```

## 🔧 Provider Setup Quick Links

- **Google**: [Google Cloud Console](https://console.cloud.google.com/) → APIs & Services → Credentials
- **GitHub**: [GitHub Settings](https://github.com/settings/developers) → OAuth Apps
- **Microsoft**: [Azure Portal](https://portal.azure.com/) → Azure AD → App registrations

## 🎯 Key Learning Points

1. **Modern Architecture**: Uses Razor Pages instead of MVC controllers
2. **Clean Separation**: Logic in `.cshtml.cs` files, UI in `.cshtml` files
3. **Type Safety**: Strongly-typed page models with properties
4. **Security**: Built-in authorization attributes and user claim access
5. **Simplicity**: Less boilerplate than traditional MVC pattern

This example showcases modern ASP.NET Core development practices while demonstrating the full capabilities of the Noundry.Authnz OAuth library.