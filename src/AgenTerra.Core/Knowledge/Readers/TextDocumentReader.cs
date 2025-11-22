using AgenTerra.Core.Knowledge.Models;

namespace AgenTerra.Core.Knowledge.Readers;

/// <summary>
/// Reads plain text and Markdown documents.
/// </summary>
public class TextDocumentReader : IDocumentReader
{
    private static readonly string[] s_supportedExtensions = [".txt", ".md", ".markdown"];

    public IReadOnlyList<string> SupportedExtensions => s_supportedExtensions;

    public async Task<DocumentContent> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new DocumentReadException($"File not found: {filePath}");
        }

        var fileInfo = new FileInfo(filePath);
        var text = await File.ReadAllTextAsync(filePath, cancellationToken);

        return new DocumentContent
        {
            Text = text,
            Source = filePath,
            FileSizeBytes = fileInfo.Length,
            Metadata = new Dictionary<string, object>
            {
                ["Extension"] = fileInfo.Extension,
                ["FileName"] = fileInfo.Name,
                ["LastModified"] = fileInfo.LastWriteTimeUtc
            }
        };
    }

    public async Task<DocumentContent> ReadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken);

        return new DocumentContent
        {
            Text = text,
            Source = fileName,
            FileSizeBytes = stream.Length,
            Metadata = new Dictionary<string, object>
            {
                ["Extension"] = Path.GetExtension(fileName),
                ["FileName"] = fileName
            }
        };
    }
}
