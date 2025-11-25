# Accessing Tokens from Parse Result

As of this update, the `ToonParseResult` now includes the tokens generated during lexical analysis. This allows you to access both the parsed AST and the raw token stream in a single API call.

## Example Usage

```csharp
using ToonTokenizer;

var source = "name: John\nage: 30";
var result = Toon.Parse(source);

// Access the parsed document (AST)
Console.WriteLine($"Properties: {result.Document.Properties.Count}");

// Access the tokens
Console.WriteLine($"Tokens: {result.Tokens.Count}");
foreach (var token in result.Tokens)
{
    Console.WriteLine($"  {token.Type}: '{token.Value}' at line {token.Line}");
}
```

## Benefits

1. **Single API Call** - No need to call both `Parse()` and `Tokenize()`
2. **Guaranteed Consistency** - Tokens and AST come from the same parse run
3. **Better for Tooling** - IDEs, syntax highlighters, and formatters can access everything
4. **Error Correlation** - You can map AST nodes back to their source tokens

## Use Cases

- **Syntax Highlighting**: Iterate over tokens to colorize source code
- **IDE Features**: Provide hover information, diagnostics, and code completion
- **Error Reporting**: Show precise locations using token positions
- **Source Mapping**: Map AST nodes back to original source text
- **Code Formatting**: Preserve whitespace and comments during reformatting

## Example: Syntax Highlighting

```csharp
var result = Toon.Parse(source);

foreach (var token in result.Tokens)
{
    switch (token.Type)
    {
        case TokenType.Identifier:
            Console.ForegroundColor = ConsoleColor.Cyan;
            break;
        case TokenType.String:
            Console.ForegroundColor = ConsoleColor.Green;
            break;
        case TokenType.Number:
            Console.ForegroundColor = ConsoleColor.Yellow;
            break;
        case TokenType.Comment:
            Console.ForegroundColor = ConsoleColor.Gray;
            break;
        default:
            Console.ResetColor();
            break;
    }
    Console.Write(token.Value);
}
Console.ResetColor();
```

## Backward Compatibility

This is a non-breaking change. The `Tokens` property is always populated and initialized to an empty list if no tokens are available.
