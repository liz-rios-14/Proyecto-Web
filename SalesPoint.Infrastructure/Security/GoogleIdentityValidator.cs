using Google.Apis.Auth;
using SalesPoint.Application.Interfaces.Security;

namespace SalesPoint.Infrastructure.Security;

public sealed class GoogleIdentityValidator : IExternalIdentityValidator
{
    private readonly string _clientId;

    public GoogleIdentityValidator(string? clientId)
    {
        _clientId = clientId?.Trim() ?? string.Empty;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_clientId);
    public string ClientId => _clientId;

    public async Task<ExternalIdentity?> ValidateGoogleTokenAsync(
        string credential)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(credential))
            return null;

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                credential,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_clientId]
                });

            return new ExternalIdentity(
                "GOOGLE",
                payload.Subject,
                payload.Email ?? string.Empty,
                payload.EmailVerified);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
