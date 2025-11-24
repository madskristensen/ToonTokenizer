using ToonTokenizer;

// Example 1: Using TryParse with span information
Console.WriteLine("=== Example 1: TryParse with error spans ===");
var source1 = @"name John
age: 30";

if (!Toon.TryParse(source1, out var errors))
{
    Console.WriteLine($"Found {errors.Count} error(s):");
    foreach (var error in errors)
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

if (!Toon.TryParse(source2, out var errors2))
{
    Console.WriteLine($"Found {errors2.Count} error(s):");
    foreach (var error in errors2)
    {
        Console.WriteLine($"  Error: {error}");
    }
}
else
{
    Console.WriteLine("Document parsed successfully!");
}

Console.WriteLine();

// Example 3: Catching ParseException directly
Console.WriteLine("=== Example 3: ParseException with span info ===");
var source3 = "items[3: incomplete";

try
{
    var doc = Toon.Parse(source3);
}
catch (ParseException ex)
{
    Console.WriteLine($"ParseException caught:");
    Console.WriteLine($"  Message: {ex.Message}");
    Console.WriteLine($"  Position: {ex.Position}");
    Console.WriteLine($"  Length: {ex.Length}");
    Console.WriteLine($"  Line: {ex.Line}");
    Console.WriteLine($"  Column: {ex.Column}");
}
