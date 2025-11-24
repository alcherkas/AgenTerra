namespace AgenTerra.Core.State.Models;

/// <summary>
/// Exception thrown when workflow state operations fail.
/// </summary>
public class WorkflowStateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowStateException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public WorkflowStateException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowStateException"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public WorkflowStateException(string message, Exception innerException) 
        : base(message, innerException) { }
}
