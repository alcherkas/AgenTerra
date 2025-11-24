namespace AgenTerra.Core.Reasoning;

/// <summary>
/// Exception thrown when reasoning operations fail.
/// </summary>
public class ReasoningException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReasoningException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ReasoningException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ReasoningException"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ReasoningException(string message, Exception innerException) 
        : base(message, innerException) { }
}
