using AgenTerra.Core.State.Models;

namespace AgenTerra.Core.State;

/// <summary>
/// Defines the contract for workflow session state storage and management.
/// Provides methods to store, retrieve, and manage workflow session state across runs.
/// </summary>
public interface IWorkflowStateStore
{
    /// <summary>
    /// Retrieves a workflow session by its unique identifier.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the workflow session if found; otherwise, null.
    /// </returns>
    Task<WorkflowSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates a workflow session in the store.
    /// </summary>
    /// <param name="session">The workflow session to save or update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveSessionAsync(WorkflowSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific state value from a session.
    /// </summary>
    /// <typeparam name="T">The type of the state value to retrieve.</typeparam>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="key">The key identifying the state value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the state value if found; otherwise, the default value for type T.
    /// </returns>
    Task<T?> GetStateAsync<T>(string sessionId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a specific state value in a session, creating the session if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the state value to set.</typeparam>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="key">The key identifying the state value.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous set operation.</returns>
    Task SetStateAsync<T>(string sessionId, string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workflow session from the store.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result is true if the session was deleted; false if it didn't exist.
    /// </returns>
    Task<bool> DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active session identifiers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only list of all session identifiers.
    /// </returns>
    Task<IReadOnlyList<string>> GetAllSessionIdsAsync(CancellationToken cancellationToken = default);
}
