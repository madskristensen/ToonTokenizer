# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in ToonTokenizer, please report it by emailing the maintainer directly. Please do not open a public issue for security vulnerabilities.

**Contact**: https://github.com/madskristensen/ToonTokenizer/security/advisories/new

We take security seriously and will respond to valid reports as quickly as possible. Please include:

- A description of the vulnerability
- Steps to reproduce the issue
- Potential impact assessment
- Any suggested fixes (if applicable)

## Security Considerations

ToonTokenizer is designed to parse and encode TOON (Token-Oriented Object Notation) format data. When using this library, consider the following security aspects:

### Input Validation

The library includes built-in protections against common attack vectors:

- **Input Size Limits**: Prevents memory exhaustion from oversized inputs (default: 10 MB)
- **Token Count Limits**: Prevents algorithmic complexity attacks (default: 1,000,000 tokens)
- **String Length Limits**: Prevents memory exhaustion from excessively long strings (default: 64 KB)
- **Nesting Depth Limits**: Prevents stack overflow from deeply nested structures (default: 100 levels)
- **Array Size Limits**: Prevents memory exhaustion from extremely large arrays (default: 1,000,000 elements)
- **Regex Timeouts**: Protects against Regular Expression Denial of Service (ReDoS) attacks (1 second timeout)

### Resource Limits Configuration

All security limits can be configured using the `ToonParserOptions` class:

```csharp
var options = new ToonParserOptions
{
    MaxInputSize = 5 * 1024 * 1024,      // 5 MB (default: 10 MB)
    MaxTokenCount = 500_000,              // 500k tokens (default: 1M)
    MaxStringLength = 32 * 1024,          // 32 KB (default: 64 KB)
    MaxNestingDepth = 50,                 // 50 levels (default: 100)
    MaxArraySize = 100_000                // 100k elements (default: 1M)
};

var result = Toon.Parse(toonSource, options);
```

#### Preset Configurations

The library provides two preset configurations:

**Default Preset** (Recommended for most scenarios):
- Suitable for typical TOON documents
- Balances security and functionality
- Use when parsing untrusted or user-provided input

**Unlimited Preset** (Use with caution):
- Removes all resource limits
- Only use with trusted input sources
- Appropriate for internal tools or controlled environments

```csharp
// For untrusted input (recommended)
var safeOptions = ToonParserOptions.Default;

// For trusted input only
var unlimitedOptions = ToonParserOptions.Unlimited;
```

### Best Practices

#### When Parsing Untrusted Input

1. **Always use the default resource limits** - Don't use `ToonParserOptions.Unlimited` for untrusted data
2. **Validate the source** - Verify the origin of TOON data before parsing
3. **Handle errors gracefully** - Catch and log parsing exceptions without exposing internal details
4. **Limit concurrent parsing operations** - Prevent resource exhaustion from parallel attacks
5. **Monitor resource usage** - Track parsing performance and memory consumption

```csharp
try
{
    var options = ToonParserOptions.Default; // Use safe defaults
    var result = Toon.Parse(untrustedInput, options);
    
    if (!result.Success)
    {
        // Log errors without exposing details to end users
        Logger.Warning($"TOON parsing failed with {result.Errors.Count} errors");
        return null;
    }
    
    return result.Root;
}
catch (ArgumentException ex)
{
    // Handle resource limit violations
    Logger.Warning($"TOON input validation failed: {ex.Message}");
    return null;
}
```

#### When Parsing Trusted Input

For internal tools or scenarios where you control the input source:

```csharp
// Option 1: Use custom limits appropriate for your use case
var customOptions = new ToonParserOptions
{
    MaxInputSize = 50 * 1024 * 1024,  // 50 MB for large internal documents
    MaxNestingDepth = 200              // Allow deeper nesting for complex data
};

// Option 2: Remove limits entirely (use with extreme caution)
var unlimitedOptions = ToonParserOptions.Unlimited;
```

### Encoder Security

When encoding data to TOON format:

- **JSON Input Validation**: The `EncodeFromJson` method validates input size (10 MB limit)
- **Regex Protection**: All regex patterns use 1-second timeouts to prevent ReDoS attacks
- **Memory Management**: The encoder is designed to process data efficiently without excessive allocations

```csharp
var encoder = new ToonEncoder();

try
{
    string toon = encoder.EncodeFromJson(jsonString);
}
catch (ArgumentException ex)
{
    // Handle size limit violations
    Logger.Warning($"JSON encoding failed: {ex.Message}");
}
```

### Error Handling

The library provides detailed error information without exposing sensitive details:

- Parse errors include line/column position for debugging
- Error messages are safe to display to end users
- No stack traces or internal state information in error messages
- Resource limit violations provide actionable guidance

### Thread Safety

- **ToonParser** and **ToonLexer** instances are **not thread-safe** - create separate instances per thread
- **ToonEncoder** instances are **not thread-safe** - create separate instances per thread
- The **Toon** static class methods are thread-safe (create new parser instances internally)
- **ToonParserOptions** instances are immutable after creation and safe to share across threads

### Dependencies

ToonTokenizer has minimal dependencies:

- **.NET Standard 2.0** / **.NET Framework 4.6.2** / **.NET 8** / **.NET 10** - No additional runtime dependencies
- **System.Text.Json 9.0.0** - Used only by ToonEncoder for JSON parsing

All dependencies are from trusted sources and regularly updated.

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |

## Security Updates

Security updates will be released as patch versions. We recommend:

- Always use the latest patch version
- Subscribe to repository releases for notifications
- Review the CHANGELOG for security-related updates

## Acknowledgments

We appreciate security researchers who responsibly disclose vulnerabilities. Contributors will be acknowledged in release notes (unless they prefer to remain anonymous).
