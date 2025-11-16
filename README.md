# AuditTrails

A simple audit trail library for .NET with **FluentResults** integration. Creates audit entries that you save to your own database.

## Features

- ✨ **Simple** - Just creates audit entries, you handle storage
- 📝 **Comprehensive** - Track all CRUD operations
- 🔄 **Change Tracking** - Capture before/after values
- ✅ **FluentResults** - Built-in result-based error handling
- 🎯 **DI Ready** - Native ASP.NET Core support

## Installation

```bash
dotnet add package Eniks.AuditTrails
```

## Quick Start

### 1. Register Service

```csharp
using AuditTrails.Extensions;

builder.Services.AddAuditTrails();
```

### 2. Implement IAuditService (Your Storage Logic)

```csharp
using AuditTrails.Interfaces;
using AuditTrails.Models;
using FluentResults;

public class MyAuditService : IAuditService
{
    private readonly AppDbContext _db;
    
    public MyAuditService(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<Result> SaveAuditEntryAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            _db.AuditEntries.Add(entry);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to save audit: {ex.Message}");
        }
    }
}

// Register your service
builder.Services.AddScoped<IAuditService, MyAuditService>();
```

### 3. Use It

```csharp
public class UserService
{
    private readonly IAuditLogger _auditLogger;
    private readonly IAuditService _auditService;
    
    public UserService(IAuditLogger auditLogger, IAuditService auditService)
    {
        _auditLogger = auditLogger;
        _auditService = auditService;
    }
    
    public async Task<Result> CreateUserAsync(User user)
    {
        // ... create user ...
        
        // Create audit entry
        var auditResult = _auditLogger.CreateEntityAuditEntry(
            AuditOperation.Create,
            "User",
            user.Id,
            $"Created user {user.Email}",
            userId: currentUserId
        );
        
        if (auditResult.IsSuccess)
        {
            // Save to your database
            await _auditService.SaveAuditEntryAsync(auditResult.Value);
        }
        
        return Result.Ok();
    }
}
```

## Usage Examples

### Simple Audit

```csharp
var result = _auditLogger.CreateAuditEntry(
    AuditOperation.Login,
    "User logged in successfully",
    userId: "user123"
);

if (result.IsSuccess)
{
    await _auditService.SaveAuditEntryAsync(result.Value);
}
```

### Entity Operations

```csharp
var result = _auditLogger.CreateEntityAuditEntry(
    AuditOperation.Update,
    entityType: "Product",
    entityId: "prod-456",
    description: "Updated product price",
    userId: currentUserId
);

if (result.IsSuccess)
{
    await _auditService.SaveAuditEntryAsync(result.Value);
}
```

### Change Tracking

```csharp
var oldProduct = new { Name = "Widget", Price = 10.00 };
var newProduct = new { Name = "Widget", Price = 15.00 };

var result = _auditLogger.CreateChangeAuditEntry(
    AuditOperation.Update,
    "Product",
    productId,
    oldValues: oldProduct,
    newValues: newProduct,
    userId: currentUserId
);

if (result.IsSuccess)
{
    await _auditService.SaveAuditEntryAsync(result.Value);
}
```

### Error Logging

```csharp
try
{
    await PerformOperationAsync();
}
catch (Exception ex)
{
    var result = _auditLogger.CreateErrorAuditEntry(
        AuditOperation.Update,
        "Failed to update profile",
        ex,
        userId: currentUserId
    );
    
    if (result.IsSuccess)
    {
        await _auditService.SaveAuditEntryAsync(result.Value);
    }
}
```

## Configuration

```csharp
builder.Services.AddAuditTrails(options =>
{
    options.IncludeStackTrace = false;  // Include full stack trace (default: false)
    options.MaxDescriptionLength = 2000;  // Max description length (default: 2000)
});
```

## AuditEntry Model

```csharp
public class AuditEntry
{
    public string Id { get; set; }                  // Auto-generated GUID
    public DateTime Timestamp { get; set; }         // Auto-set to UTC now
    public AuditOperation Operation { get; set; }   // Type of operation
    public AuditSeverity Severity { get; set; }     // Auto-set based on operation
    public string? EntityType { get; set; }         // e.g., "User", "Product"
    public string? EntityId { get; set; }           // Entity identifier
    public string? UserId { get; set; }             // Who performed the action
    public string? Name { get; set; }               // User's name
    public string Description { get; set; }         // What happened
    public string? OldValues { get; set; }          // Before (JSON)
    public string? NewValues { get; set; }          // After (JSON)
    public Dictionary<string, string>? Metadata { get; set; }  // Extra data
    public string? ExceptionDetails { get; set; }   // Error info
    public bool Success { get; set; }               // Was it successful
}
```

## Operations

```csharp
public enum AuditOperation
{
    Create, Read, Update, Delete,
    Login, Logout, AccessDenied,
    Export, Import, ConfigurationChange, Custom
}
```

## Severity Levels

```csharp
public enum AuditSeverity
{
    Information,  // Normal operations
    Warning,      // Potentially problematic
    Error,        // Failed operations
    Critical,     // Severe issues
    Success       // Explicitly successful
}
```

## Database Setup Example

### Entity Framework Core

```csharp
public class AppDbContext : DbContext
{
    public DbSet<AuditEntry> AuditEntries { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EntityType);
        });
    }
}
```

### Querying Your Audit Logs

```csharp
// You handle all queries in your application
public async Task<List<AuditEntry>> GetUserActivityAsync(string userId, int days = 7)
{
    return await _db.AuditEntries
        .Where(a => a.UserId == userId)
        .Where(a => a.Timestamp >= DateTime.UtcNow.AddDays(-days))
        .OrderByDescending(a => a.Timestamp)
        .Take(50)
        .ToListAsync();
}

public async Task<List<AuditEntry>> GetEntityHistoryAsync(string entityType, string entityId)
{
    return await _db.AuditEntries
        .Where(a => a.EntityType == entityType && a.EntityId == entityId)
        .OrderByDescending(a => a.Timestamp)
        .ToListAsync();
}
```

## Why This Approach?

- **Your Database, Your Rules** - Store audit logs however you want
- **Simple** - No complex abstractions
- **Flexible** - Query and filter your way
- **Portable** - Works with any data store

## Building the Package

```bash
cd src/AuditTrails
dotnet pack -c Release
```

## License

MIT License - Free for commercial and personal use

## Support

For issues or questions, please open an issue on GitHub.
