# Noundry.Authnz Example Application

This is a sample ASP.NET Core application demonstrating how to integrate and use the Noundry.Authnz OAuth library.

## Features Demonstrated

- Multiple OAuth provider login buttons
- User authentication status display
- Protected dashboard with user information
- Logout functionality
- Claims inspection

## Running the Example

1. **Configure OAuth Providers**
   
   Update `appsettings.json` with your OAuth provider credentials:
   
   ```json
   {
     "OAuth": {
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

2. **Configure OAuth Provider Redirect URIs**
   
   Set up redirect URIs in your OAuth provider console:
   - Google: `https://localhost:7234/oauth/callback/google`
   - GitHub: `https://localhost:7234/oauth/callback/github`
   - Microsoft: `https://localhost:7234/oauth/callback/microsoft`

3. **Run the Application**
   
   ```bash
   cd example/Noundry.Authnz.Example
   dotnet run
   ```

4. **Navigate to the Application**
   
   Open your browser to `https://localhost:7234`

## Pages

- **Home (`/`)** - Landing page with login options
- **Dashboard (`/Home/Dashboard`)** - Protected page showing user information
- **Privacy (`/Home/Privacy`)** - Sample privacy policy page

## OAuth Endpoints

The following endpoints are automatically configured:

- `GET /oauth/login/{provider}` - Initiate OAuth flow
- `GET /oauth/callback/{provider}` - Handle OAuth callback
- `GET /oauth/logout` - Sign out user

## TagHelpers Used

```html
<!-- Login buttons for all configured providers -->
<noundry-oauth-login show-all="true"></noundry-oauth-login>

<!-- Single provider login -->
<noundry-oauth-login provider="google" button-text="Sign in with Google"></noundry-oauth-login>

<!-- User status display -->
<noundry-oauth-status show-avatar="true" show-name="true" show-email="true"></noundry-oauth-status>

<!-- Logout button -->
<noundry-oauth-logout button-text="Sign Out"></noundry-oauth-logout>
```

## Provider Setup

### Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add redirect URI: `https://localhost:7234/oauth/callback/google`

### GitHub OAuth Setup

1. Go to GitHub Settings > Developer settings > OAuth Apps
2. Create a new OAuth App
3. Set Authorization callback URL: `https://localhost:7234/oauth/callback/github`