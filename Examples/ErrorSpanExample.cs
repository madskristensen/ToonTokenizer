using ToonTokenizer;

// Example 1: Using TryParse with span information
Console.WriteLine("=== Example 1: TryParse with error spans ===");
var source1 = @"name John
age: 30";

if (!Toon.TryParse(source1, out var result1))
{
    Console.WriteLine($"Found {result1.Errors.Count} error(s):");
    foreach (var error in result1.Errors)
    {
        Console.WriteLine($"  - {error.Message}");
        Console.WriteLine($"    Position: {error.Position}, Length: {error.Length}");
        Console.WriteLine($"    Line: {error.Line}, Column: {error.Column}");
        Console.WriteLine($"    Span: {error.Position}..{error.EndPosition}");
        Console.WriteLine();
        
        // Show the error location in context
        var lines = source1.Split('\n');
        if (error.Line > 0 && error.Line <= lines.Length)
        {
            var line = lines[error.Line - 1];
            Console.WriteLine($"    Context: {line}");
            Console.WriteLine($"             {new string(' ', error.Column - 1)}{new string('^', Math.Max(1, error.Length))}");
        }
    }
}

Console.WriteLine();

// Example 2: Multiple errors with different positions
Console.WriteLine("=== Example 2: Complex error with nested structure ===");
var source2 = @"users[5]{id,name}:
  1,Alice
  2 Bob
  3,Charlie";

if (!Toon.TryParse(source2, out var result2))
{
    Console.WriteLine($"Found {result2.Errors.Count} error(s):");
    foreach (var error in result2.Errors)
    {
        Console.WriteLine($"  Error: {error}");
    }
}
else
{
    Console.WriteLine("Document parsed successfully!");
}

Console.WriteLine();

// Example 3: Using Parse with result object
Console.WriteLine("=== Example 3: Parse result with error info ===");
var source3 = "items[3: incomplete";

var result3 = Toon.Parse(source3);
if (!result3.IsSuccess)
{
    Console.WriteLine($"Parse failed with {result3.Errors.Count} error(s):");
    foreach (var error in result3.Errors)
    {
        Console.WriteLine($"  Message: {error.Message}");
        Console.WriteLine($"  Position: {error.Position}");
        Console.WriteLine($"  Length: {error.Length}");
        Console.WriteLine($"  Line: {error.Line}");
        Console.WriteLine($"  Column: {error.Column}");
    }
}
else
{
    Console.WriteLine($"Parse successful! Found {result3.Document?.Properties.Count ?? 0} properties.");
}
