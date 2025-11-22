using System.Text;

namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Provides step-by-step reasoning capabilities with in-memory session storage.
/// This implementation is thread-safe and supports multiple concurrent sessions.
/// </summary>
public class ReasoningTool : IReasoningTool
{
    private readonly Dictionary<string, List<ReasoningStep>> _sessions = new();
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task<string> ThinkAsync(ThinkInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        if (string.IsNullOrWhiteSpace(input.SessionId))
        {
            throw new ArgumentException("SessionId cannot be null or whitespace.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new ArgumentException("Title cannot be null or whitespace.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.Thought))
        {
            throw new ArgumentException("Thought cannot be null or whitespace.", nameof(input));
        }

        var content = new StringBuilder();
        content.AppendLine($"Thought: {input.Thought}");
        if (!string.IsNullOrWhiteSpace(input.Action))
        {
            content.AppendLine($"Action: {input.Action}");
        }

        var step = new ReasoningStep(
            Type: "think",
            Title: input.Title,
            Content: content.ToString().TrimEnd(),
            Confidence: input.Confidence,
            Timestamp: DateTime.UtcNow
        );

        lock (_lock)
        {
            if (!_sessions.TryGetValue(input.SessionId, out var steps))
            {
                steps = new List<ReasoningStep>();
                _sessions[input.SessionId] = steps;
            }
            steps.Add(step);
        }

        var response = new StringBuilder();
        response.AppendLine($"[THINK] {input.Title}");
        response.AppendLine($"Confidence: {input.Confidence:F2}");
        response.AppendLine(content.ToString());
        response.AppendLine($"Recorded at: {step.Timestamp:O}");

        return Task.FromResult(response.ToString());
    }

    /// <inheritdoc />
    public Task<string> AnalyzeAsync(AnalyzeInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        
        if (string.IsNullOrWhiteSpace(input.SessionId))
        {
            throw new ArgumentException("SessionId cannot be null or whitespace.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new ArgumentException("Title cannot be null or whitespace.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.Result))
        {
            throw new ArgumentException("Result cannot be null or whitespace.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.Analysis))
        {
            throw new ArgumentException("Analysis cannot be null or whitespace.", nameof(input));
        }

        var content = new StringBuilder();
        content.AppendLine($"Result: {input.Result}");
        content.AppendLine($"Analysis: {input.Analysis}");
        content.AppendLine($"Next Action: {input.NextAction}");

        var step = new ReasoningStep(
            Type: "analyze",
            Title: input.Title,
            Content: content.ToString().TrimEnd(),
            Confidence: input.Confidence,
            Timestamp: DateTime.UtcNow
        );

        lock (_lock)
        {
            if (!_sessions.TryGetValue(input.SessionId, out var steps))
            {
                steps = new List<ReasoningStep>();
                _sessions[input.SessionId] = steps;
            }
            steps.Add(step);
        }

        var response = new StringBuilder();
        response.AppendLine($"[ANALYZE] {input.Title}");
        response.AppendLine($"Confidence: {input.Confidence:F2}");
        response.AppendLine(content.ToString());
        response.AppendLine($"Recorded at: {step.Timestamp:O}");

        return Task.FromResult(response.ToString());
    }

    /// <inheritdoc />
    public IReadOnlyList<ReasoningStep> GetReasoningHistory(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("SessionId cannot be null or whitespace.", nameof(sessionId));
        }

        lock (_lock)
        {
            if (_sessions.TryGetValue(sessionId, out var steps))
            {
                return steps.AsReadOnly();
            }
            return Array.Empty<ReasoningStep>();
        }
    }
}
