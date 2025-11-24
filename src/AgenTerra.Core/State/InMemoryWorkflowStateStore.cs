using AgenTerra.Core.State.Models;

namespace AgenTerra.Core.State;

/// <summary>
/// In-memory implementation of <see cref="IWorkflowStateStore"/>.
/// Stores workflow sessions in memory using a thread-safe dictionary.
/// Suitable for single-instance deployments and testing scenarios.
/// </summary>
public class InMemoryWorkflowStateStore : IWorkflowStateStore, IDisposable
{
    private readonly Dictionary<string, WorkflowSession> _sessions = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

    /// <inheritdoc/>
    public async Task<WorkflowSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _sessions.TryGetValue(sessionId, out var session)
                ? session with { } // Return a copy
                : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task SaveSessionAsync(WorkflowSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var updatedSession = session with { UpdatedAt = DateTime.UtcNow };
            _sessions[session.SessionId] = updatedSession;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetStateAsync<T>(string sessionId, string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(key);

        var session = await GetSessionAsync(sessionId, cancellationToken);
        if (session?.SessionState.TryGetValue(key, out var value) == true)
        {
            return (T?)value;
        }
        return default;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method allows storing null values intentionally when T is a nullable type.
    /// The null-forgiving operator is used because the Dictionary requires non-nullable object values,
    /// but the actual value can be null when T is nullable.
    /// </remarks>
    public async Task SetStateAsync<T>(string sessionId, string key, T value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(key);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                session = new WorkflowSession
                {
                    SessionId = sessionId,
                    SessionState = new Dictionary<string, object>()
                };
            }

            var newState = new Dictionary<string, object>(session.SessionState)
            {
                [key] = value!
            };

            var updatedSession = session with
            {
                SessionState = newState,
                UpdatedAt = DateTime.UtcNow
            };

            _sessions[sessionId] = updatedSession;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _sessions.Remove(sessionId);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetAllSessionIdsAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _sessions.Keys.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Disposes the resources used by the InMemoryWorkflowStateStore.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _lock.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
        GC.SuppressFinalize(this);
    }
}
