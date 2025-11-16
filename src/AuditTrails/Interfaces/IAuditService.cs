using AuditTrails.Models;
using FluentResults;

namespace AuditTrails.Interfaces;

public interface IAuditService
{
    Task<Result> SaveAuditEntryAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

