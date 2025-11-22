using System.ComponentModel.DataAnnotations;

namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Represents input for a thinking step in the reasoning process.
/// </summary>
/// <param name="SessionId">The unique identifier of the reasoning session.</param>
/// <param name="Title">A brief title describing this thinking step.</param>
/// <param name="Thought">The main thought or reasoning content.</param>
/// <param name="Action">An optional action to take based on this thought.</param>
/// <param name="Confidence">Confidence level in this thought (0.0 to 1.0, default 0.8).</param>
public record ThinkInput(
    [Required] string SessionId,
    [Required] string Title,
    [Required] string Thought,
    string? Action = null,
    double Confidence = 0.8
);
