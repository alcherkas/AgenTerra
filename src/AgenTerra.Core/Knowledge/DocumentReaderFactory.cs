using AgenTerra.Core.Knowledge.Models;
using AgenTerra.Core.Knowledge.Readers;

namespace AgenTerra.Core.Knowledge;

/// <summary>
/// Factory for creating appropriate document readers based on file extension.
/// </summary>
public class DocumentReaderFactory
{
    private readonly Dictionary<string, IDocumentReader> _readers = new();

    public DocumentReaderFactory()
    {
        // Register default readers
        RegisterReader(new TextDocumentReader());
        RegisterReader(new PdfDocumentReader());
    }

    /// <summary>
    /// Registers a custom document reader.
    /// </summary>
    public void RegisterReader(IDocumentReader reader)
    {
        foreach (var extension in reader.SupportedExtensions)
        {
            _readers[extension.ToLowerInvariant()] = reader;
        }
    }

    /// <summary>
    /// Gets a reader for the specified file extension.
    /// </summary>
    public IDocumentReader? GetReader(string filePathOrExtension)
    {
        var extension = Path.GetExtension(filePathOrExtension).ToLowerInvariant();
        return _readers.TryGetValue(extension, out var reader) ? reader : null;
    }

    /// <summary>
    /// Checks if a reader exists for the specified file extension.
    /// </summary>
    public bool IsSupported(string filePathOrExtension)
    {
        var extension = Path.GetExtension(filePathOrExtension).ToLowerInvariant();
        return _readers.ContainsKey(extension);
    }

    /// <summary>
    /// Gets all supported file extensions.
    /// </summary>
    public IReadOnlyList<string> GetSupportedExtensions()
    {
        return _readers.Keys.ToList();
    }

    /// <summary>
    /// Reads a document using the appropriate reader.
    /// </summary>
    public async Task<DocumentContent> ReadDocumentAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        var reader = GetReader(filePath);
        if (reader == null)
        {
            throw new DocumentReadException(
                $"No reader available for file: {filePath}. " +
                $"Supported extensions: {string.Join(", ", GetSupportedExtensions())}");
        }

        return await reader.ReadAsync(filePath, cancellationToken);
    }
}
