using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IErrorLogRepository
{
    Task<ErrorLog> CreateAsync(ErrorLog errorLog);
    Task<List<ErrorLog>> GetAllAsync();
}
