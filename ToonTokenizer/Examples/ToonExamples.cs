using System;
using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizer.Examples
{
    /// <summary>
    /// Example usage of the TOON tokenizer and parser.
    /// </summary>
    public class ToonExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== TOON Tokenizer Examples ===\n");

            SimplePropertyExample();
            NestedObjectExample();
            ArrayExample();
            TableArrayExample();
            ComplexExample();
        }

        private static void SimplePropertyExample()
        {
            Console.WriteLine("1. Simple Property:");
            
            string toonSource = @"name: John Doe
age: 30
active: true";

            var document = Toon.Parse(toonSource);
            Console.WriteLine($"Properties count: {document.Properties.Count}");
            
            foreach (var prop in document.Properties)
            {
                Console.WriteLine($"  {prop.Key}: {GetValueString(prop.Value)}");
            }
            Console.WriteLine();
        }

        private static void NestedObjectExample()
        {
            Console.WriteLine("2. Nested Object:");
            
            string toonSource = @"user:
  name: Jane Smith
  email: jane@example.com
  settings:
    theme: dark
    notifications: true";

            var document = Toon.Parse(toonSource);
            Console.WriteLine(document.ToDebugString());
        }

        private static void ArrayExample()
        {
            Console.WriteLine("3. Simple Array:");
            
            string toonSource = @"colors[3]: red,green,blue
numbers[4]: 1,2,3,4";

            var document = Toon.Parse(toonSource);
            
            foreach (var prop in document.Properties)
            {
                Console.WriteLine($"  {prop.Key}:");
                if (prop.Value is ArrayNode arr)
                {
                    foreach (var elem in arr.Elements)
                    {
                        Console.WriteLine($"    - {GetValueString(elem)}");
                    }
                }
            }
            Console.WriteLine();
        }

        private static void TableArrayExample()
        {
            Console.WriteLine("4. Table Array:");
            
            string toonSource = @"users[3]{id,name,age}:
  1,Alice,25
  2,Bob,30
  3,Charlie,35";

            var document = Toon.Parse(toonSource);
            
            if (document.Properties.Count > 0 && document.Properties[0].Value is TableArrayNode table)
            {
                Console.WriteLine($"  Schema: {string.Join(", ", table.Schema)}");
                Console.WriteLine($"  Rows:");
                foreach (var row in table.Rows)
                {
                    var values = new System.Collections.Generic.List<string>();
                    foreach (var val in row)
                    {
                        values.Add(GetValueString(val));
                    }
                    Console.WriteLine($"    {string.Join(", ", values)}");
                }
            }
            Console.WriteLine();
        }

        private static void ComplexExample()
        {
            Console.WriteLine("5. Complex Example (from TOON spec):");
            
            string toonSource = @"context:
  task: Our favorite hikes together
  location: Boulder
  season: spring_2025

friends[3]: ana,luis,sam

hikes[3]{id,name,distanceKm,elevationGain,companion,wasSunny}:
  1,Blue Lake Trail,7.5,320,ana,true
  2,Ridge Overlook,9.2,540,luis,false
  3,Wildflower Loop,5.1,180,sam,true";

            var document = Toon.Parse(toonSource);
            Console.WriteLine(document.ToDebugString());
        }

        private static string GetValueString(AstNode node)
        {
            switch (node)
            {
                case StringValueNode str:
                    return $"\"{str.Value}\"";
                case NumberValueNode num:
                    return num.Value.ToString();
                case BooleanValueNode boolVal:
                    return boolVal.Value.ToString().ToLower();
                case NullValueNode _:
                    return "null";
                default:
                    return node.GetType().Name;
            }
        }
    }
}
