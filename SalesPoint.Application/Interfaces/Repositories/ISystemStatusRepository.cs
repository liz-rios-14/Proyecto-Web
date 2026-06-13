namespace SalesPoint.Application.Interfaces.Repositories;

public interface ISystemStatusRepository
{
    Task<bool> CanConnectToDatabaseAsync();
}
