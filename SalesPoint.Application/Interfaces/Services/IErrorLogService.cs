using SalesPoint.Application.DTOs.ErrorLogs;
namespace SalesPoint.Application.Interfaces.Services;
public interface IErrorLogService
{
    Task<List<ErrorLogDto>> GetAllAsync();
    Task<ErrorLogDto> CreateAsync(CreateErrorLogRequest request);
    Task<ErrorLogDto> RegisterExceptionAsync(Exception exception, string source);
}
