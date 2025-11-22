using System.ComponentModel.DataAnnotations;

namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Represents input for an analysis step in the reasoning process.
/// </summary>
/// <param name="SessionId">The unique identifier of the reasoning session.</param>
/// <param name="Title">A brief title describing this analysis step.</param>
/// <param name="Result">The result or observation being analyzed.</param>
/// <param name="Analysis">The analysis of the result.</param>
/// <param name="NextAction">The recommended next action in the reasoning process.</param>
/// <param name="Confidence">Confidence level in this analysis (0.0 to 1.0, default 0.8).</param>
public record AnalyzeInput(
    [Required] string SessionId,
    [Required] string Title,
    [Required] string Result,
    [Required] string Analysis,
    NextAction NextAction = NextAction.Continue,
    double Confidence = 0.8
);
