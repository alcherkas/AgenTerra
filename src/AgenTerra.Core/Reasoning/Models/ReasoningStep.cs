namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Represents a single step in the reasoning process.
/// </summary>
public record ReasoningStep
{
    /// <summary>
    /// Gets the type of step ("think" or "analyze").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets a brief title describing this step.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the content of the reasoning step.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the confidence level in this step (0.0 to 1.0).
    /// </summary>
    public required double Confidence { get; init; }

    /// <summary>
    /// Gets the timestamp when this step was recorded.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}
