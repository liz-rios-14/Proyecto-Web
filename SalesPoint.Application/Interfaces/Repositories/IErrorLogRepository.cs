using SalesPoint.Domain.Entities;
namespace SalesPoint.Application.Interfaces.Repositories;
public interface IErrorLogRepository
{
    Task<List<ErrorLog>> GetAllAsync();
    Task<ErrorLog> CreateAsync(ErrorLog errorLog);
}
