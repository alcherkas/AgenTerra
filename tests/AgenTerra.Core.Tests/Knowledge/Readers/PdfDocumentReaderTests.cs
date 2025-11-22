using System.Text;
using AgenTerra.Core.Knowledge.Models;
using AgenTerra.Core.Knowledge.Readers;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace AgenTerra.Core.Tests.Knowledge.Readers;

public class PdfDocumentReaderTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly PdfDocumentReader _reader;

    public PdfDocumentReaderTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _reader = new PdfDocumentReader();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void SupportedExtensions_ReturnsPdfExtension()
    {
        // Arrange & Act
        var extensions = _reader.SupportedExtensions;

        // Assert
        Assert.Contains(".pdf", extensions);
        Assert.Single(extensions);
    }

    [Fact]
    public async Task ReadAsync_WithValidPdf_ExtractsText()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.pdf");
        var expectedText = "Hello from PDF!";
        CreateSimplePdf(filePath, expectedText);

        // Act
        var result = await _reader.ReadAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedText, result.Text);
        Assert.Equal(filePath, result.Source);
        Assert.True(result.FileSizeBytes > 0);
    }

    [Fact]
    public async Task ReadAsync_WithValidPdf_ReturnsPageCount()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.pdf");
        CreateSimplePdf(filePath, "Test content");

        // Act
        var result = await _reader.ReadAsync(filePath);

        // Assert
        Assert.NotNull(result.PageCount);
        Assert.Equal(1, result.PageCount.Value);
        Assert.Contains("PageCount", result.Metadata.Keys);
        Assert.Equal(1, result.Metadata["PageCount"]);
    }

    [Fact]
    public async Task ReadAsync_WithValidPdf_IncludesPdfMetadata()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.pdf");
        CreateSimplePdf(filePath, "Test content");

        // Act
        var result = await _reader.ReadAsync(filePath);

        // Assert
        Assert.Contains("Extension", result.Metadata.Keys);
        Assert.Contains("FileName", result.Metadata.Keys);
        Assert.Contains("PdfVersion", result.Metadata.Keys);
        Assert.Contains("PageCount", result.Metadata.Keys);
        Assert.Equal(".pdf", result.Metadata["Extension"]);
    }

    [Fact]
    public async Task ReadAsync_WithNonExistentFile_ThrowsDocumentReadException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.pdf");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DocumentReadException>(
            () => _reader.ReadAsync(filePath));
        Assert.Contains("File not found", exception.Message);
        Assert.Contains(filePath, exception.Message);
    }

    [Fact]
    public async Task ReadAsync_WithInvalidPdf_ThrowsDocumentReadException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "invalid.pdf");
        await File.WriteAllTextAsync(filePath, "This is not a valid PDF file");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DocumentReadException>(
            () => _reader.ReadAsync(filePath));
        Assert.Contains("Failed to read PDF", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public async Task ReadAsync_WithMultiPagePdf_ExtractsAllPages()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "multipage.pdf");
        CreateMultiPagePdf(filePath, ["Page 1 content", "Page 2 content", "Page 3 content"]);

        // Act
        var result = await _reader.ReadAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.PageCount);
        Assert.Equal(3, result.PageCount.Value);
        Assert.Contains("Page 1 content", result.Text);
        Assert.Contains("Page 2 content", result.Text);
        Assert.Contains("Page 3 content", result.Text);
    }

    [Fact]
    public async Task ReadAsync_WithStream_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.pdf");
        var expectedText = "Stream PDF test";
        CreateSimplePdf(filePath, expectedText);
        
        using var stream = File.OpenRead(filePath);
        var fileName = "test.pdf";

        // Act
        var result = await _reader.ReadAsync(stream, fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedText, result.Text);
        Assert.Equal(fileName, result.Source);
        Assert.Equal(stream.Length, result.FileSizeBytes);
    }

    [Fact]
    public async Task ReadAsync_WithStreamAndInvalidPdf_ThrowsDocumentReadException()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid PDF content"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DocumentReadException>(
            () => _reader.ReadAsync(stream, "test.pdf"));
        Assert.Contains("Failed to read PDF from stream", exception.Message);
    }

    [Fact]
    public async Task ReadAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.pdf");
        CreateSimplePdf(filePath, "Test content");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _reader.ReadAsync(filePath, cts.Token));
    }

    private static void CreateSimplePdf(string filePath, string text)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        page.AddText(text, 12, new PdfPoint(50, 700), font);
        var pdfBytes = builder.Build();
        File.WriteAllBytes(filePath, pdfBytes);
    }

    private static void CreateMultiPagePdf(string filePath, string[] pageTexts)
    {
        var builder = new PdfDocumentBuilder();
        foreach (var text in pageTexts)
        {
            var page = builder.AddPage(PageSize.A4);
            var font = builder.AddStandard14Font(Standard14Font.Helvetica);
            page.AddText(text, 12, new PdfPoint(50, 700), font);
        }
        var pdfBytes = builder.Build();
        File.WriteAllBytes(filePath, pdfBytes);
    }
}
