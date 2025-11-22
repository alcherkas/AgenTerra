# Reasoning Tools Implementation Task

**Priority:** HIGH  
**Status:** Not Started  
**Estimated Effort:** 2-3 days  
**Date Created:** November 22, 2025

## Overview

Implement minimal Reasoning Tools feature as the first AgenTerra capability. This provides foundational step-by-step reasoning without requiring deployment infrastructure, external integrations, or database persistence.

## Implementation Steps

### 1. Define Core Interfaces
**File:** `src/AgenTerra.Core/Reasoning/IReasoningTool.cs`

```csharp
public interface IReasoningTool
{
    Task<string> ThinkAsync(ThinkInput input);
    Task<string> AnalyzeAsync(AnalyzeInput input);
    IReadOnlyList<ReasoningStep> GetReasoningHistory(string sessionId);
}

public enum NextAction
{
    Continue,
    Validate,
    FinalAnswer
}
```

**Acceptance Criteria:**
- [ ] Interface defined with XML documentation
- [ ] NextAction enum covers all reasoning states
- [ ] Methods return simple string responses (for LLM consumption)

### 2. Define Reasoning Models
**Files:** `src/AgenTerra.Core/Reasoning/Models/`

```csharp
// ThinkInput.cs
public record ThinkInput(
    string SessionId,
    string Title,
    string Thought,
    string? Action = null,
    double Confidence = 0.8
);

// AnalyzeInput.cs
public record AnalyzeInput(
    string SessionId,
    string Title,
    string Result,
    string Analysis,
    NextAction NextAction = NextAction.Continue,
    double Confidence = 0.8
);

// ReasoningStep.cs
public record ReasoningStep(
    string Type, // "think" or "analyze"
    string Title,
    string Content,
    double Confidence,
    DateTime Timestamp
);
```

**Acceptance Criteria:**
- [ ] Records use C# 14 primary constructors
- [ ] Validation attributes where appropriate
- [ ] Immutable by default (record types)

### 3. Implement Basic Reasoning Tool
**File:** `src/AgenTerra.Core/Reasoning/ReasoningTool.cs`

```csharp
public class ReasoningTool : IReasoningTool
{
    private readonly Dictionary<string, List<ReasoningStep>> _sessions = new();
    private readonly object _lock = new();

    public Task<string> ThinkAsync(ThinkInput input)
    {
        // Store reasoning step
        // Return formatted response
    }

    public Task<string> AnalyzeAsync(AnalyzeInput input)
    {
        // Store analysis step
        // Return formatted response
    }

    public IReadOnlyList<ReasoningStep> GetReasoningHistory(string sessionId)
    {
        // Return immutable view of session history
    }
}
```

**Acceptance Criteria:**
- [ ] Thread-safe session storage
- [ ] Formatted string responses suitable for LLM consumption
- [ ] History retrieval works correctly
- [ ] No external dependencies

### 4. Create Sample Console Application
**File:** `samples/AgenTerra.Sample/Program.cs`

**Sample Scenario:** Fox, Chicken, and Grain River Crossing Puzzle

```csharp
var reasoningTool = new ReasoningTool();
var sessionId = Guid.NewGuid().ToString();

// Step 1: Think
await reasoningTool.ThinkAsync(new ThinkInput(
    SessionId: sessionId,
    Title: "Initial State Analysis",
    Thought: "Man, fox, chicken, grain on left bank. Goal: all on right bank.",
    Action: "Identify constraints"
));

// Step 2: Analyze
await reasoningTool.AnalyzeAsync(new AnalyzeInput(
    SessionId: sessionId,
    Title: "Constraint Analysis",
    Result: "Fox eats chicken if alone. Chicken eats grain if alone.",
    Analysis: "Must never leave fox+chicken or chicken+grain together",
    NextAction: NextAction.Continue
));

// ... more steps ...

// Display reasoning history
var history = reasoningTool.GetReasoningHistory(sessionId);
foreach (var step in history)
{
    Console.WriteLine($"[{step.Type}] {step.Title} (confidence: {step.Confidence})");
    Console.WriteLine(step.Content);
}
```

**Acceptance Criteria:**
- [ ] Demonstrates full think/analyze workflow
- [ ] Shows clear reasoning progression
- [ ] Outputs formatted history
- [ ] Easy to understand for new users

### 5. Write Unit Tests
**Files:** `tests/AgenTerra.Core.Tests/Reasoning/`

**Test Coverage:**
- [ ] `ReasoningToolTests.cs` - Basic think/analyze operations
- [ ] `SessionManagementTests.cs` - Multi-session isolation
- [ ] `ReasoningHistoryTests.cs` - History retrieval accuracy
- [ ] Thread-safety tests for concurrent operations

**Minimum Coverage:** 80%

## Design Decisions

### In-Memory vs. Persistent Storage
**Decision:** Start with in-memory `Dictionary<string, List<ReasoningStep>>`

**Rationale:**
- Simplest implementation for v1
- No external dependencies
- Easy to test
- Can extract `ISessionStateStore` interface later when persistence is needed

**Future Enhancement:** Add interface when implementing Workflow Session State feature

### Synchronous vs. Asynchronous
**Decision:** Use async methods despite in-memory implementation

**Rationale:**
- Prepares for future LLM integration
- Aligns with Microsoft.Extensions.AI patterns
- Standard pattern for agent tools
- No performance penalty for simple Task.FromResult

### LLM Integration
**Decision:** Defer to Phase 2

**Rationale:**
- Focus on data structures and workflow patterns first
- Allows manual testing without API keys/costs
- Tool interface can be tested independently
- LLM integration should use Microsoft.Extensions.AI abstractions

## Success Criteria

- [ ] All interfaces and models defined
- [ ] ReasoningTool implementation complete
- [ ] Sample application runs successfully
- [ ] Unit tests pass with >80% coverage
- [ ] Documentation includes XML comments
- [ ] Code follows SOLID principles
- [ ] No external dependencies added to AgenTerra.Core
- [ ] Builds without warnings on .NET 10.0

## Follow-Up Tasks

After completion, create tasks for:
1. **LLM Integration** - Add Microsoft.Extensions.AI integration for automatic reasoning
2. **Reasoning Instructions** - Template system for auto-injecting reasoning guidance
3. **Visualization** - Display reasoning steps in structured format (Markdown/HTML)

## References

- Gap Analysis: `/research/gaps.md` - Section 2: Reasoning Tools
- Agno Reference: https://github.com/agno-agi/agno - `agno/tools/reasoning.py`
- Agent Guidelines: `/agents/CSharpExpert.agent.md`
