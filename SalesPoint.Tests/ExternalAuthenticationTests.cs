using SalesPoint.Application.DTOs.Auth;
using SalesPoint.Application.Interfaces.Security;
using SalesPoint.Application.Services;
using Xunit;

namespace SalesPoint.Tests;

public sealed class ExternalAuthenticationTests
{
    [Fact]
    public void Status_HidesGoogleWhenClientIdIsNotConfigured()
    {
        var service = CreateService(new ExternalIdentityValidatorStub());

        var status = service.GetExternalAuthenticationStatus();

        Assert.False(status.GoogleEnabled);
        Assert.Empty(status.GoogleClientId);
    }

    [Fact]
    public async Task GoogleLogin_ReturnsClearErrorWhenProviderIsNotConfigured()
    {
        var service = CreateService(new ExternalIdentityValidatorStub());

        var result = await service.LoginWithGoogleAsync(
            new GoogleLoginRequestDto { Credential = "test-token" });

        Assert.False(result.IsSuccess);
        Assert.Contains("no está configurado", result.Message);
    }

    private static AuthService CreateService(
        IExternalIdentityValidator validator) =>
        new(null!, null!, null!, validator);

    private sealed class ExternalIdentityValidatorStub
        : IExternalIdentityValidator
    {
        public bool IsConfigured => false;
        public string ClientId => string.Empty;

        public Task<ExternalIdentity?> ValidateGoogleTokenAsync(
            string credential) =>
            Task.FromResult<ExternalIdentity?>(null);
    }
}
