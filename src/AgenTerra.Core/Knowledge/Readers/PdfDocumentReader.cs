using System.Text;
using AgenTerra.Core.Knowledge.Models;
using UglyToad.PdfPig;

namespace AgenTerra.Core.Knowledge.Readers;

/// <summary>
/// Reads PDF documents using PdfPig library.
/// </summary>
public class PdfDocumentReader : IDocumentReader
{
    private static readonly string[] s_supportedExtensions = [".pdf"];

    public IReadOnlyList<string> SupportedExtensions => s_supportedExtensions;

    public async Task<DocumentContent> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new DocumentReadException($"File not found: {filePath}");
        }

        return await Task.Run(() =>
        {
            try
            {
                using var document = PdfDocument.Open(filePath);
                var text = ExtractText(document);
                var fileInfo = new FileInfo(filePath);

                return new DocumentContent
                {
                    Text = text,
                    Source = filePath,
                    PageCount = document.NumberOfPages,
                    FileSizeBytes = fileInfo.Length,
                    Metadata = new Dictionary<string, object>
                    {
                        ["Extension"] = ".pdf",
                        ["FileName"] = fileInfo.Name,
                        ["PdfVersion"] = document.Version.ToString(),
                        ["PageCount"] = document.NumberOfPages
                    }
                };
            }
            catch (Exception ex)
            {
                throw new DocumentReadException($"Failed to read PDF: {filePath}", ex);
            }
        }, cancellationToken);
    }

    public async Task<DocumentContent> ReadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var document = PdfDocument.Open(stream);
                var text = ExtractText(document);

                return new DocumentContent
                {
                    Text = text,
                    Source = fileName,
                    PageCount = document.NumberOfPages,
                    FileSizeBytes = stream.Length,
                    Metadata = new Dictionary<string, object>
                    {
                        ["Extension"] = ".pdf",
                        ["FileName"] = fileName,
                        ["PdfVersion"] = document.Version.ToString(),
                        ["PageCount"] = document.NumberOfPages
                    }
                };
            }
            catch (Exception ex)
            {
                throw new DocumentReadException($"Failed to read PDF from stream", ex);
            }
        }, cancellationToken);
    }

    private static string ExtractText(PdfDocument document)
    {
        var textBuilder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            textBuilder.AppendLine(page.Text);
        }

        return textBuilder.ToString();
    }
}
