using System;
using ToonTokenizer;

namespace ToonTokenizer.Examples
{
    /// <summary>
    /// Demonstrates JSONC (JSON with Comments) support in Toon.Encode.
    /// This allows encoding configuration files that contain comments, such as VS Code settings.json.
    /// </summary>
    public class JsoncExample
    {
        public static void Main()
        {
            Console.WriteLine("=== JSONC Support Example ===\n");

            // Example 1: Single-line comments
            Console.WriteLine("Example 1: Single-line comments");
            var jsoncWithSingleLineComments = @"{
  // Application configuration
  ""name"": ""MyApp"",
  ""version"": ""1.0.0"",  // Current version
  ""port"": 8080
}";

            Console.WriteLine("Input JSONC:");
            Console.WriteLine(jsoncWithSingleLineComments);
            Console.WriteLine("\nOutput TOON:");
            var toon1 = Toon.Encode(jsoncWithSingleLineComments);
            Console.WriteLine(toon1);

            // Example 2: Multi-line comments
            Console.WriteLine("\n\nExample 2: Multi-line comments");
            var jsoncWithMultiLineComments = @"{
  /* Server configuration
     These settings control the server behavior */
  ""host"": ""localhost"",
  ""port"": 3000,
  ""ssl"": true  /* Enable SSL in production */
}";

            Console.WriteLine("Input JSONC:");
            Console.WriteLine(jsoncWithMultiLineComments);
            Console.WriteLine("\nOutput TOON:");
            var toon2 = Toon.Encode(jsoncWithMultiLineComments);
            Console.WriteLine(toon2);

            // Example 3: Trailing commas
            Console.WriteLine("\n\nExample 3: Trailing commas");
            var jsoncWithTrailingCommas = @"{
  ""database"": {
    ""host"": ""localhost"",
    ""port"": 5432,
  },
  ""cache"": {
    ""enabled"": true,
    ""ttl"": 3600,
  }
}";

            Console.WriteLine("Input JSONC:");
            Console.WriteLine(jsoncWithTrailingCommas);
            Console.WriteLine("\nOutput TOON:");
            var toon3 = Toon.Encode(jsoncWithTrailingCommas);
            Console.WriteLine(toon3);

            // Example 4: Real-world VS Code settings
            Console.WriteLine("\n\nExample 4: Real-world VS Code settings.json");
            var vscodeSettings = @"{
  // Editor settings
  ""editor.fontSize"": 14,
  ""editor.tabSize"": 2,
  ""editor.wordWrap"": ""on"",
  
  /* Extension settings
     Configure various extensions */
  ""files.exclude"": {
    ""**/.git"": true,
    ""**/.DS_Store"": true,
  },
  
  // Language-specific settings
  ""[csharp]"": {
    ""editor.formatOnSave"": true,  // Auto-format on save
  }
}";

            Console.WriteLine("Input JSONC:");
            Console.WriteLine(vscodeSettings);
            Console.WriteLine("\nOutput TOON:");
            var toon4 = Toon.Encode(vscodeSettings);
            Console.WriteLine(toon4);

            // Example 5: Comments in arrays
            Console.WriteLine("\n\nExample 5: Comments in arrays");
            var jsoncWithArrayComments = @"{
  ""features"": [
    // Core features
    ""authentication"",
    ""authorization"",
    /* Optional features */
    ""analytics"",
    ""reporting"",  // Add more features here
  ]
}";

            Console.WriteLine("Input JSONC:");
            Console.WriteLine(jsoncWithArrayComments);
            Console.WriteLine("\nOutput TOON:");
            var toon5 = Toon.Encode(jsoncWithArrayComments);
            Console.WriteLine(toon5);

            Console.WriteLine("\n\n=== Benefits of JSONC Support ===");
            Console.WriteLine("✓ Works with VS Code settings files");
            Console.WriteLine("✓ Works with TypeScript tsconfig.json");
            Console.WriteLine("✓ Works with ESLint .eslintrc.json");
            Console.WriteLine("✓ Supports both // and /* */ comment styles");
            Console.WriteLine("✓ Allows trailing commas in objects and arrays");
            Console.WriteLine("✓ Comments are automatically stripped from output");
        }
    }
}
