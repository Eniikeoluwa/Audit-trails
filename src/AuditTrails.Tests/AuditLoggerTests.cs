using AuditTrails.Enums;
using AuditTrails.Interfaces;
using AuditTrails.Models;
using AuditTrails.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AuditTrails.Tests;

public class AuditLoggerTests
{
    private readonly IAuditLogger _logger;
    private readonly AuditOptions _options;

    public AuditLoggerTests()
    {
        _options = new AuditOptions();
        _logger = new AuditLogger(Options.Create(_options));
    }

    [Fact]
    public void CreateAuditEntry_ShouldReturnSuccess()
    {
        var result = _logger.CreateAuditEntry(
            AuditOperation.Create,
            "Test operation",
            "user123"
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Description.Should().Be("Test operation");
        result.Value.UserId.Should().Be("user123");
        result.Value.Operation.Should().Be(AuditOperation.Create);
    }

    [Fact]
    public void CreateAuditEntry_ShouldGenerateId()
    {
        var result = _logger.CreateAuditEntry(
            AuditOperation.Create,
            "Test",
            "user123"
        );

        result.Value.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(result.Value.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void CreateAuditEntry_ShouldSetTimestamp()
    {
        var before = DateTime.UtcNow;

        var result = _logger.CreateAuditEntry(
            AuditOperation.Create,
            "Test",
            "user123"
        );

        var after = DateTime.UtcNow;
        result.Value.Timestamp.Should().BeOnOrAfter(before);
        result.Value.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void CreateAuditEntry_ShouldSetCorrectSeverity_ForDifferentOperations()
    {
        var deleteResult = _logger.CreateAuditEntry(AuditOperation.Delete, "Delete", "user1");
        var accessDeniedResult = _logger.CreateAuditEntry(AuditOperation.AccessDenied, "Access denied", "user2");
        var createResult = _logger.CreateAuditEntry(AuditOperation.Create, "Create", "user3");

        deleteResult.Value.Severity.Should().Be(AuditSeverity.Warning);
        accessDeniedResult.Value.Severity.Should().Be(AuditSeverity.Warning);
        createResult.Value.Severity.Should().Be(AuditSeverity.Information);
    }

    [Fact]
    public void CreateEntityAuditEntry_ShouldIncludeEntityInfo()
    {
        var result = _logger.CreateEntityAuditEntry(
            AuditOperation.Update,
            "Product",
            "prod-123",
            "Updated product price",
            "user456"
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.EntityType.Should().Be("Product");
        result.Value.EntityId.Should().Be("prod-123");
        result.Value.Description.Should().Be("Updated product price");
        result.Value.UserId.Should().Be("user456");
    }

    [Fact]
    public void CreateChangeAuditEntry_ShouldSerializeValues()
    {
        var oldProduct = new { Name = "Widget", Price = 10.00 };
        var newProduct = new { Name = "Widget", Price = 15.00 };

        var result = _logger.CreateChangeAuditEntry(
            AuditOperation.Update,
            "Product",
            "prod-123",
            oldProduct,
            newProduct,
            "user789"
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.OldValues.Should().Contain("10");
        result.Value.NewValues.Should().Contain("15");
        result.Value.OldValues.Should().Contain("Widget");
    }

    [Fact]
    public void CreateChangeAuditEntry_ShouldHandleNullValues()
    {
        var result = _logger.CreateChangeAuditEntry<object>(
            AuditOperation.Create,
            "Product",
            "prod-123",
            null,
            new { Name = "Widget" },
            "user789"
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.OldValues.Should().BeNull();
        result.Value.NewValues.Should().NotBeNull();
    }

    [Fact]
    public void CreateErrorAuditEntry_ShouldLogExceptionDetails()
    {
        var exception = new InvalidOperationException("Test exception");

        var result = _logger.CreateErrorAuditEntry(
            AuditOperation.Update,
            "Failed to update user",
            exception,
            "user123"
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Severity.Should().Be(AuditSeverity.Error);
        result.Value.Success.Should().BeFalse();
        result.Value.ExceptionDetails.Should().Contain("Test exception");
    }

    [Fact]
    public void CreateErrorAuditEntry_ShouldIncludeStackTrace_WhenEnabled()
    {
        _options.IncludeStackTrace = true;
        var logger = new AuditLogger(Options.Create(_options));
        var exception = new InvalidOperationException("Test exception");

        var result = logger.CreateErrorAuditEntry(
            AuditOperation.Update,
            "Failed operation",
            exception,
            "user123"
        );

        result.Value.ExceptionDetails.Should().Contain("InvalidOperationException");
        result.Value.ExceptionDetails.Should().Contain("Test exception");
    }

    [Fact]
    public void CreateAuditEntry_ShouldTruncateDescription_WhenTooLong()
    {
        _options.MaxDescriptionLength = 50;
        var logger = new AuditLogger(Options.Create(_options));
        var longDescription = new string('a', 100);

        var result = logger.CreateAuditEntry(
            AuditOperation.Create,
            longDescription,
            "user123"
        );

        result.Value.Description.Length.Should().Be(53); // 50 + "..."
        result.Value.Description.Should().EndWith("...");
    }

    [Fact]
    public void CreateAuditEntry_ShouldNotTruncate_WhenMaxLengthIsZero()
    {
        _options.MaxDescriptionLength = 0;
        var logger = new AuditLogger(Options.Create(_options));
        var longDescription = new string('a', 5000);

        var result = logger.CreateAuditEntry(
            AuditOperation.Create,
            longDescription,
            "user123"
        );

        result.Value.Description.Length.Should().Be(5000);
    }

    [Fact]
    public void CreateAuditEntry_ShouldSetSuccessToTrue_ByDefault()
    {
        var result = _logger.CreateAuditEntry(AuditOperation.Create, "Test", "user123");

        result.Value.Success.Should().BeTrue();
    }

    [Fact]
    public void CreateErrorAuditEntry_ShouldSetSuccessToFalse()
    {
        var exception = new Exception("Test error");

        var result = _logger.CreateErrorAuditEntry(AuditOperation.Update, "Failed", exception, "user123");

        result.Value.Success.Should().BeFalse();
    }

    [Fact]
    public void CreateAuditEntry_ShouldGenerateUniqueIds()
    {
        var result1 = _logger.CreateAuditEntry(AuditOperation.Create, "Entry 1", "user1");
        var result2 = _logger.CreateAuditEntry(AuditOperation.Create, "Entry 2", "user2");
        var result3 = _logger.CreateAuditEntry(AuditOperation.Create, "Entry 3", "user3");

        var ids = new[] { result1.Value.Id, result2.Value.Id, result3.Value.Id };
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void CreateEntityAuditEntry_ShouldWorkWithoutUserId()
    {
        var result = _logger.CreateEntityAuditEntry(
            AuditOperation.Read,
            "Document",
            "doc-123",
            "Document accessed"
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().BeNull();
    }
}
