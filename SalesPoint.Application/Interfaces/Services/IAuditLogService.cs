using SalesPoint.Application.DTOs.AuditLogs;
using SalesPoint.Application.DTOs.Common;

namespace SalesPoint.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task RegisterAsync(RegisterAuditLogRequest request);
    Task<PagedResponse<AuditLogDto>> SearchAsync(AuditLogSearchRequest request);
}
