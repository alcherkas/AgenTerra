namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Represents a single step in the reasoning process.
/// </summary>
/// <param name="Type">The type of step ("think" or "analyze").</param>
/// <param name="Title">A brief title describing this step.</param>
/// <param name="Content">The content of the reasoning step.</param>
/// <param name="Confidence">Confidence level in this step (0.0 to 1.0).</param>
/// <param name="Timestamp">When this step was recorded.</param>
public record ReasoningStep(
    string Type,
    string Title,
    string Content,
    double Confidence,
    DateTime Timestamp
);
