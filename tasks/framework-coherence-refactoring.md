# Framework Coherence Refactoring

**Priority:** High  
**Status:** Not Started  
**Created:** 2024-11-22  
**Estimated Effort:** 1-2 days

## Overview

Refactor the three core features (Reasoning Tools, Knowledge Readers, Workflow Session State) to create a coherent framework with consistent patterns, thread safety, and coding conventions.

## Current State Assessment

All three features are **functionally complete** with excellent test coverage (106 total tests), but have critical inconsistencies:

### Strengths ✅
- All features fully functional and well-tested
- Excellent test coverage (45 reasoning + 32 knowledge + 29 state tests)
- Good naming conventions and XML documentation
- Clean separation of concerns
- Minimal external dependencies
- Good samples and examples

### Critical Issues 🔴
1. **Thread safety inconsistency** - Reasoning uses `lock`, State uses `SemaphoreSlim`
2. **Missing CancellationToken** - Only Knowledge supports cancellation (2/3 features missing)
3. **Record syntax inconsistency** - Reasoning uses positional, others use property syntax
4. **No DI setup** - No extension methods for dependency injection
5. **Unclear exception pattern** - Only Knowledge has custom exception

## Detailed Analysis

### Thread Safety Mechanisms
| Feature | Mechanism | Type | Async-Compatible |
|---------|----------|------|------------------|
| Reasoning | `lock` statement | `object` | ❌ No (dangerous in async) |
| Knowledge | None (stateless) | N/A | N/A |
| State | `SemaphoreSlim` | Async-safe | ✅ Yes |

**Risk:** Using `lock` in async methods can cause deadlocks.

### CancellationToken Support
| Feature | Support | Default Value |
|---------|---------|---------------|
| Reasoning | ❌ None | N/A |
| Knowledge | ✅ All methods | ✅ `default` |
| State | ❌ None | N/A |

### Record Syntax
| Feature | Style | Validation |
|---------|-------|------------|
| Reasoning | Positional (`record(params)`) | `[Required]` attributes |
| Knowledge | Property (`required` keyword) | `required` keyword |
| State | Property (`required` keyword) | `required` keyword |

### Custom Exceptions
| Feature | Exception Type | Inner Exception Preserved |
|---------|---------------|---------------------------|
| Reasoning | ❌ None | N/A |
| Knowledge | ✅ `DocumentReaderException` | ✅ Yes |
| State | ❌ None | N/A |

## Implementation Tasks

### Phase 1: Critical Fixes (P0 - Must Do)

#### Task 1.1: Fix Reasoning Thread Safety
**Priority:** 🔴 Critical  
**Impact:** High (prevents async deadlocks)  
**Effort:** Low

**Files to Modify:**
- `src/AgenTerra.Core/Reasoning/ReasoningTool.cs`

**Changes:**
```csharp
// Replace
private readonly object _lock = new();

// With
private readonly SemaphoreSlim _lock = new(1, 1);

// Update all lock statements
lock (_lock) { ... }

// To
await _lock.WaitAsync(cancellationToken);
try { ... }
finally { _lock.Release(); }
```

**Tests to Update:**
- `tests/AgenTerra.Core.Tests/Reasoning/ThreadSafetyTests.cs`

#### Task 1.2: Add CancellationToken Support
**Priority:** 🔴 Critical  
**Impact:** High (enables cancellation of long operations)  
**Effort:** Low

**Files to Modify:**
- `src/AgenTerra.Core/Reasoning/IReasoningTool.cs`
- `src/AgenTerra.Core/Reasoning/ReasoningTool.cs`
- `src/AgenTerra.Core/State/IWorkflowStateStore.cs`
- `src/AgenTerra.Core/State/InMemoryWorkflowStateStore.cs`

**Changes:**
```csharp
// Add CancellationToken parameter to all async methods
Task<string> ThinkAsync(ThinkInput input, CancellationToken cancellationToken = default);
Task<string> AnalyzeAsync(AnalyzeInput input, CancellationToken cancellationToken = default);
Task<string> GetHistoryAsync(string sessionId, CancellationToken cancellationToken = default);

Task SaveSessionAsync(WorkflowSession session, CancellationToken cancellationToken = default);
Task<WorkflowSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);
Task<IReadOnlyList<WorkflowSession>> GetAllSessionsAsync(CancellationToken cancellationToken = default);
Task<bool> DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default);
```

**Tests to Add:**
- Cancellation token tests for Reasoning
- Cancellation token tests for State

### Phase 2: Framework Consistency (P1 - Should Do)

#### Task 2.1: Standardize Record Syntax
**Priority:** 🟡 Medium  
**Impact:** Medium (code consistency)  
**Effort:** Low

**Files to Modify:**
- `src/AgenTerra.Core/Reasoning/Models/ThinkInput.cs`
- `src/AgenTerra.Core/Reasoning/Models/AnalyzeInput.cs`
- `src/AgenTerra.Core/Reasoning/Models/ReasoningStep.cs`

**Changes:**
```csharp
// Convert from positional syntax
public record ThinkInput(
    [Required] string SessionId,
    [Required] string Question,
    string Context = ""
);

// To property syntax
public record ThinkInput
{
    public required string SessionId { get; init; }
    public required string Question { get; init; }
    public string Context { get; init; } = "";
}
```

#### Task 2.2: Add Custom Exceptions
**Priority:** 🟡 Medium  
**Impact:** Medium (error handling consistency)  
**Effort:** Low

**Files to Create:**
- `src/AgenTerra.Core/Reasoning/Models/ReasoningException.cs`
- `src/AgenTerra.Core/State/Models/WorkflowStateException.cs`

**Pattern to Follow:**
```csharp
public class ReasoningException : Exception
{
    public ReasoningException(string message) : base(message) { }
    public ReasoningException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

**Files to Update:**
- Use exceptions in validation logic
- Update tests to verify exception throwing

#### Task 2.3: Add Dependency Injection Extensions
**Priority:** 🟡 Medium  
**Impact:** Medium (ease of use)  
**Effort:** Medium

**Files to Create:**
- `src/AgenTerra.Core/ServiceCollectionExtensions.cs`

**Implementation:**
```csharp
using Microsoft.Extensions.DependencyInjection;

namespace AgenTerra.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgenTerra(this IServiceCollection services)
    {
        // Reasoning - Singleton for shared session management
        services.AddSingleton<IReasoningTool, ReasoningTool>();
        
        // State - Singleton for shared state storage
        services.AddSingleton<IWorkflowStateStore, InMemoryWorkflowStateStore>();
        
        // Knowledge - Transient (stateless)
        services.AddTransient<DocumentReaderFactory>();
        services.AddTransient<IDocumentReader, TextDocumentReader>();
        services.AddTransient<IDocumentReader, PdfDocumentReader>();
        
        return services;
    }
}
```

**Files to Update:**
- Add `Microsoft.Extensions.DependencyInjection.Abstractions` NuGet package
- Update samples to demonstrate DI usage
- Add DI usage tests

### Phase 3: Advanced Enhancements (P2 - Nice to Have)

#### Task 3.1: Add Metadata to Reasoning Steps
**Priority:** 🟢 Low  
**Impact:** Low (extensibility improvement)  
**Effort:** Low

**Files to Modify:**
- `src/AgenTerra.Core/Reasoning/Models/ReasoningStep.cs`

**Changes:**
```csharp
public record ReasoningStep
{
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required double Confidence { get; init; }
    public required DateTime Timestamp { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new(); // NEW
}
```

#### Task 3.2: Extract Common Abstractions
**Priority:** 🟢 Low  
**Impact:** Low (code reuse)  
**Effort:** Medium

**Files to Create:**
- `src/AgenTerra.Core/Common/IMetadataProvider.cs`
- `src/AgenTerra.Core/Common/ITimestamped.cs`

**Implementation:**
```csharp
public interface IMetadataProvider
{
    Dictionary<string, object> Metadata { get; }
}

public interface ITimestamped
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
}
```

**Files to Update:**
- Make `WorkflowSession`, `DocumentContent`, `ReasoningStep` implement interfaces

#### Task 3.3: Refactor Factory to Use DI
**Priority:** 🟢 Low  
**Impact:** Low (testability improvement)  
**Effort:** Medium

**Files to Modify:**
- `src/AgenTerra.Core/Knowledge/DocumentReaderFactory.cs`

**Changes:**
```csharp
public class DocumentReaderFactory
{
    private readonly Dictionary<string, IDocumentReader> _readers;

    public DocumentReaderFactory(IEnumerable<IDocumentReader> readers)
    {
        _readers = new Dictionary<string, IDocumentReader>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var reader in readers)
        {
            foreach (var extension in reader.SupportedExtensions)
            {
                _readers[extension] = reader;
            }
        }
    }
    
    // Rest of implementation...
}
```

#### Task 3.4: Add Common Validation Utilities
**Priority:** 🟢 Low  
**Impact:** Low (code consistency)  
**Effort:** Low

**Files to Create:**
- `src/AgenTerra.Core/Common/Validate.cs`

**Implementation:**
```csharp
internal static class Validate
{
    public static void NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
    }
    
    public static void NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }
}
```

## Testing Requirements

### Test Updates Required
1. **Thread Safety Tests** - Update to test `SemaphoreSlim` instead of `lock`
2. **Cancellation Tests** - Add tests for all new `CancellationToken` parameters
3. **Record Syntax Tests** - Verify validation still works with property syntax
4. **Exception Tests** - Test new custom exceptions
5. **DI Tests** - Test service registration and resolution

### Test Coverage Goals
- Maintain or exceed current 106 test count
- Add minimum 15 new tests for cancellation and DI
- Keep all existing tests passing

## Samples Updates

### Samples to Update
1. **`ReasoningToolSample.cs`** - Add cancellation demonstration
2. **`KnowledgeReadersSample.cs`** - Already has cancellation, add DI example
3. **`WorkflowSessionStateSample.cs`** - Add cancellation and DI demonstration

### New Sample to Create
4. **`DependencyInjectionSample.cs`** - Show full DI setup with all features

## Success Criteria

✅ All async methods support `CancellationToken`  
✅ All features use `SemaphoreSlim` for thread safety (where applicable)  
✅ All record types use consistent property syntax  
✅ All features have custom exception types  
✅ DI extension method `AddAgenTerra()` available  
✅ All 106+ tests passing  
✅ Samples updated to demonstrate new patterns  
✅ No breaking changes to public APIs (only additions)

## Migration Guide for Users

### Breaking Changes
**None** - All changes are additive and backward compatible.

### Recommended Updates
```csharp
// Old way (still works)
var reasoningTool = new ReasoningTool();
await reasoningTool.ThinkAsync(new ThinkInput("session1", "question"));

// New way (recommended)
services.AddAgenTerra();
var reasoningTool = serviceProvider.GetRequiredService<IReasoningTool>();
await reasoningTool.ThinkAsync(
    new ThinkInput { SessionId = "session1", Question = "question" },
    cancellationToken
);
```

## Dependencies

- No new external dependencies for P0/P1 tasks
- `Microsoft.Extensions.DependencyInjection.Abstractions` for P1 Task 2.3

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Breaking existing tests | High | Run all tests after each change |
| `SemaphoreSlim` not disposed | Medium | Implement `IDisposable` on classes |
| Performance impact of `SemaphoreSlim` | Low | Benchmark before/after |
| DI complexity for simple use cases | Low | Keep parameterless constructors |

## Timeline Estimate

- **Phase 1 (P0):** 4-6 hours
- **Phase 2 (P1):** 6-8 hours  
- **Phase 3 (P2):** 4-6 hours (optional)
- **Total:** 1-2 days for critical and high-priority work

## Notes

- Follow Microsoft Agent Framework guidelines throughout
- Maintain XML documentation for all public APIs
- Keep commit messages descriptive and reference this task
- Create feature branch: `feature/framework-coherence-refactoring`
- Update `Agents.md` if patterns change significantly

## References

- Original task files:
  - `tasks/reasoning-tools-implementation.md`
  - `tasks/knowledge-readers-implementation.md`
  - `tasks/workflow-session-state.md`
- Analysis document: (This task was created from comprehensive feature review)
