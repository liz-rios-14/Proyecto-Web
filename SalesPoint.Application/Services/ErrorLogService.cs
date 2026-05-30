using SalesPoint.Application.DTOs.ErrorLogs;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Services;
public sealed class ErrorLogService : IErrorLogService
{
    private readonly IErrorLogRepository _repository;
    public ErrorLogService(IErrorLogRepository repository) => _repository = repository;
    public async Task<List<ErrorLogDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(Map).ToList();
    public async Task<ErrorLogDto> CreateAsync(CreateErrorLogRequest request)
    {
        var log = new ErrorLog(request.Source, request.Message, request.Detail);
        await _repository.CreateAsync(log);
        return Map(log);
    }
    public async Task<ErrorLogDto> RegisterExceptionAsync(Exception exception, string source)
    {
        var log = new ErrorLog(source, exception.Message, exception.ToString());
        await _repository.CreateAsync(log);
        return Map(log);
    }
    private static ErrorLogDto Map(ErrorLog log) => new() { Id = log.Id, Source = log.Source, Message = log.Message, Detail = log.Detail, CreatedAt = log.CreatedAt };
}
