using System.Text;
using AgenTerra.Core.Knowledge.Models;
using AgenTerra.Core.Knowledge.Readers;

namespace AgenTerra.Core.Tests.Knowledge.Readers;

public class TextDocumentReaderTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly TextDocumentReader _reader;

    public TextDocumentReaderTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _reader = new TextDocumentReader();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void SupportedExtensions_ReturnsExpectedExtensions()
    {
        // Arrange & Act
        var extensions = _reader.SupportedExtensions;

        // Assert
        Assert.Contains(".txt", extensions);
        Assert.Contains(".md", extensions);
        Assert.Contains(".markdown", extensions);
        Assert.Equal(3, extensions.Count);
    }

    [Fact]
    public async Task ReadAsync_WithValidTextFile_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        var expectedText = "Hello, World!";
        await File.WriteAllTextAsync(filePath, expectedText);

        // Act
        var result = await _reader.ReadAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedText, result.Text);
        Assert.Equal(filePath, result.Source);
        Assert.True(result.FileSizeBytes > 0);
        Assert.Contains("Extension", result.Metadata.Keys);
        Assert.Contains("FileName", result.Metadata.Keys);
        Assert.Contains("LastModified", result.Metadata.Keys);
    }

    [Fact]
    public async Task ReadAsync_WithValidMarkdownFile_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.md");
        var expectedText = "# Markdown Test\n\nThis is **bold** text.";
        await File.WriteAllTextAsync(filePath, expectedText);

        // Act
        var result = await _reader.ReadAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedText, result.Text);
        Assert.Equal(filePath, result.Source);
        Assert.Equal(".md", result.Metadata["Extension"]);
    }

    [Fact]
    public async Task ReadAsync_WithNonExistentFile_ThrowsDocumentReadException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DocumentReadException>(
            () => _reader.ReadAsync(filePath));
        Assert.Contains("File not found", exception.Message);
        Assert.Contains(filePath, exception.Message);
    }

    [Fact]
    public async Task ReadAsync_WithStream_ReturnsContent()
    {
        // Arrange
        var expectedText = "Stream content test";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedText));
        var fileName = "test.txt";

        // Act
        var result = await _reader.ReadAsync(stream, fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedText, result.Text);
        Assert.Equal(fileName, result.Source);
        Assert.Equal(stream.Length, result.FileSizeBytes);
        Assert.Contains("Extension", result.Metadata.Keys);
        Assert.Contains("FileName", result.Metadata.Keys);
    }

    [Fact]
    public async Task ReadAsync_WithStream_PreservesStreamPosition()
    {
        // Arrange
        var text = "Stream position test";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var initialPosition = stream.Position;

        // Act
        await _reader.ReadAsync(stream, "test.txt");

        // Assert
        // Stream should be at the end after reading, but still accessible
        Assert.True(stream.CanRead);
        Assert.Equal(text.Length, stream.Position);
    }

    [Fact]
    public async Task ReadAsync_WithEmptyFile_ReturnsEmptyContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "empty.txt");
        await File.WriteAllTextAsync(filePath, string.Empty);

        // Act
        var result = await _reader.ReadAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(0, result.FileSizeBytes);
    }

    [Fact]
    public async Task ReadAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        await File.WriteAllTextAsync(filePath, "Test content");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _reader.ReadAsync(filePath, cts.Token));
    }

    [Fact]
    public async Task ReadAsync_WithUtf8Content_PreservesEncoding()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "utf8.txt");
        var expectedText = "Hello 世界 🌍";
        await File.WriteAllTextAsync(filePath, expectedText, Encoding.UTF8);

        // Act
        var result = await _reader.ReadAsync(filePath);

        // Assert
        Assert.Equal(expectedText, result.Text);
    }
}
