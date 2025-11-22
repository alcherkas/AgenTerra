using AgenTerra.Core.Knowledge.Models;

namespace AgenTerra.Core.Knowledge;

/// <summary>
/// Represents a reader that can extract text content from documents.
/// </summary>
public interface IDocumentReader
{
    /// <summary>
    /// Gets the supported file extensions (e.g., ".pdf", ".docx").
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// Reads text content from a file.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The extracted document content.</returns>
    Task<DocumentContent> ReadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads text content from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the document data.</param>
    /// <param name="fileName">The original file name (for extension detection).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The extracted document content.</returns>
    Task<DocumentContent> ReadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}
