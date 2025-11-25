# Contributing to ToonTokenizer

Thank you for your interest in contributing to ToonTokenizer! This document provides guidelines and instructions for contributing.

## Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and grow
- Keep discussions on-topic

## How to Contribute

### Reporting Bugs

Before creating a bug report, please:

1. **Search existing issues** to avoid duplicates
2. **Use the latest version** to ensure the bug hasn't been fixed
3. **Provide a minimal reproduction** if possible

When filing a bug report, include:
- ToonTokenizer version
- .NET version and runtime
- TOON input that causes the issue
- Expected vs actual behavior
- Stack trace (if applicable)

### Suggesting Features

Feature requests are welcome! Please:

1. Check if it aligns with the TOON specification
2. Explain the use case and benefit
3. Provide examples of how it would work
4. Consider implementation complexity

### Submitting Changes

#### Development Setup

1. **Fork the repository**
```bash
git clone https://github.com/YOUR_USERNAME/ToonTokenizer.git
cd ToonTokenizer
```

2. **Install dependencies**
```bash
dotnet restore
```

3. **Build the project**
```bash
dotnet build
```

4. **Run tests**
```bash
dotnet test
```

#### Making Changes

1. **Create a branch**
```bash
git checkout -b feature/your-feature-name
```

2. **Make your changes**
   - Follow the existing code style (see Code Style section)
   - Add tests for new functionality
   - Update documentation
   - Keep commits focused and atomic

3. **Test your changes**
```bash
dotnet test
```

4. **Commit your changes**
```bash
git commit -m "Add feature: your feature description"
```

Use clear, descriptive commit messages:
- Start with a verb (Add, Fix, Update, Remove)
- Be concise but descriptive
- Reference issues if applicable (#123)

5. **Push to your fork**
```bash
git push origin feature/your-feature-name
```

6. **Create a Pull Request**
   - Describe what changed and why
   - Reference related issues
   - Ensure all checks pass

## Code Style

### General Principles

- **Clarity over cleverness** - Code should be easy to understand
- **Consistency** - Follow existing patterns
- **Simplicity** - Avoid unnecessary complexity
- **Testing** - All new code should have tests

### C# Style Guidelines

Follow the existing code style:

```csharp
// ✅ Good: Clear, well-named, documented
/// <summary>
/// Parses a TOON document and returns the AST with any errors.
/// </summary>
public static ToonParseResult Parse(string source)
{
    if (source == null)
        throw new ArgumentNullException(nameof(source));
    
    // Implementation...
}

// ❌ Bad: Unclear, poorly named
public static object P(string s) { ... }
```

#### Naming Conventions

- Classes: `PascalCase`
- Methods: `PascalCase`
- Properties: `PascalCase`
- Fields (private): `_camelCase` with underscore
- Parameters: `camelCase`
- Local variables: `camelCase`
- Constants: `PascalCase`

#### Code Organization

```csharp
namespace ToonTokenizer
{
    // 1. Using statements (sorted)
    using System;
    using System.Collections.Generic;
    
    // 2. XML documentation
    /// <summary>
    /// Brief description
    /// </summary>
    public class MyClass
    {
        // 3. Constants
        private const int DefaultValue = 42;
        
        // 4. Fields
        private readonly string _value;
        
        // 5. Constructors
        public MyClass(string value)
        {
            _value = value;
        }
        
        // 6. Properties
        public string Value => _value;
        
        // 7. Public methods
        public void PublicMethod() { }
        
        // 8. Private methods
        private void PrivateMethod() { }
    }
}
```

#### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Brief one-line description.
/// </summary>
/// <param name="source">Parameter description.</param>
/// <returns>What the method returns.</returns>
/// <exception cref="ArgumentNullException">When thrown.</exception>
public static ToonParseResult Parse(string source)
```

### Testing Guidelines

#### Test Structure

```csharp
[TestClass]
public class MyFeatureTests
{
    [TestMethod]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var input = "test data";
        
        // Act
        var result = MyMethod(input);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("expected", result.Value);
    }
}
```

#### Test Coverage

- **Happy paths**: Normal, expected inputs
- **Edge cases**: Empty strings, nulls, boundary values
- **Error cases**: Invalid input, malformed syntax
- **Regression tests**: For bug fixes

#### Test Naming

Use descriptive names that explain the scenario:

```csharp
// ✅ Good
[TestMethod]
public void Parse_EmptyArray_ReturnsEmptyArrayNode()

[TestMethod]
public void Parse_MissingColon_ReturnsErrorInResult()

// ❌ Bad
[TestMethod]
public void Test1()

[TestMethod]
public void ParseTest()
```

## Project Structure

```
ToonTokenizer/
├── ToonTokenizer/           # Main library
│   ├── Toon.cs              # Public API
│   ├── ToonLexer.cs         # Tokenization
│   ├── ToonParser.cs        # Parsing
│   ├── ToonParseResult.cs   # Result types
│   ├── Token.cs             # Token definitions
│   ├── Ast/                 # AST node types
│   └── Extensions/          # Extension methods
├── ToonTokenizerTest/       # Unit tests
├── Examples/                # Usage examples
├── README.md                # Main documentation
├── spec.md                  # TOON specification
└── CHANGELOG.md             # Version history
```

## Adding New Features

### 1. Specification Alignment

Before implementing a feature:

1. Check the TOON specification (spec.md)
2. Ensure your feature aligns with the spec
3. If adding a non-standard feature, discuss first

### 2. Implementation Checklist

- [ ] Feature code implemented
- [ ] Unit tests added
- [ ] Integration tests added (if needed)
- [ ] XML documentation added
- [ ] README updated (if user-facing)
- [ ] CHANGELOG updated
- [ ] Examples added (if significant feature)
- [ ] All tests pass
- [ ] No new compiler warnings

### 3. Pull Request Checklist

- [ ] Descriptive title
- [ ] Clear description of changes
- [ ] References related issues
- [ ] Tests pass locally
- [ ] Documentation updated
- [ ] CHANGELOG entry added

## Areas for Contribution

### High Priority

- More extension methods for common patterns
- Additional examples and documentation
- Performance optimizations
- Better error messages

### Medium Priority

- TOON encoding support (AST → TOON text)
- JSON converter (TOON ↔ JSON)
- Configuration options
- Schema validation

### Low Priority

- Streaming parser API
- Binary TOON format
- Additional language bindings

## Questions?

- Open a [GitHub Discussion](https://github.com/madskristensen/ToonTokenizer/discussions)
- File an [Issue](https://github.com/madskristensen/ToonTokenizer/issues)
- Check existing documentation in the repo

## Recognition

Contributors will be:
- Listed in release notes
- Credited in CHANGELOG.md
- Thanked in the community

## License

By contributing, you agree that your contributions will be licensed under the Apache License 2.0.

---

Thank you for contributing to ToonTokenizer! 🎉
