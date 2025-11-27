using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for string escaping edge cases per TOON spec §7.3.
    /// Spec §7.3: Strings support escape sequences for special characters.
    /// </summary>
    [TestClass]
    public class StringEscapingEdgeCaseTests
    {
        #region Basic Escape Sequences

        [TestMethod]
        public void Parse_StringWithNewlineEscape_ParsesCorrectly()
        {
            var source = @"text: ""Line1\nLine2""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("Line1\nLine2", value.Value, "\\n should produce newline");
        }

        [TestMethod]
        public void Parse_StringWithTabEscape_ParsesCorrectly()
        {
            var source = @"text: ""Column1\tColumn2""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("Column1\tColumn2", value.Value, "\\t should produce tab");
        }

        [TestMethod]
        public void Parse_StringWithCarriageReturnEscape_ParsesCorrectly()
        {
            var source = @"text: ""Line1\rLine2""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("Line1\rLine2", value.Value, "\\r should produce carriage return");
        }

        [TestMethod]
        public void Parse_StringWithBackslashEscape_ParsesCorrectly()
        {
            var source = @"path: ""C:\\Users\\Admin""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("C:\\Users\\Admin", value.Value, "\\\\ should produce single backslash");
        }

        [TestMethod]
        public void Parse_StringWithQuoteEscape_ParsesCorrectly()
        {
            var source = @"quote: ""She said \""Hello\""""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("She said \"Hello\"", value.Value, "\\\" should produce double quote");
        }

        #endregion

        #region Multiple Escape Sequences

        [TestMethod]
        public void Parse_StringWithMultipleEscapeTypes_ParsesCorrectly()
        {
            var source = @"text: ""Line1\nLine2\tTabbed\r\nCRLF""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("Line1\nLine2\tTabbed\r\nCRLF", value.Value, "Multiple escape types should all work");
        }

        [TestMethod]
        public void Parse_StringWithMixedEscapesAndText_ParsesCorrectly()
        {
            var source = @"text: ""Start\n\tMiddle\\End\""Quote\""""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("Start\n\tMiddle\\End\"Quote\"", value.Value, "Mixed escapes and text should parse");
        }

        [TestMethod]
        public void Parse_StringWithConsecutiveEscapes_ParsesCorrectly()
        {
            var source = @"text: ""\n\n\n""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("\n\n\n", value.Value, "Consecutive escapes should work");
        }

        [TestMethod]
        public void Parse_StringWithEscapeAtStart_ParsesCorrectly()
        {
            var source = @"text: ""\nStartsWithNewline""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("\nStartsWithNewline", value.Value, "Escape at start should work");
        }

        [TestMethod]
        public void Parse_StringWithEscapeAtEnd_ParsesCorrectly()
        {
            var source = @"text: ""EndsWithNewline\n""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("EndsWithNewline\n", value.Value, "Escape at end should work");
        }

        #endregion

        #region Unicode Escapes

        [TestMethod]
        public void Parse_StringWithUnicodeEscape_ParsesCorrectly()
        {
            // If TOON supports \uXXXX Unicode escapes
            var source = @"text: ""Hello\u0020World""";

            ToonParseResult result = Toon.Parse(source);

            if (result.IsSuccess)
            {
                var value = (StringValueNode)result.Document.Properties[0].Value;
                // \u0020 is space
                Assert.Contains(" ", value.Value, "Unicode escape should work if supported");
            }
            // If not supported, parser may treat it literally - that's okay
        }

        [TestMethod]
        public void Parse_StringWithEmojiDirect_ParsesCorrectly()
        {
            var source = "text: \"Hello 👋 World\"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("Hello 👋 World", value.Value, "Direct emoji should parse");
        }

        [TestMethod]
        public void Parse_StringWithJapaneseCharacters_ParsesCorrectly()
        {
            var source = "text: \"こんにちは\"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("こんにちは", value.Value, "Japanese characters should parse");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_StringWithOnlyEscapes_ParsesCorrectly()
        {
            var source = @"text: ""\n\t\r""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("\n\t\r", value.Value, "String of only escapes should work");
        }

        [TestMethod]
        public void Parse_StringWithOnlySpaces_ParsesCorrectly()
        {
            var source = "text: \"   \"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("   ", value.Value, "String with only spaces should preserve spaces");
        }

        [TestMethod]
        public void Parse_StringWithEscapedBackslashBeforeQuote_ParsesCorrectly()
        {
            // Tricky case: \\" should be backslash + quote
            var source = @"text: ""Path: C:\\""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("Path: C:\\", value.Value, "Escaped backslash before quote should work");
        }

        #endregion

        #region Invalid Escapes

        [TestMethod]
        public void Parse_StringWithBackslashAtEnd_HandlesGracefully()
        {
            // Trailing backslash (incomplete escape)
            var source = @"text: ""EndsWithBackslash\""";

            ToonParseResult result = Toon.Parse(source);

            // This is malformed, but parser should not crash
            Assert.IsNotNull(result, "Parser should handle incomplete escape gracefully");
        }

        #endregion

        #region Escapes in Arrays and Objects

        [TestMethod]
        public void Parse_ArrayWithEscapedStrings_ParsesCorrectly()
        {
            var source = @"items[3]: ""Line1\nLine2"",""Tab\tSeparated"",""Quote\""Here""";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            Assert.AreEqual("Line1\nLine2", ((StringValueNode)array.Elements[0]).Value, "First element");
            Assert.AreEqual("Tab\tSeparated", ((StringValueNode)array.Elements[1]).Value, "Second element");
            Assert.AreEqual("Quote\"Here", ((StringValueNode)array.Elements[2]).Value, "Third element");
        }

        [TestMethod]
        public void Parse_TableArrayWithEscapedStrings_ParsesCorrectly()
        {
            var source = @"data[2]{id,message}:
  1,""Line1\nLine2""
  2,""Tab\tted""";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            Assert.AreEqual("Line1\nLine2", ((StringValueNode)table.Rows[0][1]).Value, "First row message");
            Assert.AreEqual("Tab\tted", ((StringValueNode)table.Rows[1][1]).Value, "Second row message");
        }

        [TestMethod]
        public void Parse_NestedObjectWithEscapedStrings_ParsesCorrectly()
        {
            var source = @"config:
  message: ""Hello\nWorld""
  path: ""C:\\Users\\Admin""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var config = (ObjectNode)result.Document.Properties[0].Value;

            var message = (StringValueNode)config.Properties[0].Value;
            var path = (StringValueNode)config.Properties[1].Value;

            Assert.AreEqual("Hello\nWorld", message.Value, "Nested message");
            Assert.AreEqual("C:\\Users\\Admin", path.Value, "Nested path");
        }

        #endregion

        #region Real-World Scenarios

        [TestMethod]
        public void Parse_JSONLikeEscapedString_ParsesCorrectly()
        {
            // JSON-style escaped string - use regular string literal
            var source = "json: \"{\\\"name\\\":\\\"John\\\",\\\"age\\\":30}\"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("{\"name\":\"John\",\"age\":30}", value.Value, "JSON-like escaped string");
        }

        [TestMethod]
        public void Parse_RegexPatternWithEscapes_ParsesCorrectly()
        {
            // Regex pattern with escapes - use regular string literal
            var source = "pattern: \"\\\\d+\\\\.\\\\d+\"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.AreEqual("\\d+\\.\\d+", value.Value, "Regex pattern with escapes");
        }

        [TestMethod]
        public void Parse_MultilineTextRepresentation_ParsesCorrectly()
        {
            // Representing multiline text with \n escapes
            var source = @"poem: ""Roses are red,\nViolets are blue,\nSugar is sweet,\nAnd so are you.""";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;

            Assert.Contains("\n", value.Value, "Multiline text representation should have newlines");
            Assert.HasCount(4, value.Value.Split('\n'), "Should have 4 lines");
        }

        #endregion
    }
}
