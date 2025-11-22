namespace AgenTerra.Core.State.Models;

/// <summary>
/// Represents a workflow session with persistent state across workflow runs.
/// This is an immutable record type that stores session metadata and state dictionary.
/// </summary>
public record WorkflowSession
{
    /// <summary>
    /// Gets the unique identifier for this session.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the dictionary containing all state key-value pairs for this session.
    /// </summary>
    public Dictionary<string, object> SessionState { get; init; }

    /// <summary>
    /// Gets the timestamp when this session was created (in UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when this session was last updated (in UTC).
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Gets the optional workflow identifier associated with this session.
    /// </summary>
    public string? WorkflowId { get; init; }

    /// <summary>
    /// Gets the optional user identifier associated with this session.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowSession"/> record.
    /// Sets default values for SessionState, CreatedAt, and UpdatedAt.
    /// </summary>
    public WorkflowSession()
    {
        SessionState = new Dictionary<string, object>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
