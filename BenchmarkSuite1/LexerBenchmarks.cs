using BenchmarkDotNet.Attributes;
using ToonTokenizer;
using Microsoft.VSDiagnostics;
using System.Linq;
using System.Collections.Generic;

namespace ToonTokenizerBenchmarks;

[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class LexerBenchmarks
{
    private string _simpleTokens = null!;
    private string _stringHeavyDocument = null!;
    private string _numberHeavyDocument = null!;
    private string _commentHeavyDocument = null!;
    private string _mixedDocument = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Simple tokens: basic identifiers and values
        _simpleTokens = """
            name: value
            key: data
            foo: bar
            test: result
            """;

        // String-heavy: lots of quoted strings
        _stringHeavyDocument = GenerateStringHeavyDocument();

        // Number-heavy: lots of numeric values
        _numberHeavyDocument = GenerateNumberHeavyDocument();

        // Comment-heavy: lots of comments
        _commentHeavyDocument = GenerateCommentHeavyDocument();

        // Mixed document: variety of token types
        _mixedDocument = GenerateMixedDocument();
    }

    private static string GenerateStringHeavyDocument()
    {
        var lines = new System.Text.StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            lines.AppendLine($"str_{i}: \"This is a quoted string value number {i} with some longer text\"");
        }
        return lines.ToString();
    }

    private static string GenerateNumberHeavyDocument()
    {
        var lines = new System.Text.StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            lines.AppendLine($"int_{i}: {i * 1000}");
            lines.AppendLine($"float_{i}: {i * 3.14159}");
            lines.AppendLine($"negative_{i}: -{i}");
        }
        return lines.ToString();
    }

    private static string GenerateCommentHeavyDocument()
    {
        var lines = new System.Text.StringBuilder();
        for (int i = 0; i < 50; i++)
        {
            lines.AppendLine($"# This is comment number {i} with some descriptive text");
            lines.AppendLine($"prop_{i}: value_{i}");
        }
        return lines.ToString();
    }

    private static string GenerateMixedDocument()
    {
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("# Mixed document with various token types");
        
        for (int i = 0; i < 50; i++)
        {
            lines.AppendLine($"# Section {i}");
            lines.AppendLine($"section_{i}:");
            lines.AppendLine($"    string_val: \"value {i}\"");
            lines.AppendLine($"    int_val: {i * 100}");
            lines.AppendLine($"    float_val: {i * 1.5}");
            lines.AppendLine($"    bool_val: {(i % 2 == 0).ToString().ToLower()}");
            lines.AppendLine($"    null_val: null");
            lines.AppendLine($"    array: [item1, item2, item3]");
        }
        
        return lines.ToString();
    }

    [Benchmark]
    public List<Token> TokenizeSimple()
    {
        var lexer = new ToonLexer(_simpleTokens);
        return lexer.Tokenize();
    }

    [Benchmark]
    public List<Token> TokenizeStringHeavy()
    {
        var lexer = new ToonLexer(_stringHeavyDocument);
        return lexer.Tokenize();
    }

    [Benchmark]
    public List<Token> TokenizeNumberHeavy()
    {
        var lexer = new ToonLexer(_numberHeavyDocument);
        return lexer.Tokenize();
    }

    [Benchmark]
    public List<Token> TokenizeCommentHeavy()
    {
        var lexer = new ToonLexer(_commentHeavyDocument);
        return lexer.Tokenize();
    }

    [Benchmark]
    public List<Token> TokenizeMixed()
    {
        var lexer = new ToonLexer(_mixedDocument);
        return lexer.Tokenize();
    }
}
