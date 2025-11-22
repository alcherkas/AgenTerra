# Workflow Session State Management Implementation Task

**Priority:** HIGH  
**Status:** Not Started  
**Estimated Effort:** 2-3 days  
**Date Created:** November 22, 2025

## Overview

Implement minimal Workflow Session State Management as a foundational feature for AgenTerra. This provides persistent state across workflow runs without requiring database infrastructure initially. Uses in-memory storage with a clean interface that can be extended to database persistence later.

## Implementation Steps

### 1. Define Core Interfaces
**File:** `src/AgenTerra.Core/State/IWorkflowStateStore.cs`

```csharp
namespace AgenTerra.Core.State;

public interface IWorkflowStateStore
{
    Task<WorkflowSession?> GetSessionAsync(string sessionId);
    Task SaveSessionAsync(WorkflowSession session);
    Task<T?> GetStateAsync<T>(string sessionId, string key);
    Task SetStateAsync<T>(string sessionId, string key, T value);
    Task<bool> DeleteSessionAsync(string sessionId);
    Task<IReadOnlyList<string>> GetAllSessionIdsAsync();
}
```

**Acceptance Criteria:**
- [ ] Interface defined with XML documentation
- [ ] Generic type support for state values
- [ ] Async signatures for future database support
- [ ] Nullable return types where appropriate
- [ ] Session lifecycle methods (get, save, delete)

### 2. Define Session Models
**File:** `src/AgenTerra.Core/State/Models/WorkflowSession.cs`

```csharp
namespace AgenTerra.Core.State.Models;

public record WorkflowSession
{
    public required string SessionId { get; init; }
    public required Dictionary<string, object> SessionState { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? WorkflowId { get; init; }
    public string? UserId { get; init; }
    
    public WorkflowSession()
    {
        SessionState = new Dictionary<string, object>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Acceptance Criteria:**
- [ ] Immutable record type
- [ ] Required properties enforced
- [ ] Timestamps managed properly
- [ ] Optional metadata (WorkflowId, UserId)
- [ ] Default constructor initializes state dictionary

### 3. Implement In-Memory State Store
**File:** `src/AgenTerra.Core/State/InMemoryWorkflowStateStore.cs`

```csharp
namespace AgenTerra.Core.State;

public class InMemoryWorkflowStateStore : IWorkflowStateStore
{
    private readonly Dictionary<string, WorkflowSession> _sessions = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<WorkflowSession?> GetSessionAsync(string sessionId)
    {
        await _lock.WaitAsync();
        try
        {
            return _sessions.TryGetValue(sessionId, out var session) 
                ? session with { } // Return a copy
                : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveSessionAsync(WorkflowSession session)
    {
        await _lock.WaitAsync();
        try
        {
            var updatedSession = session with { UpdatedAt = DateTime.UtcNow };
            _sessions[session.SessionId] = updatedSession;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<T?> GetStateAsync<T>(string sessionId, string key)
    {
        var session = await GetSessionAsync(sessionId);
        if (session?.SessionState.TryGetValue(key, out var value) == true)
        {
            return (T?)value;
        }
        return default;
    }

    public async Task SetStateAsync<T>(string sessionId, string key, T value)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                session = new WorkflowSession
                {
                    SessionId = sessionId,
                    SessionState = new Dictionary<string, object>()
                };
            }

            var newState = new Dictionary<string, object>(session.SessionState)
            {
                [key] = value!
            };

            var updatedSession = session with 
            { 
                SessionState = newState,
                UpdatedAt = DateTime.UtcNow 
            };

            _sessions[sessionId] = updatedSession;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        await _lock.WaitAsync();
        try
        {
            return _sessions.Remove(sessionId);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<string>> GetAllSessionIdsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _sessions.Keys.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }
}
```

**Acceptance Criteria:**
- [ ] Thread-safe using SemaphoreSlim
- [ ] Immutable session updates (using `with` expressions)
- [ ] Auto-creates session if missing on SetStateAsync
- [ ] Updates timestamps correctly
- [ ] No external dependencies

### 4. Create Sample Console Application
**File:** `samples/AgenTerra.Sample/Program.cs`

**Sample Scenario:** Shopping Cart Workflow with Persistent State

```csharp
using AgenTerra.Core.State;
using AgenTerra.Core.State.Models;

var stateStore = new InMemoryWorkflowStateStore();
var sessionId = "user_123";

// Simulate first workflow run - Add items
Console.WriteLine("=== Run 1: Adding items to cart ===");
await stateStore.SetStateAsync(sessionId, "shopping_list", new List<string> { "milk", "bread" });
await stateStore.SetStateAsync(sessionId, "total_items", 2);

var session = await stateStore.GetSessionAsync(sessionId);
Console.WriteLine($"Session created at: {session?.CreatedAt}");

// Simulate second workflow run - Add more items
Console.WriteLine("\n=== Run 2: Adding more items ===");
var existingList = await stateStore.GetStateAsync<List<string>>(sessionId, "shopping_list");
existingList?.Add("eggs");
await stateStore.SetStateAsync(sessionId, "shopping_list", existingList);
await stateStore.SetStateAsync(sessionId, "total_items", 3);

// Simulate third workflow run - Check cart
Console.WriteLine("\n=== Run 3: Checking cart ===");
var shoppingList = await stateStore.GetStateAsync<List<string>>(sessionId, "shopping_list");
var totalItems = await stateStore.GetStateAsync<int>(sessionId, "total_items");

Console.WriteLine($"Shopping list: {string.Join(", ", shoppingList ?? new List<string>())}");
Console.WriteLine($"Total items: {totalItems}");

session = await stateStore.GetSessionAsync(sessionId);
Console.WriteLine($"Last updated: {session?.UpdatedAt}");

// Display all active sessions
Console.WriteLine("\n=== All Sessions ===");
var allSessions = await stateStore.GetAllSessionIdsAsync();
Console.WriteLine($"Active sessions: {string.Join(", ", allSessions)}");

// Cleanup
await stateStore.DeleteSessionAsync(sessionId);
Console.WriteLine("\nSession deleted successfully");
```

**Acceptance Criteria:**
- [ ] Demonstrates state persistence across "runs"
- [ ] Shows type-safe state retrieval
- [ ] Displays session metadata (timestamps)
- [ ] Shows session lifecycle (create, update, delete)
- [ ] Clear, easy to understand output

### 5. Write Unit Tests
**Files:** `tests/AgenTerra.Core.Tests/State/`

**Test Coverage:**

**InMemoryWorkflowStateStoreTests.cs:**
- [ ] GetSessionAsync returns null for non-existent session
- [ ] SaveSessionAsync creates new session
- [ ] SaveSessionAsync updates existing session timestamp
- [ ] GetStateAsync returns correct typed value
- [ ] GetStateAsync returns default for missing key
- [ ] SetStateAsync creates session if not exists
- [ ] SetStateAsync updates state correctly
- [ ] SetStateAsync preserves immutability
- [ ] DeleteSessionAsync removes session
- [ ] GetAllSessionIdsAsync returns all sessions
- [ ] Concurrent operations are thread-safe

**WorkflowSessionTests.cs:**
- [ ] Record initialization works correctly
- [ ] Required properties enforced
- [ ] With expressions create new instances
- [ ] Default constructor sets CreatedAt and UpdatedAt

**Minimum Coverage:** 85%

## Design Decisions

### In-Memory vs. Database Persistence
**Decision:** Start with in-memory `Dictionary<string, WorkflowSession>`

**Rationale:**
- Simplest implementation for Phase 1
- No external dependencies or database setup
- Easy to test and debug
- Interface allows swapping to database later
- Suitable for single-instance deployments and testing

**Future Enhancement:** Create PostgreSQL/SQLite implementations in Phase 2

### Thread Safety Approach
**Decision:** Use `SemaphoreSlim` instead of `lock`

**Rationale:**
- Compatible with async/await
- Prevents deadlocks in async contexts
- More flexible than lock statement
- Standard pattern for async-safe critical sections

### Immutability Pattern
**Decision:** Use record types with `with` expressions

**Rationale:**
- Prevents accidental state mutations
- Thread-safe by design
- Aligns with functional programming principles
- Clear intent when state changes occur

### Generic Type Support
**Decision:** Use `GetStateAsync<T>` and `SetStateAsync<T>`

**Rationale:**
- Type-safe state retrieval
- Avoids casting in consumer code
- Better IntelliSense support
- Standard .NET pattern

## Success Criteria

- [ ] All interfaces and models defined
- [ ] InMemoryWorkflowStateStore implementation complete
- [ ] Sample application runs successfully
- [ ] Unit tests pass with >85% coverage
- [ ] XML documentation for all public APIs
- [ ] Code follows SOLID principles
- [ ] No external dependencies added to AgenTerra.Core
- [ ] Builds without warnings on .NET 10.0
- [ ] Thread-safety verified with concurrent tests

## Follow-Up Tasks

After completion, create tasks for:
1. **PostgreSQL State Store** - Persistent storage using Npgsql
2. **SQLite State Store** - File-based persistence for local scenarios
3. **State Store Factory** - Configuration-based store selection
4. **Workflow Integration** - Connect state store to workflow execution
5. **State Serialization** - JSON serialization for complex types
6. **State Expiration** - Auto-cleanup of old sessions
7. **State Encryption** - Optional encryption for sensitive data

## References

- Gap Analysis: `/research/gaps.md` - Section 5: Advanced State Management
- Agno Reference: https://github.com/agno-agi/agno - Workflow session_state parameter
- Agent Guidelines: `/agents/CSharpExpert.agent.md`
- Microsoft Docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record

## Dependencies

**Required Features:**
- None (foundational feature)

**Enables Features:**
- AgentOS (needs session management)
- Workflow Agent Intelligence (needs history tracking)
- Knowledge Management (needs state for filtering context)

## Notes

- Keep implementation simple and focused
- Prioritize clean interface design over features
- Database persistence is Phase 2 - don't over-engineer
- Consider state size limits in future iterations
- Monitor memory usage for long-running sessions
