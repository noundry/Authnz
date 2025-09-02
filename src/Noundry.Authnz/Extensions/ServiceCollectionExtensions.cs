using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Noundry.Authnz.Models;
using Noundry.Authnz.Services;

namespace Noundry.Authnz.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNoundryOAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OAuthSettings>(configuration.GetSection(OAuthSettings.SectionName));
        
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/oauth/login";
                options.LogoutPath = "/oauth/logout";
                options.AccessDeniedPath = "/oauth/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.Cookie.Name = "NoundryAuth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });

        services.AddHttpClient<IOAuthService, OAuthService>();
        services.AddScoped<IOAuthService, OAuthService>();
        services.AddScoped<IOAuthStateService, OAuthStateService>();

        services.AddDataProtection();
        services.AddHttpContextAccessor();
        
        return services;
    }

    public static IServiceCollection AddNoundryOAuth(this IServiceCollection services, IConfiguration configuration, Action<OAuthSettings> configureOptions)
    {
        services.AddNoundryOAuth(configuration);
        services.Configure(configureOptions);
        return services;
    }
}