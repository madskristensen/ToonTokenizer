using BenchmarkDotNet.Attributes;
using ToonTokenizer;
using Microsoft.VSDiagnostics;
using System.Linq;

namespace ToonTokenizerBenchmarks;

[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class ParserBenchmarks
{
    private string _smallDocument = null!;
    private string _mediumDocument = null!;
    private string _largeDocument = null!;
    private string _deeplyNestedDocument = null!;
    private string _arrayHeavyDocument = null!;
    private string _tableArrayDocument = null!;
    [GlobalSetup]
    public void Setup()
    {
        // Small document: Simple key-value pairs
        _smallDocument = """
            name: John Doe
            age: 30
            active: true
            email: john@example.com
            """;
        // Medium document: Multiple properties with nested objects
        _mediumDocument = GenerateMediumDocument();
        // Large document: Many properties and arrays
        _largeDocument = GenerateLargeDocument();
        // Deeply nested document: Tests nesting depth handling
        _deeplyNestedDocument = GenerateDeeplyNestedDocument(20);
        // Array-heavy document: Tests array parsing performance
        _arrayHeavyDocument = GenerateArrayHeavyDocument();
        // Table array document: Tests table array parsing
        _tableArrayDocument = GenerateTableArrayDocument();
    }

    private static string GenerateMediumDocument()
    {
        return """
            # User profile document
            user:
                name: John Doe
                email: john.doe@example.com
                age: 30
                active: true
                score: 95.5
                
            settings:
                theme: dark
                notifications: true
                language: en-US
                
            tags: [developer, designer, manager]
            
            metadata:
                created: 2024-01-15
                updated: 2024-06-20
                version: 1.0.0
            """;
    }

    private static string GenerateLargeDocument()
    {
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("# Large document with many properties");
        for (int i = 0; i < 100; i++)
        {
            lines.AppendLine($"item_{i}:");
            lines.AppendLine($"    id: {i}");
            lines.AppendLine($"    name: Item {i}");
            lines.AppendLine($"    value: {i * 10.5}");
            lines.AppendLine($"    active: {(i % 2 == 0).ToString().ToLower()}");
            lines.AppendLine($"    tags: [tag{i}a, tag{i}b, tag{i}c]");
        }

        return lines.ToString();
    }

    private static string GenerateDeeplyNestedDocument(int depth)
    {
        var lines = new System.Text.StringBuilder();
        var indent = "";
        for (int i = 0; i < depth; i++)
        {
            lines.AppendLine($"{indent}level_{i}:");
            indent += "    ";
        }

        lines.AppendLine($"{indent}value: deepest");
        return lines.ToString();
    }

    private static string GenerateArrayHeavyDocument()
    {
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("# Document with many arrays");
        for (int i = 0; i < 50; i++)
        {
            var items = string.Join(", ", Enumerable.Range(0, 20).Select(j => $"item{j}"));
            lines.AppendLine($"array_{i}: [{items}]");
        }

        // Add some nested arrays
        lines.AppendLine("nested_arrays:");
        for (int i = 0; i < 10; i++)
        {
            lines.AppendLine($"    arr_{i}: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]");
        }

        return lines.ToString();
    }

    private static string GenerateTableArrayDocument()
    {
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("# Table array document");
        lines.AppendLine("users:");
        lines.AppendLine("\tname\tage\temail\tactive");
        for (int i = 0; i < 50; i++)
        {
            lines.AppendLine($"\tUser{i}\t{20 + i}\tuser{i}@example.com\ttrue");
        }

        return lines.ToString();
    }

    [Benchmark]
    public ToonParseResult ParseSmallDocument()
    {
        return Toon.Parse(_smallDocument);
    }

    [Benchmark]
    public ToonParseResult ParseMediumDocument()
    {
        return Toon.Parse(_mediumDocument);
    }

    [Benchmark]
    public ToonParseResult ParseLargeDocument()
    {
        return Toon.Parse(_largeDocument);
    }

    [Benchmark]
    public ToonParseResult ParseDeeplyNestedDocument()
    {
        return Toon.Parse(_deeplyNestedDocument);
    }

    [Benchmark]
    public ToonParseResult ParseArrayHeavyDocument()
    {
        return Toon.Parse(_arrayHeavyDocument);
    }

    [Benchmark]
    public ToonParseResult ParseTableArrayDocument()
    {
        return Toon.Parse(_tableArrayDocument);
    }
}