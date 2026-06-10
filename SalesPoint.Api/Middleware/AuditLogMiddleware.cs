using System.Security.Claims;
using System.Text;
using System.Text.Json;
using SalesPoint.Application.DTOs.AuditLogs;
using SalesPoint.Application.DTOs.ErrorLogs;
using SalesPoint.Application.Interfaces.Services;

namespace SalesPoint.Api.Middleware;

// AuditLog: registra acciones sin interrumpir la operación principal.
public sealed class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(
        RequestDelegate next,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var descriptor = Describe(context.Request);
        string? requestBody = null;

        if (descriptor is not null && descriptor.Value.CaptureBody)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        await _next(context);

        if (descriptor is null || context.Response.StatusCode >= 500)
            return;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
            var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

            if (descriptor.Value.Action == "LOGIN")
            {
                userName = ExtractLoginName(requestBody) ?? userName;
            }

            var action = descriptor.Value.Action == "LOGIN"
                ? context.Response.StatusCode < 400 ? "LOGIN_EXITOSO" : "LOGIN_FALLIDO"
                : descriptor.Value.Action;

            await service.RegisterAsync(new RegisterAuditLogRequest
            {
                UserId = int.TryParse(userIdValue, out var userId) ? userId : null,
                UserName = userName,
                Action = action,
                EntityName = descriptor.Value.Entity,
                EntityId = GetEntityId(context.Request.RouteValues),
                NewValues = descriptor.Value.CaptureBody ? SanitizeBody(requestBody) : null,
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                Path = context.Request.Path,
                HttpMethod = context.Request.Method
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "No se pudo registrar la auditoría.");
            await TryRegisterErrorAsync(context, exception);
        }
    }

    private static (string Action, string Entity, bool CaptureBody)? Describe(
        HttpRequest request)
    {
        var path = request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var method = request.Method.ToUpperInvariant();

        if (path == "/api/auth/login" && method == "POST")
            return ("LOGIN", "AUTH", true);
        if (path == "/api/auth/logout" && method == "POST")
            return ("LOGOUT", "AUTH", false);
        if (path.StartsWith("/api/invoices/audit/") && method == "GET")
            return ("RECONSTRUIR_FACTURA", "FACTURA", false);
        if (method is not ("POST" or "PUT" or "DELETE"))
            return null;

        if (path.StartsWith("/api/customers"))
            return (GetCrudAction(path, method, "CLIENTE"), "CLIENTE", method != "DELETE");
        if (path.StartsWith("/api/products"))
            return (GetCrudAction(path, method, "PRODUCTO"), "PRODUCTO", method != "DELETE");
        if (path.StartsWith("/api/invoices"))
            return ("CREAR_FACTURA", "FACTURA", true);
        if (path.StartsWith("/api/users"))
            return (GetCrudAction(path, method, "USUARIO"), "USUARIO", method != "DELETE");

        return null;
    }

    private static string GetCrudAction(string path, string method, string entity)
    {
        if (path.EndsWith("/activate")) return $"ACTIVAR_{entity}";
        if (path.EndsWith("/deactivate") || path.EndsWith("/disable"))
            return $"DESACTIVAR_{entity}";
        if (path.EndsWith("/unlock")) return "DESBLOQUEAR_USUARIO";
        if (method == "POST") return $"CREAR_{entity}";
        if (method == "PUT") return $"ACTUALIZAR_{entity}";
        return $"ELIMINAR_{entity}";
    }

    private static string? ExtractLoginName(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("userNameOrEmail", out var value)
                ? value.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? GetEntityId(RouteValueDictionary routeValues)
    {
        if (routeValues.TryGetValue("id", out var id))
            return id?.ToString();
        if (routeValues.TryGetValue("invoiceNumber", out var invoiceNumber))
            return invoiceNumber?.ToString();

        return null;
    }

    private static string? SanitizeBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;
        if (body.Contains("\"password\"", StringComparison.OrdinalIgnoreCase))
            return null;

        return body.Length <= 4000 ? body : body[..4000];
    }

    private async Task TryRegisterErrorAsync(HttpContext context, Exception exception)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var errorService = scope.ServiceProvider.GetRequiredService<IErrorLogService>();
            await errorService.RegisterExceptionAsync(new RegisterErrorLogRequest
            {
                Exception = exception,
                Source = "AuditLog",
                HttpMethod = context.Request.Method,
                Path = context.Request.Path
            });
        }
        catch (Exception logException)
        {
            _logger.LogError(logException, "No se pudo registrar el error de auditoría.");
        }
    }
}
