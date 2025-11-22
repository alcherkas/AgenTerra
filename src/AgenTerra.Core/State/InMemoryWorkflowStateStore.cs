using AgenTerra.Core.State.Models;

namespace AgenTerra.Core.State;

/// <summary>
/// In-memory implementation of <see cref="IWorkflowStateStore"/>.
/// Stores workflow sessions in memory using a thread-safe dictionary.
/// Suitable for single-instance deployments and testing scenarios.
/// </summary>
public class InMemoryWorkflowStateStore : IWorkflowStateStore
{
    private readonly Dictionary<string, WorkflowSession> _sessions = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <inheritdoc/>
    public async Task<WorkflowSession?> GetSessionAsync(string sessionId)
    {
        await _lock.WaitAsync();
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
    public async Task SaveSessionAsync(WorkflowSession session)
    {
        await _lock.WaitAsync();
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
    public async Task<T?> GetStateAsync<T>(string sessionId, string key)
    {
        var session = await GetSessionAsync(sessionId);
        if (session?.SessionState.TryGetValue(key, out var value) == true)
        {
            return (T?)value;
        }
        return default;
    }

    /// <inheritdoc/>
    public async Task SetStateAsync<T>(string sessionId, string key, T value)
    {
        await _lock.WaitAsync();
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
    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        await _lock.WaitAsync();
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
    public async Task<IReadOnlyList<string>> GetAllSessionIdsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _sessions.Keys.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }
}
