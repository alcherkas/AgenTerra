using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Represents input for an analysis step in the reasoning process.
/// </summary>
public record AnalyzeInput
{
    /// <summary>
    /// Gets the unique identifier of the reasoning session.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets a brief title describing this analysis step.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the result or observation being analyzed.
    /// </summary>
    public required string Result { get; init; }

    /// <summary>
    /// Gets the analysis of the result.
    /// </summary>
    public required string Analysis { get; init; }

    /// <summary>
    /// Gets the recommended next action in the reasoning process.
    /// </summary>
    public NextAction NextAction { get; init; } = NextAction.Continue;

    /// <summary>
    /// Gets the confidence level in this analysis (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; } = 0.8;

    /// <summary>
    /// Initializes a new instance of the AnalyzeInput record (for backward compatibility).
    /// </summary>
    public AnalyzeInput()
    {
    }

    /// <summary>
    /// Initializes a new instance of the AnalyzeInput record with positional parameters (for backward compatibility).
    /// </summary>
    [SetsRequiredMembers]
    public AnalyzeInput(string sessionId, string title, string result, string analysis, NextAction nextAction = NextAction.Continue, double confidence = 0.8)
    {
        SessionId = sessionId;
        Title = title;
        Result = result;
        Analysis = analysis;
        NextAction = nextAction;
        Confidence = confidence;
    }
}
