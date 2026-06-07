using System.Security.Claims;
using SalesPoint.Application.Interfaces.Security;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Api.Security;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var value = GetClaim(ClaimTypes.NameIdentifier);

            if (!int.TryParse(value, out var userId) || userId <= 0)
                throw new DomainException("No se pudo identificar al vendedor autenticado.");

            return userId;
        }
    }

    public string UserName => GetClaim(ClaimTypes.Name);

    public string Role => GetClaim(ClaimTypes.Role);

    private string GetClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);

        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("La sesión del usuario no contiene la información requerida.");

        return value;
    }
}
