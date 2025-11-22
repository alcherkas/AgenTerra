using AgenTerra.Core.Reasoning;

namespace AgenTerra.Core.Tests.Reasoning;

public class ThreadSafetyTests
{
    [Fact]
    public async Task ConcurrentThinkCalls_ToSameSession_AreThreadSafe()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "concurrent-think-test";
        var taskCount = 100;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < taskCount; i++)
        {
            var index = i;
            var task = tool.ThinkAsync(new ThinkInput(sessionId, $"Concurrent Think {index}", $"Thought {index}"
            ));
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(taskCount, history.Count);
        Assert.All(history, step => Assert.Equal("think", step.Type));
    }

    [Fact]
    public async Task ConcurrentAnalyzeCalls_ToSameSession_AreThreadSafe()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "concurrent-analyze-test";
        var taskCount = 100;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < taskCount; i++)
        {
            var index = i;
            var task = tool.AnalyzeAsync(new AnalyzeInput(sessionId, $"Concurrent Analyze {index}", $"Result {index}", $"Analysis {index}"
            ));
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(taskCount, history.Count);
        Assert.All(history, step => Assert.Equal("analyze", step.Type));
    }

    [Fact]
    public async Task ConcurrentMixedCalls_ToSameSession_AreThreadSafe()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "concurrent-mixed-test";
        var taskCount = 100;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < taskCount; i++)
        {
            var index = i;
            Task task;
            
            if (i % 2 == 0)
            {
                task = tool.ThinkAsync(new ThinkInput(sessionId, $"Think {index}", $"Thought {index}"
                ));
            }
            else
            {
                task = tool.AnalyzeAsync(new AnalyzeInput(sessionId, $"Analyze {index}", $"Result {index}", $"Analysis {index}"
                ));
            }
            
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        var history = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(taskCount, history.Count);
    }

    [Fact]
    public async Task ConcurrentCallsToMultipleSessions_AreThreadSafe()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionCount = 50;
        var stepsPerSession = 20;
        var tasks = new List<Task>();

        // Act
        for (int s = 0; s < sessionCount; s++)
        {
            var sessionId = $"session-{s}";
            
            for (int i = 0; i < stepsPerSession; i++)
            {
                var index = i;
                var task = tool.ThinkAsync(new ThinkInput(sessionId, $"Step {index}", $"Thought {index}"
                ));
                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);

        // Assert
        for (int s = 0; s < sessionCount; s++)
        {
            var history = tool.GetReasoningHistory($"session-{s}");
            Assert.Equal(stepsPerSession, history.Count);
        }
    }

    [Fact]
    public async Task ConcurrentReads_WhileWriting_AreThreadSafe()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionId = "read-write-test";
        var writeCount = 100;
        var readCount = 100;
        var tasks = new List<Task>();

        // Act - Start concurrent writes
        for (int i = 0; i < writeCount; i++)
        {
            var index = i;
            var task = tool.ThinkAsync(new ThinkInput(sessionId, $"Step {index}", $"Thought {index}"
            ));
            tasks.Add(task);
        }

        // Start concurrent reads
        for (int i = 0; i < readCount; i++)
        {
            var task = Task.Run(() => tool.GetReasoningHistory(sessionId));
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        var finalHistory = tool.GetReasoningHistory(sessionId);

        // Assert
        Assert.Equal(writeCount, finalHistory.Count);
    }

    [Fact]
    public async Task ParallelSessionCreation_IsThreadSafe()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionCount = 100;

        // Act
        var tasks = Enumerable.Range(0, sessionCount)
            .Select(i => tool.ThinkAsync(new ThinkInput($"session-{i}", "Initial Step", "Initial thought"
            )))
            .ToList();

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < sessionCount; i++)
        {
            var history = tool.GetReasoningHistory($"session-{i}");
            Assert.Single(history);
        }
    }

    [Fact]
    public async Task StressTest_ManyOperationsSimultaneously()
    {
        // Arrange
        var tool = new ReasoningTool();
        var sessionCount = 10;
        var operationsPerSession = 50;
        var random = new Random(42); // Fixed seed for reproducibility
        var tasks = new List<Task>();

        // Act
        for (int s = 0; s < sessionCount; s++)
        {
            var sessionId = $"stress-session-{s}";
            
            for (int i = 0; i < operationsPerSession; i++)
            {
                var index = i;
                Task task;
                
                // Randomly choose between think, analyze, or read
                var operation = random.Next(3);
                
                if (operation == 0)
                {
                    task = tool.ThinkAsync(new ThinkInput(sessionId, $"Think {index}", $"Thought {index}"
                    ));
                }
                else if (operation == 1)
                {
                    task = tool.AnalyzeAsync(new AnalyzeInput(sessionId, $"Analyze {index}", $"Result {index}", $"Analysis {index}"
                    ));
                }
                else
                {
                    task = Task.Run(() => tool.GetReasoningHistory(sessionId));
                }
                
                tasks.Add(task);
            }
        }

        // Should not throw any exceptions
        await Task.WhenAll(tasks);

        // Assert - verify all sessions have some data
        for (int s = 0; s < sessionCount; s++)
        {
            var history = tool.GetReasoningHistory($"stress-session-{s}");
            Assert.NotEmpty(history);
        }
    }
}
