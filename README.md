# TOON Tokenizer for C#

A comprehensive C# library for tokenizing and parsing the **TOON (Token-Oriented Object Notation)** language. This library provides full lexical analysis, AST generation, and utilities for building language support tools like syntax highlighters for Visual Studio extensions.

## Overview

TOON is a compact, human-readable encoding of the JSON data model designed for LLM prompts. It provides lossless serialization with minimized tokens and easy-to-follow structure.

This library is built for **.NET Standard 2.1** and provides:
- ✅ Complete lexical analyzer (tokenizer)
- ✅ Full AST (Abstract Syntax Tree) parser
- ✅ Position tracking for every token and AST node
- ✅ Utilities for syntax highlighting
- ✅ Visitor pattern for AST traversal
- ✅ Extension methods for common operations

## Installation

Add the ToonTokenizer project to your solution or reference the compiled DLL.

```bash
dotnet add reference path/to/ToonTokenizer.csproj
```

## Quick Start

### Basic Parsing

```csharp
using ToonTokenizer;
using ToonTokenizer.Ast;

string toonSource = @"
name: John Doe
age: 30
active: true
";

// Parse TOON text into an AST
var document = Toon.Parse(toonSource);

// Access properties
foreach (var property in document.Properties)
{
    Console.WriteLine($"{property.Key}: {property.Value}");
}
```

### Tokenization Only

```csharp
using ToonTokenizer;

string toonSource = "name: John Doe";

// Get all tokens
var tokens = Toon.Tokenize(toonSource);

foreach (var token in tokens)
{
    Console.WriteLine($"{token.Type}: {token.Value} at line {token.Line}, col {token.Column}");
}
```

### Validation

```csharp
using ToonTokenizer;

string toonSource = "...";

if (Toon.TryParse(toonSource, out var errors))
{
    Console.WriteLine("Valid TOON!");
}
else
{
    Console.WriteLine("Errors:");
    foreach (var error in errors)
    {
        Console.WriteLine($"  - {error}");
    }
}
```

## TOON Language Features

### Simple Properties

```toon
name: John Doe
age: 30
active: true
isNull: null
```

### Nested Objects

```toon
user:
  name: Jane Smith
  email: jane@example.com
  settings:
    theme: dark
    notifications: true
```

### Simple Arrays

```toon
colors[3]: red,green,blue
numbers[4]: 1,2,3,4
```

### Table Arrays (with Schema)

```toon
users[3]{id,name,age}:
  1,Alice,25
  2,Bob,30
  3,Charlie,35
```

### Complex Example

```toon
context:
  task: Our favorite hikes together
  location: Boulder
  season: spring_2025

friends[3]: ana,luis,sam

hikes[3]{id,name,distanceKm,elevationGain,companion,wasSunny}:
  1,Blue Lake Trail,7.5,320,ana,true
  2,Ridge Overlook,9.2,540,luis,false
  3,Wildflower Loop,5.1,180,sam,true
```

## API Reference

### Core Classes

#### `Toon` (Main Entry Point)

```csharp
// Parse TOON text into an AST
ToonDocument Parse(string source)

// Tokenize TOON text
List<Token> Tokenize(string source)

// Validate TOON text
bool TryParse(string source, out List<string> errors)
```

#### `ToonLexer`

```csharp
// Create a lexer for the given source
ToonLexer(string source)

// Get all tokens
List<Token> Tokenize()

// Get the next token
Token NextToken()
```

#### `ToonParser`

```csharp
// Create a parser for the given tokens
ToonParser(List<Token> tokens)

// Parse tokens into an AST
ToonDocument Parse()
```

### Token Types

```csharp
public enum TokenType
{
    // Literals
    String, Number, True, False, Null,
    
    // Identifiers
    Identifier,
    
    // Structural
    Colon, Comma,
    LeftBracket, RightBracket,
    LeftBrace, RightBrace,
    
    // Formatting
    Newline, Indent, Dedent, Whitespace,
    
    // Special
    Comment, EndOfFile, Invalid
}
```

### AST Node Types

All AST nodes inherit from `AstNode` and include position tracking:

- **`ToonDocument`**: Root document containing properties
- **`PropertyNode`**: Key-value pair (property)
- **`ObjectNode`**: Nested object with properties
- **`ArrayNode`**: Simple array with elements
- **`TableArrayNode`**: Table-style array with schema and rows
- **`StringValueNode`**: String literal
- **`NumberValueNode`**: Numeric literal (integer or float)
- **`BooleanValueNode`**: Boolean literal (true/false)
- **`NullValueNode`**: Null literal

### Position Tracking

Every `Token` includes:
```csharp
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Line { get; }        // 1-based
    public int Column { get; }      // 1-based
    public int Position { get; }    // 0-based absolute position
    public int Length { get; }
}
```

Every `AstNode` includes:
```csharp
public abstract class AstNode
{
    public int StartLine { get; }
    public int StartColumn { get; }
    public int EndLine { get; }
    public int EndColumn { get; }
    public int StartPosition { get; }
    public int EndPosition { get; }
}
```

### Syntax Highlighting Extensions

```csharp
using ToonTokenizer;

var tokens = Toon.Tokenize(source);

// Get tokens on a specific line
var lineTokens = tokens.GetTokensOnLine(5);

// Get token at cursor position
var token = tokens.GetTokenAt(line: 3, column: 10);

// Get tokens by type
var strings = tokens.GetTokensByType(TokenType.String);

// Get classification for syntax highlighting
foreach (var token in tokens)
{
    string classification = token.GetClassification();
    // Returns: "string", "number", "keyword.boolean", "punctuation", etc.
}

// Check token category
bool isKeyword = token.IsKeyword();
bool isStructural = token.IsStructural();
bool isValue = token.IsValue();
```

### AST Extensions

```csharp
using ToonTokenizer;
using ToonTokenizer.Ast;

var document = Toon.Parse(source);

// Get all properties (including nested)
var allProperties = document.GetAllProperties();

// Find property by path
var property = document.FindProperty("user.settings.theme");

// Get nesting depth
int depth = property.GetDepth();

// Debug output
string debugString = document.ToDebugString();
Console.WriteLine(debugString);
```

## Visitor Pattern

Implement custom AST visitors:

```csharp
using ToonTokenizer.Ast;

public class MyVisitor : IAstVisitor<string>
{
    public string VisitDocument(ToonDocument node)
    {
        // Process document
        return "Document";
    }
    
    public string VisitProperty(PropertyNode node)
    {
        // Process property
        return $"Property: {node.Key}";
    }
    
    // Implement other Visit methods...
}

// Use the visitor
var document = Toon.Parse(source);
var result = document.Accept(new MyVisitor());
```

## Use Cases

### 1. Syntax Highlighter for Visual Studio

```csharp
public IEnumerable<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
{
    var source = span.GetText();
    var tokens = Toon.Tokenize(source);
    
    foreach (var token in tokens)
    {
        var classification = token.GetClassification();
        var tokenSpan = new SnapshotSpan(span.Snapshot, 
            token.Position, token.Length);
        
        yield return new ClassificationSpan(tokenSpan, 
            GetClassificationType(classification));
    }
}
```

### 2. IntelliSense / Autocomplete

```csharp
public IEnumerable<Completion> GetCompletions(int position)
{
    var tokens = Toon.Tokenize(source);
    var token = tokens.GetTokenAt(line, column);
    
    if (token?.Type == TokenType.Identifier)
    {
        // Provide property name suggestions
    }
    else if (token?.Type == TokenType.Colon)
    {
        // Provide value suggestions
    }
}
```

### 3. Code Folding

```csharp
public IEnumerable<NewRegion> GetRegions()
{
    var document = Toon.Parse(source);
    
    foreach (var property in document.Properties)
    {
        if (property.Value is ObjectNode obj)
        {
            yield return new NewRegion
            {
                StartLine = obj.StartLine,
                EndLine = obj.EndLine
            };
        }
    }
}
```

### 4. Error Reporting

```csharp
try
{
    var document = Toon.Parse(source);
}
catch (ParseException ex)
{
    // ex.Message contains line/column information
    ShowError(ex.Message);
}
```

## Examples

See the `ToonTokenizer.Examples` namespace for complete examples:

```csharp
using ToonTokenizer.Examples;

ToonExamples.RunExamples();
```

## Specification

This implementation is based on the TOON specification v3.0:
https://github.com/toon-format/spec

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing patterns
- All public APIs are documented
- Build succeeds without warnings

## License

MIT License - See the TOON specification repository for details.

## Links

- **TOON Specification**: https://github.com/toon-format/spec
- **Reference Implementation**: https://github.com/toon-format/toon
- **Community**: https://github.com/toon-format

## Version

Current version: 1.0.0
Compatible with TOON Spec v3.0
Target: .NET Standard 2.1
