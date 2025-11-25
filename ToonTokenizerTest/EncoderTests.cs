using ToonTokenizer;

namespace ToonTokenizerTest
{
    [TestClass]
    public class EncoderTests
    {
        [TestMethod]
        public void Encode_SimpleObject_ProducesCorrectToon()
        {
            var json = @"{""name"":""John"",""age"":30,""active"":true}";
            var toon = Toon.Encode(json);

            Assert.Contains("name: John", toon);
            Assert.Contains("age: 30", toon);
            Assert.Contains("active: true", toon);
        }

        [TestMethod]
        public void Encode_NestedObject_ProducesCorrectToon()
        {
            var json = @"{""user"":{""name"":""Jane"",""email"":""jane@example.com""}}";
            var toon = Toon.Encode(json);

            Assert.Contains("user:", toon);
            Assert.Contains("  name: Jane", toon);
            Assert.Contains("  email: jane@example.com", toon);
        }

        [TestMethod]
        public void Encode_SimpleArray_ProducesInlineArray()
        {
            var json = @"{""colors"":[""red"",""green"",""blue""]}";
            var toon = Toon.Encode(json);

            Assert.Contains("colors[3]: red,green,blue", toon);
        }

        [TestMethod]
        public void Encode_ArrayOfNumbers_ProducesInlineArray()
        {
            var json = @"{""numbers"":[1,2,3,4,5]}";
            var toon = Toon.Encode(json);

            Assert.Contains("numbers[5]: 1,2,3,4,5", toon);
        }

        [TestMethod]
        public void Encode_ArrayOfObjects_ProducesTableArray()
        {
            var json = @"{""users"":[{""id"":1,""name"":""Alice""},{""id"":2,""name"":""Bob""}]}";
            var toon = Toon.Encode(json);

            Assert.Contains("users[2]{id,name}:", toon);
            Assert.Contains("1,Alice", toon);
            Assert.Contains("2,Bob", toon);
        }

        [TestMethod]
        public void Encode_ComplexStructure_ProducesCorrectToon()
        {
            var json = @"{
                ""app"": {
                    ""name"": ""MyApp"",
                    ""version"": ""1.0"",
                    ""features"": [""auth"", ""api"", ""ui""]
                }
            }";
            var toon = Toon.Encode(json);

            Assert.Contains("app:", toon);
            Assert.Contains("name: MyApp", toon);
            Assert.Contains("version: \"1.0\"", toon);
            Assert.Contains("features[3]: auth,api,ui", toon);
        }

        [TestMethod]
        public void Encode_StringWithSpaces_QuotesValue()
        {
            var json = @"{""message"":""Hello World""}";
            var toon = Toon.Encode(json);

            Assert.Contains("message: \"Hello World\"", toon);
        }

        [TestMethod]
        public void Encode_NullValue_ProducesNull()
        {
            var json = @"{""optional"":null}";
            var toon = Toon.Encode(json);

            Assert.Contains("optional: null", toon);
        }

        [TestMethod]
        public void Encode_BooleanValues_ProducesCorrectFormat()
        {
            var json = @"{""enabled"":true,""debug"":false}";
            var toon = Toon.Encode(json);

            Assert.Contains("enabled: true", toon);
            Assert.Contains("debug: false", toon);
        }

        [TestMethod]
        public void Encode_FloatingPointNumbers_PreservesDecimals()
        {
            var json = @"{""pi"":3.14,""e"":2.718}";
            var toon = Toon.Encode(json);

            Assert.Contains("pi: 3.14", toon);
            Assert.Contains("e: 2.718", toon);
        }

        [TestMethod]
        public void Encode_WithCustomIndent_UsesCorrectSpacing()
        {
            var json = @"{""user"":{""name"":""Test""}}";
            var options = new ToonEncoderOptions { IndentSize = 4 };
            var toon = Toon.Encode(json, options);

            Assert.Contains("    name: Test", toon);
        }

        [TestMethod]
        public void Encode_TableArraysDisabled_UsesListFormat()
        {
            var json = @"{""items"":[{""a"":1},{""a"":2}]}";
            var options = new ToonEncoderOptions { UseTableArrays = false };
            var toon = Toon.Encode(json, options);

            Assert.Contains("items[2]:", toon);
            Assert.Contains("- ", toon);
        }

        [TestMethod]
        public void Encode_EmptyArray_ProducesEmptyArrayNotation()
        {
            var json = @"{""items"":[]}";
            var toon = Toon.Encode(json);

            Assert.Contains("items[0]:", toon);
        }

        [TestMethod]
        public void Encode_NestedArrays_ProducesCorrectFormat()
        {
            var json = @"{""matrix"":[[1,2],[3,4]]}";
            var toon = Toon.Encode(json);

            Assert.Contains("matrix[2]:", toon);
            Assert.Contains("- [2]: 1,2", toon);
            Assert.Contains("- [2]: 3,4", toon);
        }

        [TestMethod]
        public void Encode_RoundTrip_ParsesBackToSameStructure()
        {
            var json = @"{""name"":""Test"",""value"":42,""active"":true}";
            var toon = Toon.Encode(json);
            var result = Toon.Parse(toon);

            Assert.HasCount(3, result.Document!.Properties);
            Assert.AreEqual("name", result.Document!.Properties[0].Key);
            Assert.AreEqual("value", result.Document!.Properties[1].Key);
            Assert.AreEqual("active", result.Document!.Properties[2].Key);
        }

        // JSONC (JSON with Comments) Support Tests

        [TestMethod]
        public void Encode_JsoncWithSingleLineComment_IgnoresComment()
        {
            // JSONC format with single-line comment
            var jsonc = @"{
  // This is a comment
  ""name"": ""John"",
  ""age"": 30  // inline comment
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("name: John", toon);
            Assert.Contains("age: 30", toon);
            Assert.DoesNotContain("//", toon, "Should not contain comment markers");
            Assert.DoesNotContain("This is a comment", toon, "Should not contain comment text");
        }

        [TestMethod]
        public void Encode_JsoncWithMultiLineComment_IgnoresComment()
        {
            // JSONC format with multi-line comment
            var jsonc = @"{
  /* This is a
     multi-line comment */
  ""name"": ""John"",
  ""age"": 30
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("name: John", toon);
            Assert.Contains("age: 30", toon);
            Assert.DoesNotContain("/*", toon, "Should not contain comment start marker");
            Assert.DoesNotContain("*/", toon, "Should not contain comment end marker");
            Assert.DoesNotContain("multi-line", toon, "Should not contain comment text");
        }

        [TestMethod]
        public void Encode_JsoncWithTrailingCommas_HandlesGracefully()
        {
            // JSONC format with trailing commas
            var jsonc = @"{
  ""name"": ""John"",
  ""age"": 30,
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("name: John", toon);
            Assert.Contains("age: 30", toon);
        }

        [TestMethod]
        public void Encode_JsoncWithTrailingCommasInArray_HandlesGracefully()
        {
            var jsonc = @"{
  ""items"": [
    ""a"",
    ""b"",
    ""c"",
  ]
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("items[3]:", toon);
            Assert.Contains("a,b,c", toon);
        }

        [TestMethod]
        public void Encode_ComplexJsoncDocument_ConvertsSuccessfully()
        {
            var jsonc = @"{
  // Application configuration
  ""app"": {
    ""name"": ""MyApp"",
    ""version"": ""1.0.0"",  // Current version
    /* Features enabled
       for this release */
    ""features"": [""auth"", ""api"", ""ui""]
  },
  // User database
  ""users"": [
    {""id"": 1, ""name"": ""Alice"", ""role"": ""admin""},  // Admin user
    {""id"": 2, ""name"": ""Bob"", ""role"": ""user""},    // Regular user
  ]
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("app:", toon);
            Assert.Contains("name: MyApp", toon);
            Assert.Contains("version: 1.0.0", toon);
            Assert.Contains("features[3]:", toon);
            Assert.Contains("users[2]{id,name,role}:", toon);
            Assert.DoesNotContain("//", toon, "Should not contain single-line comment markers");
            Assert.DoesNotContain("/*", toon, "Should not contain multi-line comment start");
            Assert.DoesNotContain("*/", toon, "Should not contain multi-line comment end");
            Assert.DoesNotContain("Application configuration", toon, "Should not contain comment text");
        }

        [TestMethod]
        public void Encode_JsoncWithMixedCommentStyles_HandlesAll()
        {
            var jsonc = @"{
  // Single line before
  /* Block comment before */
  ""field1"": ""value1"",  // Inline single
  ""field2"": ""value2""  /* Inline block */
  // Single line after
  /* Block comment after */
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("field1: value1", toon);
            Assert.Contains("field2: value2", toon);
            Assert.DoesNotContain("//", toon);
            Assert.DoesNotContain("/*", toon);
            Assert.DoesNotContain("*/", toon);
        }

        [TestMethod]
        public void Encode_JsoncCommentInArray_IgnoresComment()
        {
            var jsonc = @"{
  ""items"": [
    // First item
    ""a"",
    /* Second item */
    ""b"",
    ""c""  // Last item
  ]
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("items[3]: a,b,c", toon);
            Assert.DoesNotContain("First item", toon);
        }

        [TestMethod]
        public void Encode_JsoncCommentInNestedObject_IgnoresComment()
        {
            var jsonc = @"{
  ""config"": {
    // Server settings
    ""host"": ""localhost"",
    /* Port configuration
       Default: 8080 */
    ""port"": 8080
  }
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("config:", toon);
            Assert.Contains("host: localhost", toon);
            Assert.Contains("port: 8080", toon);
            Assert.DoesNotContain("Server settings", toon);
            Assert.DoesNotContain("Port configuration", toon);
        }

        [TestMethod]
        public void Encode_RealWorldJsoncConfigFile_ConvertsSuccessfully()
        {
            // Simulates a real VS Code settings.json file
            var jsonc = @"{
  // Editor settings
  ""editor.fontSize"": 14,
  ""editor.tabSize"": 2,
  ""editor.wordWrap"": ""on"",
  
  /* Extension settings
     These control various extensions */
  ""files.exclude"": {
    ""**/.git"": true,
    ""**/.DS_Store"": true,
  },
  
  // Language-specific settings
  ""[csharp]"": {
    ""editor.formatOnSave"": true,  // Auto-format
  }
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.Contains("editor.fontSize: 14", toon);
            Assert.Contains("editor.tabSize: 2", toon);
            Assert.DoesNotContain("Editor settings", toon);
            Assert.DoesNotContain("Extension settings", toon);
            Assert.DoesNotContain("Auto-format", toon);
        }

        [TestMethod]
        public void Encode_JsoncEmptyObjectWithComments_ReturnsEmpty()
        {
            var jsonc = @"{
  // This is an empty object
}";

            var toon = Toon.Encode(jsonc);

            Assert.IsNotNull(toon);
            Assert.AreEqual("", toon.Trim());
        }
    }
}
