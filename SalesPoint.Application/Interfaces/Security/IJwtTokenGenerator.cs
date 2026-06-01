using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Security;

public interface IJwtTokenGenerator
{
    string Generate(User user, string roleName, out DateTime expiration);
}
