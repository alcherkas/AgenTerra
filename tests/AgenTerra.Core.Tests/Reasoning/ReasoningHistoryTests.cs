using AgenTerra.Core.Reasoning;

namespace AgenTerra.Core.Tests.Reasoning;

public class ReasoningHistoryTests
{
    [Fact]
    public async Task GetReasoningHistory_ReturnsReadOnlyList()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "readonly-test";

        await tool.ThinkAsync(new ThinkInput(sessionId, "Test", "Thought"));

        // Act
        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<ReasoningStep>>(history);
    }

    [Fact]
    public async Task ReasoningStep_ContainsCorrectType()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "type-test";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Think Step", "Thought"));
        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Analyze Step", "Result", "Analysis"));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal("think", history[0].Type);
        Assert.Equal("analyze", history[1].Type);
    }

    [Fact]
    public async Task ReasoningStep_ContainsCorrectTitle()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "title-test";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Custom Title", "Thought"));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal("Custom Title", history[0].Title);
    }

    [Fact]
    public async Task ThinkStep_ContentIncludesThoughtAndAction()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "content-test";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Test", "My thought",
            "My action"
        ));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Contains("My thought", history[0].Content);
        Assert.Contains("My action", history[0].Content);
    }

    [Fact]
    public async Task ThinkStep_ContentWithoutAction_OnlyIncludesThought()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "no-action-test";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Test", "My thought"
        ));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Contains("My thought", history[0].Content);
        Assert.DoesNotContain("Action:", history[0].Content);
    }

    [Fact]
    public async Task AnalyzeStep_ContentIncludesResultAnalysisAndNextAction()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "analyze-content-test";

        // Act
        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Test", "My result", "My analysis",
            NextAction.Validate
        ));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Contains("My result", history[0].Content);
        Assert.Contains("My analysis", history[0].Content);
        Assert.Contains("Validate", history[0].Content);
    }

    [Fact]
    public async Task ReasoningStep_ConfidenceIsStoredCorrectly()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "confidence-test";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Test", "Thought", null, 0.65));
        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Test", "R", "A", NextAction.Continue, 0.92));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(0.65, history[0].Confidence);
        Assert.Equal(0.92, history[1].Confidence);
    }

    [Fact]
    public async Task ReasoningStep_TimestampsAreChronological()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "timestamp-test";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "First", "Thought"));
        await Task.Delay(10); // Small delay to ensure different timestamps
        await tool.ThinkAsync(new ThinkInput(sessionId, "Second", "Thought"));
        await Task.Delay(10);
        await tool.ThinkAsync(new ThinkInput(sessionId, "Third", "Thought"));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(3, history.Count);
        Assert.True(history[0].Timestamp <= history[1].Timestamp);
        Assert.True(history[1].Timestamp <= history[2].Timestamp);
    }

    [Fact]
    public async Task ReasoningStep_AllFieldsArePopulated()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "complete-test";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Complete Step", "Full thought", "Full action", 0.88
        ));

        var history = tool.GetReasoningHistory(sessionId);
        var step = history[0];

        // Assert
        Assert.NotNull(step.Type);
        Assert.NotEmpty(step.Type);
        Assert.NotNull(step.Title);
        Assert.NotEmpty(step.Title);
        Assert.NotNull(step.Content);
        Assert.NotEmpty(step.Content);
        Assert.InRange(step.Confidence, 0.0, 1.0);
        Assert.NotEqual(default(DateTime), step.Timestamp);
    }

    [Fact]
    public async Task GetReasoningHistory_AfterMultipleCalls_ReturnsConsistentData()
    {
        // Arrange
        using var tool = new ReasoningTool();
        var sessionId = "consistency-test";

        await tool.ThinkAsync(new ThinkInput(sessionId, "Test", "Thought"));

        // Act
        var history1 = tool.GetReasoningHistory(sessionId);
        var history2 = tool.GetReasoningHistory(sessionId);
        var history3 = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(history1.Count, history2.Count);
        Assert.Equal(history2.Count, history3.Count);
        Assert.Equal(history1[0].Title, history2[0].Title);
        Assert.Equal(history2[0].Title, history3[0].Title);
    }
}
