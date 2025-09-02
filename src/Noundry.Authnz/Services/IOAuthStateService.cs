namespace Noundry.Authnz.Services;

public interface IOAuthStateService
{
    string GenerateState(string provider, string? redirectUri = null);
    bool ValidateState(string provider, string state, out string? redirectUri);
}