using AgenTerra.Core.Reasoning;

namespace AgenTerra.Sample;

/// <summary>
/// Demonstrates reasoning tools with the classic Fox, Chicken, and Grain river crossing puzzle.
/// Shows how to use Think and Analyze operations to solve complex problems step-by-step.
/// </summary>
public class ReasoningToolSample
{
    /// <summary>
    /// Runs the reasoning tool demonstration.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Fox, Chicken, and Grain River Crossing Puzzle ===");
        Console.WriteLine();

        using var reasoningTool = new ReasoningTool();
        var sessionId = Guid.NewGuid().ToString();

        // Step 1: Think - Initial State Analysis
        var response1 = await reasoningTool.ThinkAsync(new ThinkInput(
            SessionId: sessionId,
            Title: "Initial State Analysis",
            Thought: "Man, fox, chicken, grain on left bank. Goal: all on right bank. Boat holds man + one item.",
            Action: "Identify constraints",
            Confidence: 0.9
        ));
        Console.WriteLine(response1);

        // Step 2: Analyze - Constraint Analysis
        var response2 = await reasoningTool.AnalyzeAsync(new AnalyzeInput(
            SessionId: sessionId,
            Title: "Constraint Analysis",
            Result: "Fox eats chicken if alone. Chicken eats grain if alone.",
            Analysis: "Must never leave fox+chicken or chicken+grain together without the man present.",
            NextAction: NextAction.Continue,
            Confidence: 0.95
        ));
        Console.WriteLine(response2);

        // Step 3: Think - First Move Strategy
        var response3 = await reasoningTool.ThinkAsync(new ThinkInput(
            SessionId: sessionId,
            Title: "First Move Strategy",
            Thought: "Chicken is the conflict point. If we take fox first, chicken eats grain. If we take grain first, fox eats chicken.",
            Action: "Take chicken across first",
            Confidence: 0.85
        ));
        Console.WriteLine(response3);

        // Step 4: Analyze - After First Move
        var response4 = await reasoningTool.AnalyzeAsync(new AnalyzeInput(
            SessionId: sessionId,
            Title: "After First Move",
            Result: "Left bank: fox, grain. Right bank: chicken. Boat with man on left.",
            Analysis: "Fox and grain safe together. Can now move either fox or grain.",
            NextAction: NextAction.Continue,
            Confidence: 0.9
        ));
        Console.WriteLine(response4);

        // Step 5: Think - Second Move
        var response5 = await reasoningTool.ThinkAsync(new ThinkInput(
            SessionId: sessionId,
            Title: "Second Move",
            Thought: "Take fox across. But if we leave fox with chicken, fox eats chicken.",
            Action: "Take fox across, bring chicken back",
            Confidence: 0.8
        ));
        Console.WriteLine(response5);

        // Step 6: Analyze - After Second Move
        var response6 = await reasoningTool.AnalyzeAsync(new AnalyzeInput(
            SessionId: sessionId,
            Title: "After Second Move",
            Result: "Left bank: chicken, grain. Right bank: fox. Boat with man on left.",
            Analysis: "Fox is safe alone. Now need to get grain across without leaving it with chicken.",
            NextAction: NextAction.Continue,
            Confidence: 0.9
        ));
        Console.WriteLine(response6);

        // Step 7: Think - Third Move
        var response7 = await reasoningTool.ThinkAsync(new ThinkInput(
            SessionId: sessionId,
            Title: "Third Move",
            Thought: "Take grain across, leave fox and grain together (safe).",
            Action: "Take grain across",
            Confidence: 0.9
        ));
        Console.WriteLine(response7);

        // Step 8: Analyze - After Third Move
        var response8 = await reasoningTool.AnalyzeAsync(new AnalyzeInput(
            SessionId: sessionId,
            Title: "After Third Move",
            Result: "Left bank: chicken. Right bank: fox, grain. Boat with man on right.",
            Analysis: "Fox and grain are safe together. Only chicken remains on left bank.",
            NextAction: NextAction.Continue,
            Confidence: 0.95
        ));
        Console.WriteLine(response8);

        // Step 9: Think - Final Move
        var response9 = await reasoningTool.ThinkAsync(new ThinkInput(
            SessionId: sessionId,
            Title: "Final Move",
            Thought: "Go back empty, get chicken.",
            Action: "Take chicken across",
            Confidence: 1.0
        ));
        Console.WriteLine(response9);

        // Step 10: Analyze - Final State
        var response10 = await reasoningTool.AnalyzeAsync(new AnalyzeInput(
            SessionId: sessionId,
            Title: "Final State",
            Result: "Left bank: empty. Right bank: fox, chicken, grain, man.",
            Analysis: "All items successfully transported. Puzzle solved!",
            NextAction: NextAction.FinalAnswer,
            Confidence: 1.0
        ));
        Console.WriteLine(response10);

        // Display complete reasoning history
        Console.WriteLine("\n=== Complete Reasoning History ===\n");
        var history = reasoningTool.GetReasoningHistory(sessionId);
        for (int i = 0; i < history.Count; i++)
        {
            var step = history[i];
            Console.WriteLine($"Step {i + 1}: [{step.Type.ToUpper()}] {step.Title}");
            Console.WriteLine($"Confidence: {step.Confidence:F2}");
            Console.WriteLine($"Timestamp: {step.Timestamp:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine(step.Content);
            Console.WriteLine();
        }
    }
}
