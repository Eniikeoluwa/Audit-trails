using System.Text.Json;
using AuditTrails.Enums;
using AuditTrails.Interfaces;
using AuditTrails.Models;
using FluentResults;
using Microsoft.Extensions.Options;

namespace AuditTrails.Services;

public class AuditLogger : IAuditLogger
{
    private readonly AuditOptions _options;
    public AuditLogger(IOptions<AuditOptions> options)
    {
        _options = options.Value;
    }
    
    public Result<AuditEntry> CreateAuditEntry(AuditOperation operation, string description, string? userId = null)
    {
        try
        {
            var entry = new AuditEntry
            {
                Operation = operation,
                Description = TruncateDescription(description),
                UserId = userId,
                Severity = GetSeverity(operation)
            };
            
            return Result.Ok(entry);
        }
        catch (Exception ex)
        {
            return Result.Fail<AuditEntry>($"Failed to create audit entry: {ex.Message}");
        }
    }
    
    public Result<AuditEntry> CreateEntityAuditEntry(AuditOperation operation, string entityType, string entityId, 
        string description, string? userId = null)
    {
        try
        {
            var entry = new AuditEntry
            {
                Operation = operation,
                EntityType = entityType,
                EntityId = entityId,
                Description = TruncateDescription(description),
                UserId = userId,
                Severity = GetSeverity(operation)
            };
            
            return Result.Ok(entry);
        }
        catch (Exception ex)
        {
            return Result.Fail<AuditEntry>($"Failed to create entity audit entry: {ex.Message}");
        }
    }
    
    public Result<AuditEntry> CreateChangeAuditEntry<T>(AuditOperation operation, string entityType, string entityId, 
        T? oldValues, T? newValues, string? userId = null)
    {
        try
        {
            var entry = new AuditEntry
            {
                Operation = operation,
                EntityType = entityType,
                EntityId = entityId,
                Description = $"{operation} on {entityType} {entityId}",
                UserId = userId,
                Severity = GetSeverity(operation),
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null
            };
            
            return Result.Ok(entry);
        }
        catch (Exception ex)
        {
            return Result.Fail<AuditEntry>($"Failed to create change audit entry: {ex.Message}");
        }
    }
    
    public Result<AuditEntry> CreateErrorAuditEntry(AuditOperation operation, string description, Exception exception, 
        string? userId = null)
    {
        try
        {
            var entry = new AuditEntry
            {
                Operation = operation,
                Description = TruncateDescription(description),
                UserId = userId,
                Severity = AuditSeverity.Error,
                Success = false,
                ExceptionDetails = _options.IncludeStackTrace ? exception.ToString() : exception.Message
            };
            
            return Result.Ok(entry);
        }
        catch (Exception ex)
        {
            return Result.Fail<AuditEntry>($"Failed to create error audit entry: {ex.Message}");
        }
    }
    
    private string TruncateDescription(string description)
    {
        if (_options.MaxDescriptionLength > 0 && description.Length > _options.MaxDescriptionLength)
        {
            return description.Substring(0, _options.MaxDescriptionLength) + "...";
        }
        return description;
    }
    
    private static AuditSeverity GetSeverity(AuditOperation operation)
    {
        return operation switch
        {
            AuditOperation.Delete => AuditSeverity.Warning,
            AuditOperation.AccessDenied => AuditSeverity.Warning,
            AuditOperation.ConfigurationChange => AuditSeverity.Warning,
            _ => AuditSeverity.Information
        };
    }
}
