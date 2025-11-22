using AgenTerra.Core.Knowledge;
using AgenTerra.Core.Knowledge.Models;

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
await ReadDocumentSample("sample.md", factory);

// Sample 3: Read PDF file (if exists)
if (File.Exists("sample.pdf"))
{
    await ReadDocumentSample("sample.pdf", factory);
}
else
{
    Console.WriteLine("\n--- sample.pdf not found ---");
    Console.WriteLine("Note: PDF sample file is optional for testing");
}

// Sample 4: Check unsupported format
Console.WriteLine("\n--- Testing unsupported format ---");
if (!factory.IsSupported("document.docx"))
{
    Console.WriteLine("✗ .docx format not yet supported");
}

Console.WriteLine("\n=== Sample completed successfully ===");

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
