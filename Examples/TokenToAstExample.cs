using System;
using ToonTokenizer;
using ToonTokenizer.Ast;

namespace Examples
{
    /// <summary>
    /// Examples demonstrating how to navigate from tokens to AST nodes.
    /// </summary>
    public class TokenToAstExample
    {
        public static void Main()
        {
            var source = @"
name: John Doe
age: 30
user:
  email: john@example.com
  settings:
    theme: dark
    notifications: true
scores[3]: 95,87,92
users[2]{id,name}:
  1,Alice
  2,Bob
";

            var result = Toon.Parse(source);

            Console.WriteLine("=== Example 1: From Token to AST Node ===");
            DemoTokenToNode(result);

            Console.WriteLine("\n=== Example 2: From Token to PropertyNode ===");
            DemoTokenToProperty(result);

            Console.WriteLine("\n=== Example 3: Find Node at Position ===");
            DemoFindNodeAtPosition(result);

            Console.WriteLine("\n=== Example 4: Find Property at Line/Column ===");
            DemoFindPropertyAtLineColumn(result);

            Console.WriteLine("\n=== Example 5: Get All Properties ===");
            DemoGetAllProperties(result);

            Console.WriteLine("\n=== Example 6: Find Property by Path ===");
            DemoFindPropertyByPath(result);
        }

        private static void DemoTokenToNode(ToonParseResult result)
        {
            // Find a token and get its AST node
            var token = result.Tokens.Find(t => t.Type == TokenType.Identifier && t.Value == "name");
            if (token != null)
            {
                var node = token.GetAstNode(result.Document);
                Console.WriteLine($"Token '{token.Value}' at {token.Line}:{token.Column}");
                Console.WriteLine($"  AST Node Type: {node?.GetType().Name}");
                if (node is PropertyNode prop)
                {
                    Console.WriteLine($"  Property Key: {prop.Key}");
                    Console.WriteLine($"  Property Value Type: {prop.Value.GetType().Name}");
                }
            }
        }

        private static void DemoTokenToProperty(ToonParseResult result)
        {
            // Find any token and get the property it belongs to
            var token = result.Tokens.Find(t => t.Type == TokenType.Number && t.Value == "30");
            if (token != null)
            {
                var property = token.GetPropertyNode(result.Document);
                Console.WriteLine($"Token '{token.Value}' at {token.Line}:{token.Column}");
                Console.WriteLine($"  Belongs to Property: {property?.Key}");
                if (property?.Value is NumberValueNode num)
                {
                    Console.WriteLine($"  Property Value: {num.Value}");
                }
            }
        }

        private static void DemoFindNodeAtPosition(ToonParseResult result)
        {
            // Find node at a specific character position
            // Let's find the "email" property (approximate position)
            var emailToken = result.Tokens.Find(t => t.Value == "email");
            if (emailToken != null)
            {
                var node = result.GetNodeAtPosition(emailToken.Position);
                Console.WriteLine($"Node at position {emailToken.Position}:");
                Console.WriteLine($"  Type: {node?.GetType().Name}");
                if (node is PropertyNode prop)
                {
                    Console.WriteLine($"  Key: {prop.Key}");
                    Console.WriteLine($"  Indent Level: {prop.IndentLevel}");
                }
            }
        }

        private static void DemoFindPropertyAtLineColumn(ToonParseResult result)
        {
            // Find property at specific line and column
            var property = result.GetPropertyAt(line: 3, column: 1);
            if (property != null)
            {
                Console.WriteLine($"Property at line 3, column 1:");
                Console.WriteLine($"  Key: {property.Key}");
                Console.WriteLine($"  Value Type: {property.Value.GetType().Name}");
            }
        }

        private static void DemoGetAllProperties(ToonParseResult result)
        {
            // Get all properties including nested ones
            var allProperties = result.GetAllProperties();
            Console.WriteLine($"Total properties in document: {allProperties.Count}");
            foreach (var prop in allProperties)
            {
                var indent = new string(' ', prop.IndentLevel * 2);
                Console.WriteLine($"{indent}{prop.Key}: {prop.Value.GetType().Name}");
            }
        }

        private static void DemoFindPropertyByPath(ToonParseResult result)
        {
            // Find nested property using dot notation
            var theme = result.FindPropertyByPath("user.settings.theme");
            if (theme != null)
            {
                Console.WriteLine($"Property 'user.settings.theme':");
                Console.WriteLine($"  Key: {theme.Key}");
                if (theme.Value is StringValueNode str)
                {
                    Console.WriteLine($"  Value: {str.Value}");
                }
                Console.WriteLine($"  Location: Line {theme.StartLine}, Column {theme.StartColumn}");
            }

            // Another example
            var email = result.FindPropertyByPath("user.email");
            if (email != null)
            {
                Console.WriteLine($"\nProperty 'user.email':");
                Console.WriteLine($"  Key: {email.Key}");
                if (email.Value is StringValueNode str)
                {
                    Console.WriteLine($"  Value: {str.Value}");
                }
            }
        }
    }
}
