using SalesPoint.Application.DTOs.Common;
using SalesPoint.Application.DTOs.ErrorLogs;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Application.Interfaces.Security;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Domain.Entities;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Application.Services;

public sealed class ErrorLogService : IErrorLogService
{
    private static readonly int[] AllowedPageSizes = [10, 15, 20, 30];
    private readonly IErrorLogRepository _repository;
    private readonly ICurrentUserContext _currentUser;

    public ErrorLogService(
        IErrorLogRepository repository,
        ICurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<List<ErrorLogDto>> GetAllAsync()
    {
        return (await _repository.GetAllAsync()).Select(Map).ToList();
    }

    public async Task<ErrorLogDto> CreateAsync(CreateErrorLogRequest request)
    {
        if (request is null)
            throw new DomainException("Los datos del error son obligatorios.");

        var log = new ErrorLog(
            request.Source,
            request.Message,
            request.Detail,
            request.ExceptionType,
            GetCurrentUserIdOrNull(),
            request.HttpMethod,
            request.Path);
        await _repository.CreateAsync(log);
        return Map(log);
    }

    public async Task<ErrorLogDto> RegisterExceptionAsync(
        RegisterErrorLogRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Exception);

        var log = new ErrorLog(
            request.Source,
            request.Exception.Message,
            request.Exception.ToString(),
            request.Exception.GetType().FullName,
            request.UserId,
            request.HttpMethod,
            request.Path);

        await _repository.CreateAsync(log);
        return Map(log);
    }

    public async Task<PagedResponse<ErrorLogDto>> SearchAsync(
        ErrorLogQuery query)
    {
        query ??= new ErrorLogQuery();
        var pageNumber = Math.Clamp(query.PageNumber, 1, 100_000);
        var pageSize = AllowedPageSizes.Contains(query.PageSize)
            ? query.PageSize
            : 10;

        var logs = await _repository.SearchAsync(
            query.Field,
            query.Value,
            pageNumber,
            pageSize);
        var totalItems = await _repository.CountAsync(query.Field, query.Value);

        return new PagedResponse<ErrorLogDto>
        {
            Items = logs.Select(Map).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    private static ErrorLogDto Map(ErrorLog log)
    {
        return new ErrorLogDto
        {
            Id = log.Id,
            Source = log.Source,
            Message = log.Message,
            Detail = log.Detail,
            ExceptionType = log.ExceptionType,
            UserId = log.UserId,
            HttpMethod = log.HttpMethod,
            Path = log.Path,
            CreatedAt = log.CreatedAt
        };
    }

    private int? GetCurrentUserIdOrNull()
    {
        try
        {
            return _currentUser.UserId;
        }
        catch
        {
            return null;
        }
    }
}
