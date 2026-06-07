using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IErrorLogRepository
{
    Task<ErrorLog> CreateAsync(ErrorLog errorLog);
    Task<List<ErrorLog>> GetAllAsync();
    Task<List<ErrorLog>> SearchAsync(
        string field,
        string value,
        int pageNumber,
        int pageSize);
    Task<int> CountAsync(string field, string value);
}
