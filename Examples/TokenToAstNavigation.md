# Token to AST Navigation

The ToonTokenizer library provides powerful bidirectional navigation between tokens (lexical elements) and AST nodes (parsed structures). This makes it easy to build IDE features like IntelliSense, go-to-definition, hover tooltips, and more.

## Understanding the Layers

```
Source Text  →  Tokens (Lexer)  →  AST Nodes (Parser)
   ↓               ↓                    ↓
"name: John"  →  [Identifier,    →  PropertyNode
                  Colon,              Key: "name"
                  String]             Value: StringValueNode
```

- **Tokens**: Raw lexical elements with type and position
- **AST Nodes**: Structured syntax tree with semantic meaning

## API Overview

### Extension Methods on Token

```csharp
using ToonTokenizer;
using ToonTokenizer.Ast;

// Get the AST node containing this token
AstNode? node = token.GetAstNode(document);

// Get the PropertyNode containing this token
PropertyNode? property = token.GetPropertyNode(document);
```

### Extension Methods on ToonParseResult

```csharp
// Find node at a specific character position
AstNode? node = result.GetNodeAtPosition(position);

// Get node for a specific token
AstNode? node = result.GetNodeForToken(token);

// Find property at line/column
PropertyNode? property = result.GetPropertyAt(line, column);

// Get all properties (including nested)
List<PropertyNode> allProps = result.GetAllProperties();

// Find property by dot-notation path
PropertyNode? property = result.FindPropertyByPath("user.settings.theme");
```

## Common Scenarios

### 1. Hover Tooltip (Show property info at cursor position)

```csharp
public string GetHoverInfo(int line, int column)
{
    var result = Toon.Parse(documentText);
    var property = result.GetPropertyAt(line, column);
    
    if (property == null)
        return "No property found";
    
    var info = new StringBuilder();
    info.AppendLine($"Property: {property.Key}");
    info.AppendLine($"Type: {property.Value.GetType().Name}");
    info.AppendLine($"Location: Line {property.StartLine}, Column {property.StartColumn}");
    info.AppendLine($"Indent Level: {property.IndentLevel}");
    
    if (property.Value is StringValueNode str)
        info.AppendLine($"Value: \"{str.Value}\"");
    else if (property.Value is NumberValueNode num)
        info.AppendLine($"Value: {num.Value}");
    else if (property.Value is BooleanValueNode b)
        info.AppendLine($"Value: {b.Value}");
    else if (property.Value is ObjectNode obj)
        info.AppendLine($"Nested properties: {obj.Properties.Count}");
    else if (property.Value is ArrayNode arr)
        info.AppendLine($"Array elements: {arr.Elements.Count}");
    else if (property.Value is TableArrayNode table)
        info.AppendLine($"Table rows: {table.Rows.Count}, Fields: {table.Schema.Count}");
    
    return info.ToString();
}
```

### 2. Go to Definition (Navigate to property declaration)

```csharp
public Position? GoToDefinition(int line, int column)
{
    var result = Toon.Parse(documentText);
    
    // Find the token at cursor
    var token = result.Tokens.GetTokenAt(line, column);
    if (token == null)
        return null;
    
    // If it's an identifier, find the property it refers to
    if (token.Type == TokenType.Identifier || token.Type == TokenType.String)
    {
        // Find property by key
        var property = result.GetAllProperties()
            .FirstOrDefault(p => p.Key == token.Value);
        
        if (property != null)
        {
            return new Position(property.StartLine, property.StartColumn);
        }
    }
    
    return null;
}
```

### 3. Find All References (Find all uses of a property)

```csharp
public List<Range> FindReferences(string propertyKey)
{
    var result = Toon.Parse(documentText);
    var references = new List<Range>();
    
    // Find all properties with this key
    foreach (var property in result.GetAllProperties())
    {
        if (property.Key == propertyKey)
        {
            references.Add(new Range(
                property.StartLine,
                property.StartColumn,
                property.EndLine,
                property.EndColumn
            ));
        }
    }
    
    return references;
}
```

### 4. Code Completion (Suggest property paths)

```csharp
public List<string> GetPropertyPaths()
{
    var result = Toon.Parse(documentText);
    var paths = new List<string>();
    
    void CollectPaths(string prefix, ObjectNode obj)
    {
        foreach (var prop in obj.Properties)
        {
            string path = string.IsNullOrEmpty(prefix) 
                ? prop.Key 
                : $"{prefix}.{prop.Key}";
            
            paths.Add(path);
            
            if (prop.Value is ObjectNode nestedObj)
            {
                CollectPaths(path, nestedObj);
            }
        }
    }
    
    // Start with document-level properties
    foreach (var prop in result.Document.Properties)
    {
        paths.Add(prop.Key);
        if (prop.Value is ObjectNode obj)
        {
            CollectPaths(prop.Key, obj);
        }
    }
    
    return paths;
}
```

### 5. Semantic Token Classification

```csharp
public List<SemanticToken> GetSemanticTokens()
{
    var result = Toon.Parse(documentText);
    var semanticTokens = new List<SemanticToken>();
    
    foreach (var token in result.Tokens)
    {
        // Find which AST node this token belongs to
        var node = token.GetAstNode(result.Document);
        
        string tokenType;
        string[] modifiers = Array.Empty<string>();
        
        if (node is PropertyNode prop && token.Value == prop.Key)
        {
            tokenType = "property";
            modifiers = new[] { "declaration" };
        }
        else if (node is StringValueNode)
        {
            tokenType = "string";
        }
        else if (node is NumberValueNode)
        {
            tokenType = "number";
        }
        else if (node is BooleanValueNode)
        {
            tokenType = "keyword";
        }
        else if (token.IsStructural())
        {
            tokenType = "operator";
        }
        else
        {
            tokenType = token.GetClassification();
        }
        
        semanticTokens.Add(new SemanticToken(
            token.Line,
            token.Column,
            token.Length,
            tokenType,
            modifiers
        ));
    }
    
    return semanticTokens;
}
```

### 6. Document Outline (Show document structure)

```csharp
public List<OutlineItem> GetOutline()
{
    var result = Toon.Parse(documentText);
    var outline = new List<OutlineItem>();
    
    void BuildOutline(List<PropertyNode> properties, int level, List<OutlineItem> items)
    {
        foreach (var prop in properties)
        {
            var item = new OutlineItem
            {
                Label = prop.Key,
                Kind = GetKind(prop.Value),
                Range = new Range(prop.StartLine, prop.StartColumn, prop.EndLine, prop.EndColumn),
                Level = level
            };
            
            if (prop.Value is ObjectNode obj)
            {
                item.Children = new List<OutlineItem>();
                BuildOutline(obj.Properties, level + 1, item.Children);
            }
            
            items.Add(item);
        }
    }
    
    BuildOutline(result.Document.Properties, 0, outline);
    return outline;
}

private string GetKind(AstNode node)
{
    return node switch
    {
        ObjectNode => "object",
        ArrayNode => "array",
        TableArrayNode => "table",
        StringValueNode => "string",
        NumberValueNode => "number",
        BooleanValueNode => "boolean",
        NullValueNode => "null",
        _ => "property"
    };
}
```

### 7. Breadcrumb Navigation (Show current property path)

```csharp
public string GetBreadcrumb(int line, int column)
{
    var result = Toon.Parse(documentText);
    var property = result.GetPropertyAt(line, column);
    
    if (property == null)
        return string.Empty;
    
    var path = new List<string> { property.Key };
    
    // Walk up the tree to build full path
    // (This requires tracking parent references - see next section)
    
    return string.Join(" > ", path);
}
```

## Working with Nested Properties

When you need to navigate up the tree (from child to parent), you'll need to track parent references yourself:

```csharp
public Dictionary<AstNode, AstNode> BuildParentMap(ToonDocument document)
{
    var parentMap = new Dictionary<AstNode, AstNode>();
    
    void MapParents(AstNode parent, AstNode child)
    {
        parentMap[child] = parent;
        
        switch (child)
        {
            case PropertyNode prop:
                MapParents(child, prop.Value);
                break;
            case ObjectNode obj:
                foreach (var p in obj.Properties)
                    MapParents(child, p);
                break;
            case ArrayNode arr:
                foreach (var elem in arr.Elements)
                    MapParents(child, elem);
                break;
            case TableArrayNode table:
                foreach (var row in table.Rows)
                    foreach (var cell in row)
                        MapParents(child, cell);
                break;
        }
    }
    
    foreach (var prop in document.Properties)
        MapParents(document, prop);
    
    return parentMap;
}
```

## Performance Considerations

### Token Lookup
- `GetTokenAt(line, column)`: O(n) linear search
- For frequent lookups, consider building a spatial index

### AST Navigation
- `GetNodeAtPosition(position)`: O(n) tree traversal
- `FindPropertyByPath(path)`: O(d) where d is depth
- `GetAllProperties()`: O(n) full tree traversal

### Best Practices
1. **Cache parse results** if document hasn't changed
2. **Reuse ToonParseResult** instead of re-parsing
3. **Build auxiliary data structures** (parent maps, indexes) once
4. **Incremental updates**: Re-parse only changed portions (future feature)

## Complete Example: IDE Hover Provider

```csharp
public class ToonHoverProvider
{
    private string _documentText;
    private ToonParseResult? _cachedResult;
    
    public void UpdateDocument(string text)
    {
        _documentText = text;
        _cachedResult = null; // Invalidate cache
    }
    
    private ToonParseResult GetResult()
    {
        if (_cachedResult == null)
            _cachedResult = Toon.Parse(_documentText);
        return _cachedResult;
    }
    
    public string? GetHoverContent(int line, int column)
    {
        var result = GetResult();
        
        // Get token at cursor
        var token = result.Tokens.GetTokenAt(line, column);
        if (token == null)
            return null;
        
        // Get property containing this token
        var property = token.GetPropertyNode(result.Document);
        if (property == null)
            return null;
        
        var sb = new StringBuilder();
        sb.AppendLine($"**{property.Key}**");
        sb.AppendLine();
        
        // Add type information
        var valueType = property.Value.GetType().Name.Replace("Node", "");
        sb.AppendLine($"Type: `{valueType}`");
        
        // Add value preview
        switch (property.Value)
        {
            case StringValueNode str:
                sb.AppendLine($"Value: `\"{str.Value}\"`");
                break;
            case NumberValueNode num:
                sb.AppendLine($"Value: `{num.Value}`");
                break;
            case BooleanValueNode b:
                sb.AppendLine($"Value: `{b.Value}`");
                break;
            case ObjectNode obj:
                sb.AppendLine($"Properties: {obj.Properties.Count}");
                break;
            case ArrayNode arr:
                sb.AppendLine($"Elements: {arr.Elements.Count}");
                if (arr.DeclaredSize >= 0)
                    sb.AppendLine($"Declared size: {arr.DeclaredSize}");
                break;
            case TableArrayNode table:
                sb.AppendLine($"Rows: {table.Rows.Count}");
                sb.AppendLine($"Schema: {string.Join(", ", table.Schema)}");
                break;
        }
        
        // Add location
        sb.AppendLine();
        sb.AppendLine($"Location: Line {property.StartLine}, Column {property.StartColumn}");
        
        return sb.ToString();
    }
}
```

## Summary

The Token-to-AST navigation API provides:

✅ **Easy token lookup** by line/column  
✅ **AST node discovery** from tokens  
✅ **Property path navigation** with dot notation  
✅ **Full tree traversal** helpers  
✅ **IDE integration** patterns

This makes ToonTokenizer perfect for building:
- Language servers
- VS Code extensions
- Visual Studio extensions
- Syntax highlighters
- Code analysis tools
- Refactoring tools
