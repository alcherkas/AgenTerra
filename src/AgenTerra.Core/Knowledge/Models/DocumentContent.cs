namespace AgenTerra.Core.Knowledge.Models;

/// <summary>
/// Represents the content extracted from a document.
/// </summary>
public record DocumentContent
{
    /// <summary>
    /// Gets the extracted text content.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the source file name or path.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets the document metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets the number of pages (if applicable).
    /// </summary>
    public int? PageCount { get; init; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long? FileSizeBytes { get; init; }

    /// <summary>
    /// Gets the timestamp when the content was extracted.
    /// </summary>
    public DateTime ExtractedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Exception thrown when document reading fails.
/// </summary>
public class DocumentReadException : Exception
{
    public DocumentReadException(string message) : base(message) { }
    public DocumentReadException(string message, Exception innerException) 
        : base(message, innerException) { }
}
