using SalesPoint.Application.DTOs.ErrorLogs;
using SalesPoint.Application.DTOs.Common;
namespace SalesPoint.Application.Interfaces.Services;
public interface IErrorLogService
{
    Task<List<ErrorLogDto>> GetAllAsync();
    Task<ErrorLogDto> CreateAsync(CreateErrorLogRequest request);
    Task<ErrorLogDto> RegisterExceptionAsync(RegisterErrorLogRequest request);
    Task<PagedResponse<ErrorLogDto>> SearchAsync(ErrorLogQuery query);
}
