namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Defines the contract for reasoning tools that provide step-by-step thinking and analysis capabilities.
/// </summary>
public interface IReasoningTool
{
    /// <summary>
    /// Records a thinking step with a thought and optional action.
    /// </summary>
    /// <param name="input">The thinking input containing the session ID, title, thought, and optional action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A formatted string response suitable for LLM consumption.</returns>
    Task<string> ThinkAsync(ThinkInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an analysis step with a result and analysis of the next action.
    /// </summary>
    /// <param name="input">The analysis input containing the session ID, title, result, analysis, and next action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A formatted string response suitable for LLM consumption.</returns>
    Task<string> AnalyzeAsync(AnalyzeInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the complete reasoning history for a specific session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <returns>An immutable list of reasoning steps for the session.</returns>
    IReadOnlyList<ReasoningStep> GetReasoningHistory(string sessionId);
}

/// <summary>
/// Specifies the next action to take in the reasoning process.
/// </summary>
public enum NextAction
{
    /// <summary>
    /// Continue with more reasoning steps.
    /// </summary>
    Continue,

    /// <summary>
    /// Validate the current reasoning before proceeding.
    /// </summary>
    Validate,

    /// <summary>
    /// Provide the final answer or conclusion.
    /// </summary>
    FinalAnswer
}
