using SalesPoint.Application.DTOs.AuditLogs;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Application.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<List<AuditLog>> SearchAsync(AuditLogSearchRequest request);
    Task<int> CountAsync(AuditLogSearchRequest request);
}
