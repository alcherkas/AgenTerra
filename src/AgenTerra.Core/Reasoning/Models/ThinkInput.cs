using System.Diagnostics.CodeAnalysis;

namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Represents input for a thinking step in the reasoning process.
/// </summary>
public record ThinkInput
{
    /// <summary>
    /// Gets the unique identifier of the reasoning session.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets a brief title describing this thinking step.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the main thought or reasoning content.
    /// </summary>
    public required string Thought { get; init; }

    /// <summary>
    /// Gets an optional action to take based on this thought.
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    /// Gets the confidence level in this thought (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; } = 0.8;

    /// <summary>
    /// Initializes a new instance of the ThinkInput record with positional parameters (for backward compatibility).
    /// </summary>
    [SetsRequiredMembers]
    public ThinkInput(string sessionId, string title, string thought, string? action = null, double confidence = 0.8)
    {
        SessionId = sessionId;
        Title = title;
        Thought = thought;
        Action = action;
        Confidence = confidence;
    }
}
