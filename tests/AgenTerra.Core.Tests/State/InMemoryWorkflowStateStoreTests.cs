using AgenTerra.Core.State;
using AgenTerra.Core.State.Models;

namespace AgenTerra.Core.Tests.State;

public class InMemoryWorkflowStateStoreTests
{
    [Fact]
    public async Task GetSessionAsync_ReturnsNull_ForNonExistentSession()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "non-existent-session";

        // Act
        var result = await store.GetSessionAsync(sessionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSessionAsync_ThrowsArgumentNullException_WhenSessionIdIsNull()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.GetSessionAsync(null!));
    }

    [Fact]
    public async Task SaveSessionAsync_CreatesNewSession()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var session = new WorkflowSession
        {
            SessionId = "test-session",
            SessionState = new Dictionary<string, object>()
        };

        // Act
        await store.SaveSessionAsync(session);
        var result = await store.GetSessionAsync("test-session");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-session", result.SessionId);
    }

    [Fact]
    public async Task SaveSessionAsync_ThrowsArgumentNullException_WhenSessionIsNull()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.SaveSessionAsync(null!));
    }

    [Fact]
    public async Task SaveSessionAsync_UpdatesExistingSessionTimestamp()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var session = new WorkflowSession
        {
            SessionId = "test-session",
            SessionState = new Dictionary<string, object>()
        };

        // Act
        await store.SaveSessionAsync(session);
        var firstResult = await store.GetSessionAsync("test-session");
        var firstUpdatedAt = firstResult!.UpdatedAt;

        await Task.Delay(10); // Ensure time difference
        await store.SaveSessionAsync(session);
        var secondResult = await store.GetSessionAsync("test-session");
        var secondUpdatedAt = secondResult!.UpdatedAt;

        // Assert
        Assert.True(secondUpdatedAt > firstUpdatedAt);
    }

    [Fact]
    public async Task GetStateAsync_ReturnsCorrectTypedValue()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "test-session";
        var expectedValue = "test-value";

        // Act
        await store.SetStateAsync(sessionId, "key1", expectedValue);
        var result = await store.GetStateAsync<string>(sessionId, "key1");

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetStateAsync_ReturnsDefault_ForMissingKey()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "test-session";

        // Act
        await store.SetStateAsync(sessionId, "key1", "value1");
        var result = await store.GetStateAsync<string>(sessionId, "missing-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStateAsync_ReturnsDefault_ForNonExistentSession()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act
        var result = await store.GetStateAsync<string>("non-existent", "key1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStateAsync_ThrowsArgumentNullException_WhenSessionIdIsNull()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.GetStateAsync<string>(null!, "key"));
    }

    [Fact]
    public async Task GetStateAsync_ThrowsArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.GetStateAsync<string>("session", null!));
    }

    [Fact]
    public async Task SetStateAsync_CreatesSession_IfNotExists()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "new-session";

        // Act
        await store.SetStateAsync(sessionId, "key1", "value1");
        var session = await store.GetSessionAsync(sessionId);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(sessionId, session.SessionId);
    }

    [Fact]
    public async Task SetStateAsync_ThrowsArgumentNullException_WhenSessionIdIsNull()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.SetStateAsync<string>(null!, "key", "value"));
    }

    [Fact]
    public async Task SetStateAsync_ThrowsArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.SetStateAsync<string>("session", null!, "value"));
    }

    [Fact]
    public async Task SetStateAsync_UpdatesStateCorrectly()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "test-session";

        // Act
        await store.SetStateAsync(sessionId, "key1", "value1");
        await store.SetStateAsync(sessionId, "key2", 42);
        var value1 = await store.GetStateAsync<string>(sessionId, "key1");
        var value2 = await store.GetStateAsync<int>(sessionId, "key2");

        // Assert
        Assert.Equal("value1", value1);
        Assert.Equal(42, value2);
    }

    [Fact]
    public async Task SetStateAsync_PreservesImmutability()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "test-session";

        // Act
        await store.SetStateAsync(sessionId, "key1", "value1");
        var session1 = await store.GetSessionAsync(sessionId);
        
        await store.SetStateAsync(sessionId, "key2", "value2");
        var session2 = await store.GetSessionAsync(sessionId);

        // Assert
        Assert.NotEqual(session1!.UpdatedAt, session2!.UpdatedAt);
        Assert.Single(session1.SessionState);
        Assert.Equal(2, session2.SessionState.Count);
    }

    [Fact]
    public async Task DeleteSessionAsync_RemovesSession()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "test-session";
        await store.SetStateAsync(sessionId, "key1", "value1");

        // Act
        var deleteResult = await store.DeleteSessionAsync(sessionId);
        var getResult = await store.GetSessionAsync(sessionId);

        // Assert
        Assert.True(deleteResult);
        Assert.Null(getResult);
    }

    [Fact]
    public async Task DeleteSessionAsync_ReturnsFalse_ForNonExistentSession()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act
        var result = await store.DeleteSessionAsync("non-existent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteSessionAsync_ThrowsArgumentNullException_WhenSessionIdIsNull()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.DeleteSessionAsync(null!));
    }

    [Fact]
    public async Task GetAllSessionIdsAsync_ReturnsAllSessions()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        await store.SetStateAsync("session1", "key", "value");
        await store.SetStateAsync("session2", "key", "value");
        await store.SetStateAsync("session3", "key", "value");

        // Act
        var sessionIds = await store.GetAllSessionIdsAsync();

        // Assert
        Assert.Equal(3, sessionIds.Count);
        Assert.Contains("session1", sessionIds);
        Assert.Contains("session2", sessionIds);
        Assert.Contains("session3", sessionIds);
    }

    [Fact]
    public async Task GetAllSessionIdsAsync_ReturnsEmpty_WhenNoSessions()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();

        // Act
        var sessionIds = await store.GetAllSessionIdsAsync();

        // Assert
        Assert.Empty(sessionIds);
    }

    [Fact]
    public async Task ConcurrentOperations_AreThreadSafe()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "concurrent-session";
        var tasks = new List<Task>();

        // Act - Perform 100 concurrent write operations
        for (int i = 0; i < 100; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                await store.SetStateAsync(sessionId, $"key{index}", $"value{index}");
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var session = await store.GetSessionAsync(sessionId);
        Assert.NotNull(session);
        Assert.Equal(100, session.SessionState.Count);
    }

    [Fact]
    public async Task SetStateAsync_SupportsComplexTypes()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "test-session";
        var complexObject = new List<string> { "item1", "item2", "item3" };

        // Act
        await store.SetStateAsync(sessionId, "list", complexObject);
        var result = await store.GetStateAsync<List<string>>(sessionId, "list");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("item1", result[0]);
    }

    [Fact]
    public async Task GetSessionAsync_ReturnsCopy_NotReference()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var sessionId = "test-session";
        await store.SetStateAsync(sessionId, "key1", "value1");

        // Act
        var session1 = await store.GetSessionAsync(sessionId);
        var session2 = await store.GetSessionAsync(sessionId);

        // Assert
        Assert.NotSame(session1, session2);
        Assert.Equal(session1!.SessionId, session2!.SessionId);
    }

    [Fact]
    public async Task GetSessionAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await store.GetSessionAsync("test-session", cts.Token));
    }

    [Fact]
    public async Task SaveSessionAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        var session = new WorkflowSession
        {
            SessionId = "test-session",
            SessionState = new Dictionary<string, object>()
        };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await store.SaveSessionAsync(session, cts.Token));
    }

    [Fact]
    public async Task GetStateAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await store.GetStateAsync<string>("test-session", "key", cts.Token));
    }

    [Fact]
    public async Task SetStateAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await store.SetStateAsync("test-session", "key", "value", cts.Token));
    }

    [Fact]
    public async Task DeleteSessionAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await store.DeleteSessionAsync("test-session", cts.Token));
    }

    [Fact]
    public async Task GetAllSessionIdsAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var store = new InMemoryWorkflowStateStore();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await store.GetAllSessionIdsAsync(cts.Token));
    }
}
