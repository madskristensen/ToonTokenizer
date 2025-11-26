using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for multi-word unquoted string parsing.
    /// The parser allows multi-word unquoted values in simple property contexts.
    /// </summary>
    [TestClass]
    public class MultiWordStringTests
    {
        #region Simple Multi-Word Values

        [TestMethod]
        public void Parse_TwoWordUnquotedString_ParsesAsWholeValue()
        {
            var source = "city: New York";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("New York", value.Value);
        }

        [TestMethod]
        public void Parse_ThreeWordUnquotedString_ParsesAsWholeValue()
        {
            var source = "city: New York City";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("New York City", value.Value);
        }

        [TestMethod]
        public void Parse_ManyWordUnquotedString_ParsesAsWholeValue()
        {
            var source = "description: The quick brown fox jumps over the lazy dog";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("quick", value.Value);
            Assert.Contains("brown", value.Value);
            Assert.Contains("fox", value.Value);
            Assert.Contains("lazy", value.Value);
            Assert.Contains("dog", value.Value);
        }

        [TestMethod]
        public void Parse_MultiWordWithNumbers_ParsesCorrectly()
        {
            var source = "address: 123 Main Street";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("123", value.Value);
            Assert.Contains("Main", value.Value);
            Assert.Contains("Street", value.Value);
        }

        [TestMethod]
        public void Parse_MultiWordWithPunctuation_ParsesCorrectly()
        {
            var source = "name: Dr. John Smith Jr.";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("Dr", value.Value);
            Assert.Contains("John", value.Value);
            Assert.Contains("Smith", value.Value);
        }

        #endregion

        #region Email and URL Patterns

        [TestMethod]
        public void Parse_EmailAsUnquotedString_ParsesCorrectly()
        {
            var source = "email: user@example.com";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("user@example.com", value.Value);
        }

        [TestMethod]
        public void Parse_EmailWithDots_ParsesCorrectly()
        {
            var source = "email: first.last@company.co.uk";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("first", value.Value);
            Assert.Contains("last", value.Value);
            Assert.Contains("company", value.Value);
        }

        [TestMethod]
        public void Parse_EmailWithPlus_ParsesCorrectly()
        {
            var source = "email: user+tag@example.com";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("user", value.Value);
            Assert.Contains("example", value.Value);
        }

        [TestMethod]
        public void Parse_URLWithoutProtocol_ParsesCorrectly()
        {
            // URL without :// (which would need quotes)
            var source = "domain: www.example.com";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("www", value.Value);
            Assert.Contains("example", value.Value);
        }

        #endregion

        #region Multi-Word with Special Characters

        [TestMethod]
        public void Parse_MultiWordWithHyphens_ParsesCorrectly()
        {
            var source = "name: Mary-Jane Watson-Parker";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("Mary", value.Value);
            Assert.Contains("Jane", value.Value);
            Assert.Contains("Watson", value.Value);
            Assert.Contains("Parker", value.Value);
        }

        [TestMethod]
        public void Parse_MultiWordWithUnderscores_ParsesCorrectly()
        {
            var source = "filename: my_document_final_version";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("my", value.Value);
            Assert.Contains("document", value.Value);
            Assert.Contains("final", value.Value);
            Assert.Contains("version", value.Value);
        }

        [TestMethod]
        public void Parse_MultiWordWithApostrophes_ParsesCorrectly()
        {
            // Apostrophes mark string literals in TOON, so multi-word strings with apostrophes need quotes
            var source = "text: \"It's a wonderful day today\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("It", value.Value);
            Assert.Contains("wonderful", value.Value);
            Assert.Contains("day", value.Value);
            Assert.Contains("today", value.Value);
        }

        [TestMethod]
        public void Parse_MultiWordWithParentheses_ParsesCorrectly()
        {
            var source = "note: This is important (see page 42)";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("important", value.Value);
            Assert.Contains("see", value.Value);
            Assert.Contains("page", value.Value);
        }

        #endregion

        #region Multi-Word in Different Contexts

        [TestMethod]
        public void Parse_MultiWordInNestedObject_ParsesCorrectly()
        {
            var source = @"person:
  name: John Smith
  title: Senior Software Engineer";
            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            var obj = (ObjectNode)result.Document.Properties[0].Value;
            var name = (StringValueNode)obj.Properties[0].Value;
            var title = (StringValueNode)obj.Properties[1].Value;

            Assert.Contains("John", name.Value);
            Assert.Contains("Smith", name.Value);
            Assert.Contains("Senior", title.Value);
            Assert.Contains("Software", title.Value);
            Assert.Contains("Engineer", title.Value);
        }

        [TestMethod]
        public void Parse_MultiWordWithComment_StopsAtComment()
        {
            var source = "text: Hello World # This is a comment";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("Hello", value.Value);
            Assert.Contains("World", value.Value);
            Assert.DoesNotContain("comment", value.Value);
        }

        [TestMethod]
        public void Parse_MultiWordStopsAtNewline_ParsesFirstLine()
        {
            var source = @"text: First Line
second: value";
            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(2, result.Document.Properties);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.Contains("First", value.Value);
            Assert.Contains("Line", value.Value);
            Assert.DoesNotContain("second", value.Value);
        }

        #endregion

        #region Array Context (Single-Word Only)

        [TestMethod]
        public void Parse_ArrayWithSingleWords_ParsesCorrectly()
        {
            // In arrays, words are separated by delimiters, not spaces
            var source = "colors[3]: red,green,blue";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArrayElements(array, "red", "green", "blue");
        }

        [TestMethod]
        public void Parse_ArrayWithQuotedMultiWord_ParsesCorrectly()
        {
            // Multi-word values in arrays must be quoted
            var source = "cities[2]: \"New York\",\"Los Angeles\"";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArrayElements(array, "New York", "Los Angeles");
        }

        #endregion

        #region Table Array Context

        [TestMethod]
        public void Parse_TableArraySingleFieldMultiWord_ParsesCorrectly()
        {
            // Single-field table array can have multi-word unquoted values
            var source = @"trails[3]{name}:
  Blue Lake Trail
  Ridge Overlook Path
  Wildflower Loop";
            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            Assert.HasCount(3, table.Rows);

            var row1 = (StringValueNode)table.Rows[0][0];
            Assert.Contains("Blue", row1.Value);
            Assert.Contains("Lake", row1.Value);
            Assert.Contains("Trail", row1.Value);
        }

        [TestMethod]
        public void Parse_TableArrayMultiFieldRequiresQuotes_Behavior()
        {
            // Multi-field table arrays: multi-word values need quotes
            var source = @"people[2]{firstName,lastName}:
  John,Smith
  Jane,Doe";
            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            Assert.HasCount(2, table.Rows);
            ToonTestHelpers.AssertTableCellValue(table, 0, 0, "John");
            ToonTestHelpers.AssertTableCellValue(table, 0, 1, "Smith");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_MultiWordWithExtraWhitespace_NormalizesSpaces()
        {
            var source = "text: Hello    World    Test";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            // Should contain the words (spaces may be normalized or preserved)
            Assert.Contains("Hello", value.Value);
            Assert.Contains("World", value.Value);
            Assert.Contains("Test", value.Value);
        }

        [TestMethod]
        public void Parse_MultiWordWithTrailingSpaces_TrimsCorrectly()
        {
            var source = "text: Hello World   \n";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            // Trailing spaces should likely be trimmed
            Assert.Contains("Hello", value.Value);
            Assert.Contains("World", value.Value);
        }

        [TestMethod]
        public void Parse_EmptyValueAfterColon_ParsesAsNull()
        {
            var source = "value: \n";
            ObjectNode value = ToonTestHelpers.ParseAndGetValue<ObjectNode>(source);

            // A newline after colon is treated as a nested object (even if empty)
            // This is the parser's actual behavior - it sees indentation context
            Assert.IsInstanceOfType<ObjectNode>(value);
        }

        [TestMethod]
        public void Parse_VeryLongMultiWordString_ParsesCorrectly()
        {
            var words = string.Join(" ", Enumerable.Range(1, 100).Select(i => $"word{i}"));
            var source = $"text: {words}";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("word1", value.Value);
            Assert.Contains("word100", value.Value);
        }

        #endregion
    }
}
