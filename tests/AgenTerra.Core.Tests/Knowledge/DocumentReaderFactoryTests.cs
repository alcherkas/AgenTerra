using AgenTerra.Core.Knowledge;
using AgenTerra.Core.Knowledge.Models;
using AgenTerra.Core.Knowledge.Readers;

namespace AgenTerra.Core.Tests.Knowledge;

public class DocumentReaderFactoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly DocumentReaderFactory _factory;

    public DocumentReaderFactoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _factory = new DocumentReaderFactory();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void Constructor_RegistersDefaultReaders()
    {
        // Arrange & Act
        var extensions = _factory.GetSupportedExtensions();

        // Assert
        Assert.Contains(".txt", extensions);
        Assert.Contains(".md", extensions);
        Assert.Contains(".markdown", extensions);
        Assert.Contains(".pdf", extensions);
        Assert.Equal(4, extensions.Count);
    }

    [Fact]
    public void GetReader_WithSupportedExtension_ReturnsReader()
    {
        // Arrange
        var filePath = "test.txt";

        // Act
        var reader = _factory.GetReader(filePath);

        // Assert
        Assert.NotNull(reader);
        Assert.IsType<TextDocumentReader>(reader);
    }

    [Fact]
    public void GetReader_WithPdfExtension_ReturnsPdfReader()
    {
        // Arrange
        var filePath = "document.pdf";

        // Act
        var reader = _factory.GetReader(filePath);

        // Assert
        Assert.NotNull(reader);
        Assert.IsType<PdfDocumentReader>(reader);
    }

    [Fact]
    public void GetReader_WithUnsupportedExtension_ReturnsNull()
    {
        // Arrange
        var filePath = "document.docx";

        // Act
        var reader = _factory.GetReader(filePath);

        // Assert
        Assert.Null(reader);
    }

    [Fact]
    public void GetReader_WithExtensionOnly_ReturnsReader()
    {
        // Arrange
        var extension = ".md";

        // Act
        var reader = _factory.GetReader(extension);

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void GetReader_CaseInsensitive_ReturnsReader()
    {
        // Arrange & Act
        var readerLower = _factory.GetReader("test.txt");
        var readerUpper = _factory.GetReader("test.TXT");
        var readerMixed = _factory.GetReader("test.TxT");

        // Assert
        Assert.NotNull(readerLower);
        Assert.NotNull(readerUpper);
        Assert.NotNull(readerMixed);
    }

    [Fact]
    public void IsSupported_WithSupportedExtension_ReturnsTrue()
    {
        // Arrange
        var filePath = "document.pdf";

        // Act
        var result = _factory.IsSupported(filePath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSupported_WithUnsupportedExtension_ReturnsFalse()
    {
        // Arrange
        var filePath = "document.docx";

        // Act
        var result = _factory.IsSupported(filePath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSupported_CaseInsensitive_ReturnsTrue()
    {
        // Arrange & Act
        var resultLower = _factory.IsSupported("test.txt");
        var resultUpper = _factory.IsSupported("test.TXT");
        var resultMixed = _factory.IsSupported("test.Pdf");

        // Assert
        Assert.True(resultLower);
        Assert.True(resultUpper);
        Assert.True(resultMixed);
    }

    [Fact]
    public void RegisterReader_CustomReader_RegistersSuccessfully()
    {
        // Arrange
        var customReader = new CustomTestReader();

        // Act
        _factory.RegisterReader(customReader);

        // Assert
        Assert.True(_factory.IsSupported(".custom"));
        var reader = _factory.GetReader("file.custom");
        Assert.NotNull(reader);
        Assert.IsType<CustomTestReader>(reader);
    }

    [Fact]
    public void RegisterReader_OverwritesExistingReader()
    {
        // Arrange
        var customReader = new CustomTestReader([".txt"]);
        var originalReader = _factory.GetReader(".txt");

        // Act
        _factory.RegisterReader(customReader);
        var newReader = _factory.GetReader(".txt");

        // Assert
        Assert.NotNull(originalReader);
        Assert.NotNull(newReader);
        Assert.NotEqual(originalReader.GetType(), newReader.GetType());
        Assert.IsType<CustomTestReader>(newReader);
    }

    [Fact]
    public async Task ReadDocumentAsync_WithSupportedFile_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        await File.WriteAllTextAsync(filePath, "Test content");

        // Act
        var result = await _factory.ReadDocumentAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test content", result.Text);
        Assert.Equal(filePath, result.Source);
    }

    [Fact]
    public async Task ReadDocumentAsync_WithUnsupportedFile_ThrowsException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "document.docx");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DocumentReadException>(
            () => _factory.ReadDocumentAsync(filePath));
        Assert.Contains("No reader available", exception.Message);
        Assert.Contains(filePath, exception.Message);
        Assert.Contains("Supported extensions", exception.Message);
    }

    [Fact]
    public async Task ReadDocumentAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        await File.WriteAllTextAsync(filePath, "Test content");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _factory.ReadDocumentAsync(filePath, cts.Token));
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsAllExtensions()
    {
        // Arrange & Act
        var extensions = _factory.GetSupportedExtensions();

        // Assert
        Assert.NotNull(extensions);
        Assert.NotEmpty(extensions);
        Assert.Contains(".txt", extensions);
        Assert.Contains(".md", extensions);
        Assert.Contains(".markdown", extensions);
        Assert.Contains(".pdf", extensions);
    }

    // Custom test reader for testing custom reader registration
    private class CustomTestReader : IDocumentReader
    {
        private readonly string[] _extensions;

        public CustomTestReader(string[]? extensions = null)
        {
            _extensions = extensions ?? [".custom"];
        }

        public IReadOnlyList<string> SupportedExtensions => _extensions;

        public Task<DocumentContent> ReadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DocumentContent
            {
                Text = "Custom content",
                Source = filePath
            });
        }

        public Task<DocumentContent> ReadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DocumentContent
            {
                Text = "Custom content from stream",
                Source = fileName
            });
        }
    }
}
