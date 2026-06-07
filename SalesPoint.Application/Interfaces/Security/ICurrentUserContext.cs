namespace SalesPoint.Application.Interfaces.Security;

public interface ICurrentUserContext
{
    int UserId { get; }
    string UserName { get; }
    string Role { get; }
}
