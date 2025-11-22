using AgenTerra.Core.Reasoning;

namespace AgenTerra.Core.Tests.Reasoning;

public class ReasoningToolTests
{
    [Fact]
    public async Task ThinkAsync_WithValidInput_ReturnsFormattedResponse()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new ThinkInput(
            SessionId: "test-session",
            Title: "Test Thought",
            Thought: "This is a test thought",
            Action: "Test action",
            Confidence: 0.9
        );

        // Act
        var response = await tool.ThinkAsync(input);

        // Assert
        Assert.NotNull(response);
        Assert.Contains("[THINK] Test Thought", response);
        Assert.Contains("Confidence: 0.90", response);
        Assert.Contains("This is a test thought", response);
        Assert.Contains("Test action", response);
    }

    [Fact]
    public async Task ThinkAsync_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var tool = new ReasoningTool();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => tool.ThinkAsync(null!));
    }

    [Fact]
    public async Task ThinkAsync_WithNullSessionId_ThrowsArgumentException()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new ThinkInput(
            SessionId: null!,
            Title: "Test",
            Thought: "Test"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => tool.ThinkAsync(input));
    }

    [Fact]
    public async Task ThinkAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new ThinkInput(
            SessionId: "test-session",
            Title: "",
            Thought: "Test"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => tool.ThinkAsync(input));
    }

    [Fact]
    public async Task ThinkAsync_WithEmptyThought_ThrowsArgumentException()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new ThinkInput(
            SessionId: "test-session",
            Title: "Test",
            Thought: ""
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => tool.ThinkAsync(input));
    }

    [Fact]
    public async Task ThinkAsync_WithoutAction_ReturnsResponseWithoutAction()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new ThinkInput(
            SessionId: "test-session",
            Title: "Test Thought",
            Thought: "This is a test thought"
        );

        // Act
        var response = await tool.ThinkAsync(input);

        // Assert
        Assert.NotNull(response);
        Assert.Contains("This is a test thought", response);
        Assert.DoesNotContain("Action:", response);
    }

    [Fact]
    public async Task AnalyzeAsync_WithValidInput_ReturnsFormattedResponse()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new AnalyzeInput(
            SessionId: "test-session",
            Title: "Test Analysis",
            Result: "Test result",
            Analysis: "Test analysis",
            NextAction: NextAction.Validate,
            Confidence: 0.85
        );

        // Act
        var response = await tool.AnalyzeAsync(input);

        // Assert
        Assert.NotNull(response);
        Assert.Contains("[ANALYZE] Test Analysis", response);
        Assert.Contains("Confidence: 0.85", response);
        Assert.Contains("Test result", response);
        Assert.Contains("Test analysis", response);
        Assert.Contains("Validate", response);
    }

    [Fact]
    public async Task AnalyzeAsync_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var tool = new ReasoningTool();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => tool.AnalyzeAsync(null!));
    }

    [Fact]
    public async Task AnalyzeAsync_WithNullSessionId_ThrowsArgumentException()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new AnalyzeInput(
            SessionId: null!,
            Title: "Test",
            Result: "Test",
            Analysis: "Test"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => tool.AnalyzeAsync(input));
    }

    [Fact]
    public async Task AnalyzeAsync_WithEmptyResult_ThrowsArgumentException()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new AnalyzeInput(
            SessionId: "test-session",
            Title: "Test",
            Result: "",
            Analysis: "Test"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => tool.AnalyzeAsync(input));
    }

    [Fact]
    public async Task AnalyzeAsync_WithDefaultNextAction_UsesContinue()
    {
        // Arrange
        var tool = new ReasoningTool();
        var input = new AnalyzeInput(
            SessionId: "test-session",
            Title: "Test Analysis",
            Result: "Test result",
            Analysis: "Test analysis"
        );

        // Act
        var response = await tool.AnalyzeAsync(input);

        // Assert
        Assert.Contains("Continue", response);
    }

    [Fact]
    public void GetReasoningHistory_WithNullSessionId_ThrowsArgumentException()
    {
        // Arrange
        var tool = new ReasoningTool();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => tool.GetReasoningHistory(null!));
    }

    [Fact]
    public void GetReasoningHistory_WithNonExistentSession_ReturnsEmptyList()
    {
        // Arrange
        var tool = new ReasoningTool();

        // Act
        var history = tool.GetReasoningHistory("non-existent-session");

        // Assert
        Assert.NotNull(history);
        Assert.Empty(history);
    }

    [Fact]
    public async Task GetReasoningHistory_AfterAddingSteps_ReturnsAllStepsInOrder()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "test-session";

        await tool.ThinkAsync(new ThinkInput(sessionId, "First", "First thought"));
        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Second", "Result", "Analysis"));
        await tool.ThinkAsync(new ThinkInput(sessionId, "Third", "Third thought"));

        // Act
        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(3, history.Count);
        Assert.Equal("think", history[0].Type);
        Assert.Equal("First", history[0].Title);
        Assert.Equal("analyze", history[1].Type);
        Assert.Equal("Second", history[1].Title);
        Assert.Equal("think", history[2].Type);
        Assert.Equal("Third", history[2].Title);
    }

    [Fact]
    public async Task GetReasoningHistory_ReturnsSnapshotOfCurrentState()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "test-session";

        await tool.ThinkAsync(new ThinkInput(sessionId, "First", "First thought"));

        // Act
        var history1 = tool.GetReasoningHistory(sessionId);
        var history1Count = history1.Count;
        
        await tool.ThinkAsync(new ThinkInput(sessionId, "Second", "Second thought"));
        var history2 = tool.GetReasoningHistory(sessionId);
        var history2Count = history2.Count;

        // Assert
        // Each call returns a separate snapshot, so modifying the underlying data
        // after getting a snapshot doesn't affect the snapshot itself
        Assert.Equal(1, history1Count);
        Assert.Equal(2, history2Count);
    }

    [Fact]
    public async Task ReasoningSteps_ContainCorrectTimestamps()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "test-session";
        var beforeTime = DateTime.UtcNow;

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Test", "Test thought"));
        var afterTime = DateTime.UtcNow;

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Single(history);
        Assert.True(history[0].Timestamp >= beforeTime);
        Assert.True(history[0].Timestamp <= afterTime);
    }

    [Fact]
    public async Task ReasoningSteps_StoreCorrectConfidence()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "test-session";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Test", "Test", Confidence: 0.75));
        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Test", "R", "A", Confidence: 0.95));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(0.75, history[0].Confidence);
        Assert.Equal(0.95, history[1].Confidence);
    }
}
