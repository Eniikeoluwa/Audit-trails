using AuditTrails.Enums;

namespace AuditTrails.Models;
public class AuditEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AuditOperation Operation { get; set; }
    public AuditSeverity Severity { get; set; } = AuditSeverity.Information;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public string? ExceptionDetails { get; set; }
    public bool Success { get; set; } = true;
}

