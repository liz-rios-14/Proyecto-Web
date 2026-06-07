using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SalesPoint.Application.DTOs.ErrorLogs;
using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Domain.Exceptions;

namespace SalesPoint.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException exception)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                exception.Message);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogWarning(exception, "No se pudo guardar la información en la base de datos.");
            await TryRegisterErrorAsync(context, exception);

            await WriteErrorAsync(
                context,
                StatusCodes.Status409Conflict,
                "No se pudo guardar la información porque los datos entran en conflicto con un registro existente.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error no controlado procesando {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
            await TryRegisterErrorAsync(context, exception);

            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Ocurrió un error interno. Intente nuevamente.");
        }
    }

    private async Task TryRegisterErrorAsync(
        HttpContext context,
        Exception exception)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider
                .GetRequiredService<IErrorLogService>();
            var userIdValue = context.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            await service.RegisterExceptionAsync(new RegisterErrorLogRequest
            {
                Exception = exception,
                Source = $"{context.Request.Method} {context.Request.Path}",
                UserId = int.TryParse(userIdValue, out var userId)
                    ? userId
                    : null,
                HttpMethod = context.Request.Method,
                Path = context.Request.Path
            });
        }
        catch (Exception logException)
        {
            _logger.LogError(
                logException,
                "No se pudo registrar el error técnico en la base de datos.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new { message });
    }
}
