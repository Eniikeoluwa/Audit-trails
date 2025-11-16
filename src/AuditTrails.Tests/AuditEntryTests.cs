using AuditTrails.Enums;
using AuditTrails.Models;
using FluentAssertions;

namespace AuditTrails.Tests;

public class AuditEntryTests
{
    [Fact]
    public void AuditEntry_ShouldGenerateId_OnCreation()
    {
        var entry = new AuditEntry();

        entry.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(entry.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void AuditEntry_ShouldSetTimestamp_OnCreation()
    {
        var before = DateTime.UtcNow;

        var entry = new AuditEntry();

        var after = DateTime.UtcNow;
        entry.Timestamp.Should().BeOnOrAfter(before);
        entry.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void AuditEntry_ShouldHaveDefaultValues()
    {
        // Act
        var entry = new AuditEntry();

        // Assert
        entry.Severity.Should().Be(AuditSeverity.Information);
        entry.Success.Should().BeTrue();
        entry.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void AuditEntry_ShouldAllowSettingProperties()
    {
        var entry = new AuditEntry
        {
            Operation = AuditOperation.Create,
            Severity = AuditSeverity.Warning,
            EntityType = "User",
            EntityId = "user123",
            UserId = "admin",
            Name = "Admin User",
            Description = "Test description",
            Success = false
        };

        entry.Operation.Should().Be(AuditOperation.Create);
        entry.Severity.Should().Be(AuditSeverity.Warning);
        entry.EntityType.Should().Be("User");
        entry.EntityId.Should().Be("user123");
        entry.UserId.Should().Be("admin");
        entry.Name.Should().Be("Admin User");
        entry.Description.Should().Be("Test description");
        entry.Success.Should().BeFalse();
    }

    [Fact]
    public void AuditEntry_ShouldSupportMetadata()
    {
        var entry = new AuditEntry
        {
            Metadata = new Dictionary<string, string>
            {
                ["Key1"] = "Value1",
                ["Key2"] = "Value2"
            }
        };

        entry.Metadata.Should().NotBeNull();
        entry.Metadata.Should().HaveCount(2);
        entry.Metadata["Key1"].Should().Be("Value1");
        entry.Metadata["Key2"].Should().Be("Value2");
    }

    [Fact]
    public void AuditEntry_ShouldSupportChangeTracking()
    {
        var entry = new AuditEntry
        {
            OldValues = "{\"price\": 10}",
            NewValues = "{\"price\": 15}"
        };

        entry.OldValues.Should().Be("{\"price\": 10}");
        entry.NewValues.Should().Be("{\"price\": 15}");
    }

    [Fact]
    public void AuditEntry_ShouldAllowNullableProperties()
    {
        var entry = new AuditEntry();

        entry.EntityType.Should().BeNull();
        entry.EntityId.Should().BeNull();
        entry.UserId.Should().BeNull();
        entry.Name.Should().BeNull();
        entry.OldValues.Should().BeNull();
        entry.NewValues.Should().BeNull();
        entry.Metadata.Should().BeNull();
        entry.ExceptionDetails.Should().BeNull();
    }

    [Fact]
    public void AuditEntry_ShouldGenerateUniqueIds()
    {
        var entry1 = new AuditEntry();
        var entry2 = new AuditEntry();
        var entry3 = new AuditEntry();

        var ids = new[] { entry1.Id, entry2.Id, entry3.Id };
        ids.Should().OnlyHaveUniqueItems();
    }
}

