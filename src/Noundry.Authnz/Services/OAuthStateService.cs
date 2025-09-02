using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace Noundry.Authnz.Services;

public class OAuthStateService : IOAuthStateService
{
    private readonly IDataProtector _dataProtector;

    public OAuthStateService(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("Noundry.Authnz.OAuthState");
    }

    public string GenerateState(string provider, string? redirectUri = null)
    {
        var stateData = new OAuthStateData
        {
            Provider = provider,
            RedirectUri = redirectUri,
            Timestamp = DateTimeOffset.UtcNow,
            Nonce = Guid.NewGuid().ToString("N")
        };

        var stateJson = JsonSerializer.Serialize(stateData);
        return _dataProtector.Protect(stateJson);
    }

    public bool ValidateState(string provider, string state, out string? redirectUri)
    {
        redirectUri = null;

        try
        {
            var unprotectedState = _dataProtector.Unprotect(state);
            var stateData = JsonSerializer.Deserialize<OAuthStateData>(unprotectedState);

            if (stateData == null || stateData.Provider != provider)
            {
                return false;
            }

            // Check if state is not too old (30 minutes max)
            if (DateTimeOffset.UtcNow - stateData.Timestamp > TimeSpan.FromMinutes(30))
            {
                return false;
            }

            redirectUri = stateData.RedirectUri;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private class OAuthStateData
    {
        public string Provider { get; set; } = "";
        public string? RedirectUri { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Nonce { get; set; } = "";
    }
}