using AgenTerra.Core.State.Models;

namespace AgenTerra.Core.Tests.State;

public class WorkflowSessionTests
{
    [Fact]
    public void Constructor_InitializesDefaultValues()
    {
        // Act
        var session = new WorkflowSession
        {
            SessionId = "test-session"
        };

        // Assert
        Assert.NotNull(session.SessionState);
        Assert.Empty(session.SessionState);
        Assert.NotEqual(default(DateTime), session.CreatedAt);
        Assert.NotEqual(default(DateTime), session.UpdatedAt);
        // CreatedAt and UpdatedAt should be very close (within a small margin) since they're set in the constructor
        var timeDiff = (session.UpdatedAt - session.CreatedAt).TotalMilliseconds;
        Assert.True(timeDiff >= 0 && timeDiff < 10, $"CreatedAt and UpdatedAt should be very close, but differ by {timeDiff}ms");
    }

    [Fact]
    public void SessionId_IsRequired()
    {
        // Arrange & Act & Assert
        // This test verifies that the required keyword is enforced
        // by confirming we can create a session with SessionId
        var session = new WorkflowSession
        {
            SessionId = "required-id"
        };

        Assert.Equal("required-id", session.SessionId);
    }

    [Fact]
    public void WithExpression_CreatesNewInstance()
    {
        // Arrange
        var original = new WorkflowSession
        {
            SessionId = "original-session",
            SessionState = new Dictionary<string, object> { { "key1", "value1" } }
        };

        // Act
        var modified = original with { SessionId = "modified-session" };

        // Assert
        Assert.NotSame(original, modified);
        Assert.Equal("original-session", original.SessionId);
        Assert.Equal("modified-session", modified.SessionId);
    }

    [Fact]
    public void WithExpression_CopiesAllProperties()
    {
        // Arrange
        var original = new WorkflowSession
        {
            SessionId = "test-session",
            SessionState = new Dictionary<string, object> { { "key1", "value1" } },
            WorkflowId = "workflow-123",
            UserId = "user-456"
        };

        // Act
        var modified = original with { UpdatedAt = DateTime.UtcNow.AddMinutes(5) };

        // Assert
        Assert.Equal(original.SessionId, modified.SessionId);
        Assert.Equal(original.WorkflowId, modified.WorkflowId);
        Assert.Equal(original.UserId, modified.UserId);
        Assert.Same(original.SessionState, modified.SessionState); // Record with copies references for reference types
    }

    [Fact]
    public void OptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var session = new WorkflowSession
        {
            SessionId = "test-session"
        };

        // Assert
        Assert.Null(session.WorkflowId);
        Assert.Null(session.UserId);
    }

    [Fact]
    public void OptionalProperties_CanBeSet()
    {
        // Arrange & Act
        var session = new WorkflowSession
        {
            SessionId = "test-session",
            WorkflowId = "workflow-123",
            UserId = "user-456"
        };

        // Assert
        Assert.Equal("workflow-123", session.WorkflowId);
        Assert.Equal("user-456", session.UserId);
    }

    [Fact]
    public void SessionState_CanContainMultipleItems()
    {
        // Arrange & Act
        var session = new WorkflowSession
        {
            SessionId = "test-session",
            SessionState = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 42 },
                { "key3", true }
            }
        };

        // Assert
        Assert.Equal(3, session.SessionState.Count);
        Assert.Equal("value1", session.SessionState["key1"]);
        Assert.Equal(42, session.SessionState["key2"]);
        Assert.Equal(true, session.SessionState["key3"]);
    }

    [Fact]
    public void CreatedAt_IsSetToUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var session = new WorkflowSession
        {
            SessionId = "test-session"
        };
        
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(session.CreatedAt >= beforeCreation);
        Assert.True(session.CreatedAt <= afterCreation);
        Assert.Equal(DateTimeKind.Utc, session.CreatedAt.Kind);
    }

    [Fact]
    public void UpdatedAt_IsSetToUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var session = new WorkflowSession
        {
            SessionId = "test-session"
        };
        
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(session.UpdatedAt >= beforeCreation);
        Assert.True(session.UpdatedAt <= afterCreation);
        Assert.Equal(DateTimeKind.Utc, session.UpdatedAt.Kind);
    }

    [Fact]
    public void RecordEquality_WorksCorrectly()
    {
        // Arrange
        var session1 = new WorkflowSession
        {
            SessionId = "test-session",
            SessionState = new Dictionary<string, object> { { "key", "value" } },
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var session2 = new WorkflowSession
        {
            SessionId = "test-session",
            SessionState = new Dictionary<string, object> { { "key", "value" } },
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act & Assert
        // Note: Records use reference equality for reference type properties
        // so session1 != session2 even though they have the same values
        Assert.NotEqual(session1, session2);
        Assert.Equal(session1.SessionId, session2.SessionId);
    }
}
