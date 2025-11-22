using AgenTerra.Core.Reasoning;

namespace AgenTerra.Core.Tests.Reasoning;

public class SessionManagementTests
{
    [Fact]
    public async Task DifferentSessions_AreIsolatedFromEachOther()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId1 = "session-1";
        var sessionId2 = "session-2";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId1, "Session 1 Step 1", "First session thought"));
        await tool.ThinkAsync(new ThinkInput(sessionId2, "Session 2 Step 1", "Second session thought"));
        await tool.ThinkAsync(new ThinkInput(sessionId1, "Session 1 Step 2", "First session second thought"));

        var history1 = tool.GetReasoningHistory(sessionId1);
        var history2 = tool.GetReasoningHistory(sessionId2);

        // Assert
        Assert.Equal(2, history1.Count);
        Assert.Single(history2);
        Assert.All(history1, step => Assert.Contains("Session 1", step.Title));
        Assert.All(history2, step => Assert.Contains("Session 2", step.Title));
    }

    [Fact]
    public async Task MultipleSessions_CanBeCreatedConcurrently()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionCount = 10;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < sessionCount; i++)
        {
            var sessionId = $"session-{i}";
            var task = tool.ThinkAsync(new ThinkInput(sessionId, $"Session {i}", $"Thought for session {i}"
            ));
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < sessionCount; i++)
        {
            var history = tool.GetReasoningHistory($"session-{i}");
            Assert.Single(history);
            Assert.Contains($"Session {i}", history[0].Title);
        }
    }

    [Fact]
    public async Task SameSession_AccumulatesStepsOverTime()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "accumulation-test";

        // Act & Assert
        await tool.ThinkAsync(new ThinkInput(sessionId, "Step 1", "First"));
        Assert.Single(tool.GetReasoningHistory(sessionId));

        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Step 2", "Result", "Analysis"));
        Assert.Equal(2, tool.GetReasoningHistory(sessionId).Count);

        await tool.ThinkAsync(new ThinkInput(sessionId, "Step 3", "Third"));
        Assert.Equal(3, tool.GetReasoningHistory(sessionId).Count);

        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Step 4", "Result", "Analysis"));
        Assert.Equal(4, tool.GetReasoningHistory(sessionId).Count);
    }

    [Fact]
    public async Task NewSession_IsCreatedAutomaticallyOnFirstUse()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "auto-created-session";

        // Act
        var historyBefore = tool.GetReasoningHistory(sessionId);
        await tool.ThinkAsync(new ThinkInput(sessionId, "First Step", "First thought"));
        var historyAfter = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Empty(historyBefore);
        Assert.Single(historyAfter);
    }

    [Fact]
    public async Task SessionHistory_PreservesInsertionOrder()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "order-test";
        var expectedTitles = new List<string>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            var title = $"Step {i}";
            expectedTitles.Add(title);
            
            if (i % 2 == 0)
            {
                await tool.ThinkAsync(new ThinkInput(sessionId, title, $"Thought {i}"));
            }
            else
            {
                await tool.AnalyzeAsync(new AnalyzeInput(sessionId, title, $"Result {i}", $"Analysis {i}"));
            }
        }

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(5, history.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(expectedTitles[i], history[i].Title);
        }
    }

    [Fact]
    public async Task MixedStepTypes_AreCorrectlyStoredInSession()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "mixed-types-test";

        // Act
        await tool.ThinkAsync(new ThinkInput(sessionId, "Think 1", "Thought"));
        await tool.ThinkAsync(new ThinkInput(sessionId, "Think 2", "Thought"));
        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Analyze 1", "Result", "Analysis"));
        await tool.AnalyzeAsync(new AnalyzeInput(sessionId, "Analyze 2", "Result", "Analysis"));
        await tool.ThinkAsync(new ThinkInput(sessionId, "Think 3", "Thought"));

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(5, history.Count);
        Assert.Equal("think", history[0].Type);
        Assert.Equal("think", history[1].Type);
        Assert.Equal("analyze", history[2].Type);
        Assert.Equal("analyze", history[3].Type);
        Assert.Equal("think", history[4].Type);
    }
}
