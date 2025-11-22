# Sample Markdown Document

This is a sample **Markdown** file for testing the Knowledge Readers feature.

## Features

The TextDocumentReader supports:
- `.md` files
- `.markdown` files
- All standard Markdown syntax

## Benefits

1. **Easy to read** - Clean text extraction
2. **Metadata support** - File information captured
3. **Extensible** - Ready for future enhancements

## Code Example

```csharp
var factory = new DocumentReaderFactory();
var content = await factory.ReadDocumentAsync("sample.md");
Console.WriteLine(content.Text);
```

> Note: This is a minimal implementation focusing on text extraction.

---

*Sample document for AgenTerra Knowledge Readers*
