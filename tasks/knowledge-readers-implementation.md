# Knowledge Readers Implementation Task

**Priority:** MEDIUM  
**Status:** Not Started  
**Estimated Effort:** 2-3 days  
**Date Created:** November 22, 2025

## Overview

Implement minimal Knowledge Readers feature to parse common document formats and extract text content. This provides foundational document processing capabilities for future RAG (Retrieval-Augmented Generation) and Knowledge Management features, without requiring vector databases or complex indexing initially.

## Implementation Steps

### 1. Define Core Interfaces
**File:** `src/AgenTerra.Core/Knowledge/IDocumentReader.cs`

```csharp
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
```

**Acceptance Criteria:**
- [ ] Interface defined with XML documentation
- [ ] Supports both file path and stream-based reading
- [ ] Extension detection mechanism
- [ ] Async signatures with cancellation support

### 2. Define Document Models
**File:** `src/AgenTerra.Core/Knowledge/Models/DocumentContent.cs`

```csharp
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
```

**Acceptance Criteria:**
- [ ] Immutable record type
- [ ] Required properties enforced
- [ ] Metadata dictionary for extensibility
- [ ] Custom exception type for error handling
- [ ] Timestamps in UTC

### 3. Implement Text Reader (Plain Text & Markdown)
**File:** `src/AgenTerra.Core/Knowledge/Readers/TextDocumentReader.cs`

```csharp
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
```

**Acceptance Criteria:**
- [ ] Supports .txt, .md, .markdown files
- [ ] File existence validation
- [ ] Stream position preserved (leaveOpen: true)
- [ ] Metadata includes file info
- [ ] Proper exception handling

### 4. Implement PDF Reader
**File:** `src/AgenTerra.Core/Knowledge/Readers/PdfDocumentReader.cs`

**NuGet Package:** `PdfPig` (MIT license, actively maintained)

```csharp
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
```

**Acceptance Criteria:**
- [ ] Uses PdfPig library for extraction
- [ ] Extracts text from all pages
- [ ] Includes PDF metadata (version, page count)
- [ ] Handles malformed PDFs gracefully
- [ ] Async execution using Task.Run

### 5. Implement Document Reader Factory
**File:** `src/AgenTerra.Core/Knowledge/DocumentReaderFactory.cs`

```csharp
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
```

**Acceptance Criteria:**
- [ ] Extension-based reader selection
- [ ] Case-insensitive extension matching
- [ ] Custom reader registration support
- [ ] Helpful error messages
- [ ] Thread-safe (if needed, add locking)

### 6. Create Sample Console Application
**File:** `samples/AgenTerra.Sample/Program.cs`

```csharp
using AgenTerra.Core.Knowledge;
using AgenTerra.Core.Knowledge.Readers;

Console.WriteLine("=== Knowledge Readers Sample ===\n");

var factory = new DocumentReaderFactory();

// Display supported formats
Console.WriteLine("Supported formats:");
foreach (var ext in factory.GetSupportedExtensions())
{
    Console.WriteLine($"  - {ext}");
}
Console.WriteLine();

// Sample 1: Read text file
await ReadDocumentSample("sample.txt", factory);

// Sample 2: Read markdown file
await ReadDocumentSample("README.md", factory);

// Sample 3: Read PDF file (if exists)
if (File.Exists("sample.pdf"))
{
    await ReadDocumentSample("sample.pdf", factory);
}

// Sample 4: Check unsupported format
Console.WriteLine("\n--- Testing unsupported format ---");
if (!factory.IsSupported("document.docx"))
{
    Console.WriteLine("✗ .docx format not yet supported");
}

static async Task ReadDocumentSample(string filePath, DocumentReaderFactory factory)
{
    Console.WriteLine($"\n--- Reading: {filePath} ---");
    
    try
    {
        var content = await factory.ReadDocumentAsync(filePath);
        
        Console.WriteLine($"Source: {content.Source}");
        Console.WriteLine($"Size: {content.FileSizeBytes} bytes");
        if (content.PageCount.HasValue)
        {
            Console.WriteLine($"Pages: {content.PageCount}");
        }
        Console.WriteLine($"Text preview: {content.Text[..Math.Min(200, content.Text.Length)]}...");
        Console.WriteLine($"Metadata: {string.Join(", ", content.Metadata.Keys)}");
    }
    catch (DocumentReadException ex)
    {
        Console.WriteLine($"✗ Error: {ex.Message}");
    }
}
```

**Sample Files to Create:**
- `samples/AgenTerra.Sample/sample.txt` - Simple text file
- `samples/AgenTerra.Sample/sample.md` - Markdown file
- `samples/AgenTerra.Sample/sample.pdf` - Optional PDF for testing

**Acceptance Criteria:**
- [ ] Demonstrates all reader types
- [ ] Shows metadata extraction
- [ ] Handles errors gracefully
- [ ] Clear console output
- [ ] Documents supported formats

### 7. Write Unit Tests
**Files:** `tests/AgenTerra.Core.Tests/Knowledge/`

**Test Coverage:**

**TextDocumentReaderTests.cs:**
- [ ] ReadAsync_WithValidTextFile_ReturnsContent
- [ ] ReadAsync_WithValidMarkdownFile_ReturnsContent
- [ ] ReadAsync_WithNonExistentFile_ThrowsDocumentReadException
- [ ] ReadAsync_WithStream_ReturnsContent
- [ ] ReadAsync_WithStream_PreservesStreamPosition

**PdfDocumentReaderTests.cs:**
- [ ] ReadAsync_WithValidPdf_ExtractsText
- [ ] ReadAsync_WithValidPdf_ReturnsPageCount
- [ ] ReadAsync_WithInvalidPdf_ThrowsDocumentReadException
- [ ] ReadAsync_WithMultiPagePdf_ExtractsAllPages
- [ ] ReadAsync_WithStream_ReturnsContent

**DocumentReaderFactoryTests.cs:**
- [ ] GetReader_WithSupportedExtension_ReturnsReader
- [ ] GetReader_WithUnsupportedExtension_ReturnsNull
- [ ] IsSupported_WithSupportedExtension_ReturnsTrue
- [ ] IsSupported_CaseInsensitive_ReturnsTrue
- [ ] RegisterReader_CustomReader_RegistersSuccessfully
- [ ] ReadDocumentAsync_WithUnsupportedFile_ThrowsException
- [ ] GetSupportedExtensions_ReturnsAllExtensions

**Minimum Coverage:** 85%

## Design Decisions

### Reader Selection Strategy
**Decision:** Factory pattern with extension-based selection

**Rationale:**
- Simple and predictable
- Easy to extend with custom readers
- No complex MIME type detection needed
- Standard .NET pattern

**Future Enhancement:** Add MIME type detection for stream-based reading

### PDF Library Choice
**Decision:** Use PdfPig library

**Rationale:**
- MIT license (permissive)
- Pure .NET implementation
- Active maintenance
- Good performance
- No native dependencies

**Alternatives Considered:**
- iTextSharp (AGPL license - restrictive)
- Docnet (requires native libraries)
- PDFsharp (good, but PdfPig has better text extraction)

### Stream Handling
**Decision:** `leaveOpen: true` for streams

**Rationale:**
- Caller controls stream lifetime
- Allows multiple reads if needed
- Standard .NET practice for reader classes

### Synchronous PDF Processing
**Decision:** Use `Task.Run` for PDF operations

**Rationale:**
- PdfPig is synchronous library
- Prevents blocking on I/O thread
- Maintains async API surface
- Allows cancellation support

## Success Criteria

- [ ] All interfaces and models defined
- [ ] Text reader implementation complete
- [ ] PDF reader implementation complete
- [ ] Factory pattern implementation complete
- [ ] Sample application runs successfully
- [ ] Unit tests pass with >85% coverage
- [ ] XML documentation for all public APIs
- [ ] Code follows SOLID principles
- [ ] PdfPig dependency added (no other external deps for now)
- [ ] Builds without warnings on .NET 10.0

## Follow-Up Tasks

After completion, create tasks for:
1. **DOCX Reader** - Microsoft Word document support (using Open XML SDK)
2. **CSV Reader** - Structured data parsing
3. **HTML Reader** - Web content extraction
4. **JSON/XML Readers** - Structured document support
5. **Image OCR Reader** - Text extraction from images (Azure AI Vision)
6. **Document Chunking** - Split documents into manageable pieces for RAG
7. **Knowledge Management Integration** - Connect readers to vector storage

## Dependencies

**NuGet Packages:**
- `PdfPig` (>= 0.1.9) - PDF text extraction

**Required Features:**
- None (foundational feature)

**Enables Features:**
- Knowledge Management (needs document ingestion)
- Vector Database Support (needs content to embed)
- RAG scenarios (needs document parsing)

## Notes

### Minimal Scope - Phase 1
- **Include:** Text, Markdown, PDF
- **Exclude:** DOCX, CSV, HTML, Images (Phase 2)

### Performance Considerations
- Large PDF files may take time to process
- Consider streaming/chunking for very large documents in Phase 2
- Memory usage for PDF text extraction should be monitored

### Error Handling
- All exceptions wrapped in `DocumentReadException`
- Preserve inner exceptions for debugging
- Provide helpful error messages with file paths

### Extension Points
- `IDocumentReader` interface allows custom implementations
- Factory pattern supports runtime registration
- Metadata dictionary extensible per reader type

## References

- Gap Analysis: `/research/gaps.md` - Section "Additional Research Notes" #8 Knowledge Readers
- Agno Reference: https://github.com/agno-agi/agno - `agno/knowledge/readers/` directory
- PdfPig: https://github.com/UglyToad/PdfPig
- Agent Guidelines: `/agents/CSharpExpert.agent.md`
