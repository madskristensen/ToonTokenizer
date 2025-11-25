using System;
using ToonTokenizer;

namespace ToonTokenizer.Examples
{
    /// <summary>
    /// Demonstrates resilient parsing for language service scenarios.
    /// The parser continues after errors, collecting all issues while building a partial AST.
    /// </summary>
    public class ResilientParsingExample
    {
        public static void Main()
        {
            Console.WriteLine("=== Resilient Parsing Example ===\n");

            // Example 1: Document with multiple syntax errors
            var toonWithErrors = @"
name: John Doe
age 30
city: New York
job: Developer
salary invalid
department: Engineering
";

            Console.WriteLine("Parsing document with syntax errors:");
            Console.WriteLine(toonWithErrors);
            Console.WriteLine("\nResult:");

            var result = Toon.Parse(toonWithErrors);

            // Even with errors, we get a partial document
            Console.WriteLine($"- Parse completed: {result != null}");
            Console.WriteLine($"- Document available: {result.Document != null}");
            Console.WriteLine($"- Has errors: {result.HasErrors}");
            Console.WriteLine($"- Error count: {result.Errors.Count}");
            Console.WriteLine($"- Valid properties parsed: {result.Document.Properties.Count}");

            Console.WriteLine("\nErrors found:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  Line {error.Line}, Col {error.Column}: {error.Message}");
            }

            Console.WriteLine("\nSuccessfully parsed properties:");
            foreach (var prop in result.Document.Properties)
            {
                Console.WriteLine($"  {prop.Key}: {GetValueAsString(prop.Value)}");
            }

            // Example 2: Using TryParse for language service scenarios
            Console.WriteLine("\n\n=== TryParse for Language Services ===\n");

            var incompleteDocument = @"
user:
  firstName: Jane
  lastName: Smith
  contacts[2]{type,value}:
    email jane@example.com
    phone,555-1234
  settings:
    theme: dark
";

            bool success = Toon.TryParse(incompleteDocument, out var parseResult);
            
            Console.WriteLine($"TryParse returned: {success}");
            Console.WriteLine($"This allows IDE to:");
            Console.WriteLine("  - Display all errors at once (not just first error)");
            Console.WriteLine("  - Provide IntelliSense on valid parts");
            Console.WriteLine("  - Navigate partial AST structure");
            Console.WriteLine($"\nParsed structure has {parseResult.Document.Properties.Count} top-level properties");
            Console.WriteLine($"Found {parseResult.Errors.Count} syntax errors");

            if (parseResult.HasErrors)
            {
                Console.WriteLine("\nErrors that IDE can highlight:");
                foreach (var error in parseResult.Errors)
                {
                    Console.WriteLine($"  [{error.Line}:{error.Column}] {error.Message}");
                    Console.WriteLine($"    (Span: pos={error.Position}, len={error.Length})");
                }
            }

            Console.WriteLine("\n\n=== Benefits for Language Services ===");
            Console.WriteLine("1. Editor can show all errors at once (squiggly lines)");
            Console.WriteLine("2. IntelliSense works on correctly parsed sections");
            Console.WriteLine("3. Go-to-definition and hover work for valid properties");
            Console.WriteLine("4. Structure outline reflects partial document");
            Console.WriteLine("5. No need to fix errors one-by-one to see next error");
        }

        private static string GetValueAsString(ToonTokenizer.Ast.AstNode value)
        {
            return value switch
            {
                ToonTokenizer.Ast.StringValueNode s => s.Value,
                ToonTokenizer.Ast.NumberValueNode n => n.Value.ToString(),
                ToonTokenizer.Ast.BooleanValueNode b => b.Value.ToString(),
                ToonTokenizer.Ast.NullValueNode => "null",
                _ => $"[{value.GetType().Name}]"
            };
        }
    }
}
