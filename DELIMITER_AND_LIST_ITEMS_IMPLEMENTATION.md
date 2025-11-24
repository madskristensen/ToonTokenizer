# Delimiter Scoping and Objects as List Items - Implementation Guide

## Current Status

### ✅ Completed
1. **Delimiter Enum** - Added `Delimiter` enum with Comma, Tab, Pipe options
2. **Pipe TokenType** - Added `TokenType.Pipe` to support pipe delimiter
3. **Lexer Support** - Updated lexer to tokenize pipe character
4. **Parser Infrastructure** - Added `_delimiterStack` to track active delimiters
5. **Delimiter Detection** - Updated `ParseProperty` to detect delimiter in brackets (`[N]`, `[N	]`, `[N|]`)
6. **Schema Parsing** - Updated `ParseSchema` to accept and use delimiter parameter
7. **Delimiter Scope Management** - Push/pop delimiter stack when entering/exiting arrays

### ⚠️ Partially Complete
1. **Inline Array Parsing** - Updated to use active delimiter (needs testing)
2. **Table Row Parsing** - Updated `ParseTableRowFlexible` to use active delimiter (has duplicate code issue)

### ❌ Not Started
1. **ParseExpandedArray** - Method called but doesn't exist yet
2. **Helper Methods** - Missing `IsDelimiterToken` and `CreateValueNode`
3. **Objects as List Items** - Complete §10 implementation
4. **Testing** - No tests for delimiter variations yet

## Required Helper Methods

### 1. IsDelimiterToken Method
```csharp
private bool IsDelimiterToken(TokenType tokenType, Delimiter delimiter)
{
    return delimiter switch
    {
        Delimiter.Comma => tokenType == TokenType.Comma,
        Delimiter.Tab => tokenType == TokenType.Whitespace && CurrentToken.Value == "\t",
        Delimiter.Pipe => tokenType == TokenType.Pipe,
        _ => false
    };
}
```

### 2. CreateValueNode Method
```csharp
private AstNode CreateValueNode(Token token)
{
    switch (token.Type)
    {
        case TokenType.String:
        case TokenType.Identifier:
            return new StringValueNode 
            { 
                Value = token.Value, 
                RawValue = token.Value, 
                StartLine = token.Line, 
                StartColumn = token.Column, 
                StartPosition = token.Position, 
                EndLine = token.Line, 
                EndColumn = token.Column + token.Length, 
                EndPosition = token.Position + token.Length 
            };

        case TokenType.Number:
            string v = token.Value;
            bool isNegative = v.StartsWith("-");
            string digits = isNegative ? v.Substring(1) : v;
            bool isInteger = !v.Contains(".") && !v.Contains("e") && !v.Contains("E");
            
            // Check for forbidden leading zeros
            if (isInteger && digits.Length > 1 && digits.StartsWith("0"))
            {
                // Forbidden leading zeros: treat as string
                return new StringValueNode 
                { 
                    Value = token.Value, 
                    RawValue = token.Value, 
                    StartLine = token.Line, 
                    StartColumn = token.Column, 
                    StartPosition = token.Position, 
                    EndLine = token.Line, 
                    EndColumn = token.Column + token.Length, 
                    EndPosition = token.Position + token.Length 
                };
            }
            
            return new NumberValueNode 
            { 
                Value = double.Parse(token.Value, CultureInfo.InvariantCulture), 
                IsInteger = isInteger,
                RawValue = token.Value, 
                StartLine = token.Line, 
                StartColumn = token.Column, 
                StartPosition = token.Position, 
                EndLine = token.Line, 
                EndColumn = token.Column + token.Length, 
                EndPosition = token.Position + token.Length 
            };

        case TokenType.True:
        case TokenType.False:
            return new BooleanValueNode 
            { 
                Value = token.Type == TokenType.True, 
                RawValue = token.Value, 
                StartLine = token.Line, 
                StartColumn = token.Column, 
                StartPosition = token.Position, 
                EndLine = token.Line, 
                EndColumn = token.Column + token.Length, 
                EndPosition = token.Position + token.Length 
            };

        case TokenType.Null:
            return new NullValueNode 
            { 
                RawValue = token.Value, 
                StartLine = token.Line, 
                StartColumn = token.Column, 
                StartPosition = token.Position, 
                EndLine = token.Line, 
                EndColumn = token.Column + token.Length, 
                EndPosition = token.Position + token.Length 
            };

        default:
            return new StringValueNode 
            { 
                Value = token.Value, 
                RawValue = token.Value, 
                StartLine = token.Line, 
                StartColumn = token.Column, 
                StartPosition = token.Position, 
                EndLine = token.Line, 
                EndColumn = token.Column + token.Length, 
                EndPosition = token.Position + token.Length 
            };
    }
}
```

### 3. ParseExpandedArray Method
```csharp
private ArrayNode ParseExpandedArray(int size, int indentLevel)
{
    var array = new ArrayNode { DeclaredSize = size };
    var startToken = _position > 0 ? _tokens[_position - 1] : CurrentToken;

    SkipWhitespaceAndComments();

    while (!IsAtEnd() && GetCurrentIndentation() >= indentLevel)
    {
        if (CurrentToken.Type == TokenType.Identifier || CurrentToken.Type == TokenType.String)
        {
            // Check for "- " list item marker
            if (CurrentToken.Value == "-")
            {
                Advance();
                SkipWhitespace();

                // Parse list item value
                AstNode item;

                // Check for array header (- [N]: ...)
                if (CurrentToken.Type == TokenType.LeftBracket)
                {
                    // This is an inline array list item
                    var prop = ParseProperty(indentLevel);
                    item = prop != null ? prop.Value : new NullValueNode { RawValue = "null" };
                }
                // Check for object with first field on hyphen line
                else if (LookAheadForColon())
                {
                    // Parse as object with first field
                    item = ParseListItemObject(indentLevel);
                }
                else
                {
                    // Simple primitive value
                    item = ParseValue();
                }

                array.Elements.Add(item);

                // Skip to next line
                SkipWhitespaceAndComments();
            }
            else
            {
                // Not a list item, break
                break;
            }
        }
        else if (CurrentToken.Type == TokenType.Newline)
        {
            Advance();
        }
        else
        {
            break;
        }
    }

    array.StartLine = startToken.Line;
    array.StartColumn = startToken.Column;
    array.StartPosition = startToken.Position;

    if (array.Elements.Count > 0 && array.Elements[array.Elements.Count - 1] != null)
    {
        array.EndLine = array.Elements[array.Elements.Count - 1].EndLine;
        array.EndColumn = array.Elements[array.Elements.Count - 1].EndColumn;
        array.EndPosition = array.Elements[array.Elements.Count - 1].EndPosition;
    }

    return array;
}
```

### 4. ParseListItemObject Method (§10 Support)
```csharp
private ObjectNode ParseListItemObject(int baseIndent)
{
    var obj = new ObjectNode();
    var startToken = CurrentToken;
    
    // Parse first field on the hyphen line (- key: value or - key[N]{fields}:)
    var firstProp = ParseProperty(baseIndent);
    if (firstProp != null)
    {
        obj.Properties.Add(firstProp);
        
        // Check if first property is a tabular array
        bool isTabularFirst = firstProp.Value is TableArrayNode;
        
        if (isTabularFirst)
        {
            // For tabular first field: rows at depth +2, other fields at depth +1
            // (already parsed by ParseTableArray)
            // Now parse remaining fields at depth +1
            SkipWhitespaceAndComments();
            
            while (!IsAtEnd() && GetCurrentIndentation() == baseIndent + 1)
            {
                var prop = ParseProperty(baseIndent + 1);
                if (prop != null)
                {
                    obj.Properties.Add(prop);
                }
                SkipWhitespaceAndComments();
            }
        }
        else
        {
            // For non-tabular: all remaining fields at depth +2
            SkipWhitespaceAndComments();
            
            while (!IsAtEnd() && GetCurrentIndentation() >= baseIndent + 2)
            {
                if (GetCurrentIndentation() > baseIndent + 2)
                {
                    throw new ParseException($"Unexpected indentation at line {CurrentToken.Line}");
                }
                
                var prop = ParseProperty(baseIndent + 2);
                if (prop != null)
                {
                    obj.Properties.Add(prop);
                }
                SkipWhitespaceAndComments();
            }
        }
    }
    
    obj.StartLine = startToken.Line;
    obj.StartColumn = startToken.Column;
    obj.StartPosition = startToken.Position;
    
    if (obj.Properties.Count > 0)
    {
        var lastProp = obj.Properties[obj.Properties.Count - 1];
        obj.EndLine = lastProp.EndLine;
        obj.EndColumn = lastProp.EndColumn;
        obj.EndPosition = lastProp.EndPosition;
    }
    
    return obj;
}
```

### 5. LookAheadForColon Helper
```csharp
private bool LookAheadForColon()
{
    int lookAhead = _position;
    while (lookAhead < _tokens.Count)
    {
        var token = _tokens[lookAhead];
        if (token.Type == TokenType.Colon)
            return true;
        if (token.Type == TokenType.Newline || token.Type == TokenType.EndOfFile)
            return false;
        lookAhead++;
    }
    return false;
}
```

## File Cleanup Required

The current `ToonParser.cs` has duplicate code in `ParseTableRowFlexible`. Need to:
1. Remove the duplicate method body (lines after the first complete implementation)
2. Add the helper methods listed above
3. Test with delimiter variations

## Testing Checklist

### Delimiter Tests
- [ ] Comma delimiter (default): `items[3]: a,b,c`
- [ ] Tab delimiter: `items[3	]: a	b	c`
- [ ] Pipe delimiter: `items[3|]: a|b|c`
- [ ] Schema with comma: `data[2]{id,name}: 1,Alice`
- [ ] Schema with tab: `data[2	]{id	name}: 1	Alice`
- [ ] Schema with pipe: `data[2|]{id|name}: 1|Alice`

### Objects as List Items Tests
- [ ] Empty object: Single `-`
- [ ] Simple object: `- id: 1` with nested fields
- [ ] Tabular first field with rows at +2, other fields at +1
- [ ] Non-tabular first field with all fields at +2

## TOON Spec References

- **§6**: Header Syntax with delimiter detection
- **§9.2**: Arrays of Arrays (expanded list items with `- [M]: ...`)
- **§9.4**: Mixed/Non-Uniform Arrays (expanded list with `- ` marker)
- **§10**: Objects as List Items (special tabular-first-field encoding)
- **§11**: Delimiters (comma/tab/pipe with active delimiter scoping)

## Next Steps

1. **CRITICAL**: Remove duplicate code from `ToonParser.cs`
2. Add all helper methods listed above
3. Clean build and fix any compilation errors
4. Add comprehensive tests for each delimiter type
5. Add tests for objects as list items patterns
6. Test round-trip: parse → serialize → parse
