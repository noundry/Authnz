using Microsoft.AspNetCore.Builder;

namespace Noundry.Authnz.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseNoundryOAuth(this IApplicationBuilder app)
    {
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        
        return app;
    }
}