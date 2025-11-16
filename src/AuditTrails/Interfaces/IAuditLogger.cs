using AuditTrails.Enums;
using AuditTrails.Models;
using FluentResults;

namespace AuditTrails.Interfaces;

public interface IAuditLogger
{
    Result<AuditEntry> CreateAuditEntry(AuditOperation operation, string description, string? userId = null);
    Result<AuditEntry> CreateEntityAuditEntry(AuditOperation operation, string entityType, string entityId, string description, string? userId = null);
    Result<AuditEntry> CreateChangeAuditEntry<T>(AuditOperation operation, string entityType, string entityId, T? oldValues, T? newValues, string? userId = null);
    Result<AuditEntry> CreateErrorAuditEntry(AuditOperation operation, string description, Exception exception, string? userId = null);
}
