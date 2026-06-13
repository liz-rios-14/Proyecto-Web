namespace SalesPoint.Application.Interfaces.Security;

public interface IExternalIdentityValidator
{
    bool IsConfigured { get; }
    string ClientId { get; }
    Task<ExternalIdentity?> ValidateGoogleTokenAsync(string credential);
}

public sealed record ExternalIdentity(
    string Provider,
    string Subject,
    string Email,
    bool EmailVerified);
