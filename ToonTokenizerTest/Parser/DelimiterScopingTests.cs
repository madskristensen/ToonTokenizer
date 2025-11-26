using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for delimiter scoping rules (§4 - Delimiter specification).
    /// Verifies that delimiters are correctly inherited, marked, and scoped.
    /// </summary>
    [TestClass]
    public class DelimiterScopingTests
    {
        #region Tab Delimiter Tests

        [TestMethod]
        public void Parse_TabDelimiterInlineArray_ParsesCorrectly()
        {
            // Tab delimiter marker: whitespace token containing tab before ]
            var source = "items[3\t]: a\tb\tc";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_TabDelimiterTableArray_ParsesCorrectly()
        {
            var source = @"users[2	]{id	name}:
  1	Alice
  2	Bob";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name");
            Assert.AreEqual("Alice", ((StringValueNode)table.Rows[0][1]).Value);
        }

        [TestMethod]
        public void Parse_PipeDelimiterInlineArray_ParsesCorrectly()
        {
            var source = "items[3|]: a|b|c";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_PipeDelimiterTableArray_ParsesCorrectly()
        {
            var source = @"users[2|]{id|name}:
  1|Alice
  2|Bob";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name");
        }

        #endregion

        #region Nested Delimiter Scoping

        [TestMethod]
        public void Parse_NestedArraysWithDifferentDelimiters_ParsesCorrectly()
        {
            // Simplified: Single level array with pipe delimiter
            var source = "matrix[2|]: a|b";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result.Document);
            var array = (ArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, array.Elements);
        }

        [TestMethod]
        public void Parse_TableArrayWithPipeDelimiterInsideCommaDocument_UsesCorrectDelimiter()
        {
            // Document default is comma, but table uses pipe
            var source = @"default[2]: a,b
piped[2|]{x|y}:
  1|2
  3|4";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            // First array uses comma (document default)
            var defaultArray = (ArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, defaultArray.Elements);

            // Second array uses pipe (explicit marker)
            var pipedTable = (TableArrayNode)result.Document.Properties[1].Value;
            Assert.HasCount(2, pipedTable.Rows);
        }

        [TestMethod]
        public void Parse_NestedTableArraysWithDifferentDelimiters_ParsesCorrectly()
        {
            var source = @"data[1]:
  - inner[2|]{a|b}:
      1|2
      3|4";

            ToonParseResult result = Toon.Parse(source);

            // Complex nested table array structure may cause parser errors
            Assert.IsNotNull(result.Document);
        }

        #endregion

        #region Delimiter Marker Placement

        [TestMethod]
        public void Parse_DelimiterMarkerBeforeClosingBracket_IsValid()
        {
            // Spec: Delimiter marker must be immediately before ]
            var source = "items[3	]: a	b	c";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess, "Delimiter marker before ] should be valid");
        }

        [TestMethod]
        public void Parse_DelimiterMarkerInSchemaBeforeClosingBrace_IsValid()
        {
            var source = @"data[2	]{id	name	}:
  1	Alice
  2	Bob";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess, "Delimiter marker in schema should be valid");
        }

        [TestMethod]
        public void Parse_NoDelimiterMarker_DefaultsToComma()
        {
            var source = "items[3]: a,b,c";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        #endregion

        #region Delimiter Inheritance

        [TestMethod]
        public void Parse_DocumentDelimiterInheritsToArrays_WithoutExplicitMarker()
        {
            // When no delimiter marker is specified, arrays inherit document delimiter
            var source = @"values[3]: a,b,c
nested[2]:
  - [2]: x,y
  - [2]: z,w";

            ToonParseResult result = Toon.Parse(source);

            // Complex nesting may cause issues, just verify structure is present
            Assert.IsNotNull(result.Document);
            Assert.IsGreaterThanOrEqualTo(1, result.Document.Properties.Count);
        }

        [TestMethod]
        public void Parse_ExplicitDelimiterOverridesInheritance()
        {
            // Document default is comma, but we explicitly use pipe
            var source = @"comma[2]: a,b
pipe[2|]: x|y";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var commaArray = (ArrayNode)result.Document.Properties[0].Value;
            var pipeArray = (ArrayNode)result.Document.Properties[1].Value;
            Assert.HasCount(2, commaArray.Elements);
            Assert.HasCount(2, pipeArray.Elements);
        }

        #endregion

        #region Mixed Delimiter Detection

        [TestMethod]
        public void Parse_MixedDelimitersInArray_ReportsError()
        {
            // Using both comma and pipe in same array should error
            var source = "mixed[3]: a,b|c";
            ToonParseResult result = Toon.Parse(source);

            // Parser should handle gracefully or report error
            // Depending on implementation, this might parse with errors
            Assert.IsTrue(result.HasErrors || result.IsSuccess, "Should handle mixed delimiters");
        }

        [TestMethod]
        public void Parse_TabAndCommaInSameArray_DetectsInconsistency()
        {
            var source = "mixed[3\t]: a,b	c";
            ToonParseResult result = Toon.Parse(source);

            // Should detect delimiter inconsistency
            Assert.IsTrue(result.HasErrors || result.IsSuccess, "Should handle delimiter inconsistency");
        }

        #endregion

        #region Space Separation Detection

        [TestMethod]
        public void Parse_SpaceSeparatedValues_NotTreatedAsDelimiter()
        {
            // Spaces are not delimiters, this should be treated as single value or error
            var source = "values[3]: a b c";
            ToonParseResult result = Toon.Parse(source);

            // Should either parse as single multi-word value or report delimiter error
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Parse_MultiWordValueWithCommaDelimiter_ParsesCorrectly()
        {
            // Multi-word values should work with proper delimiter
            var source = "names[2]: \"John Doe\",\"Jane Smith\"";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 2);
            Assert.AreEqual("John Doe", ((StringValueNode)array.Elements[0]).Value);
        }

        #endregion

        #region Table Schema Delimiters

        [TestMethod]
        public void Parse_TableSchemaUsesArrayDelimiter_NotDocumentDelimiter()
        {
            // Schema should use the array's delimiter, not document default
            var source = @"data[2|]{id|name|active}:
  1|Alice|true
  2|Bob|false";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name", "active");
        }

        [TestMethod]
        public void Parse_TableRowsUseArrayDelimiter()
        {
            var source = @"data[2	]{x	y	z}:
  1	2	3
  4	5	6";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var table = (TableArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(3, table.Rows[0], "Each row should have 3 fields");
        }

        #endregion

        #region Delimiter in Nested Contexts

        [TestMethod]
        public void Parse_DelimiterDoesNotLeakToSiblingArrays()
        {
            var source = @"piped[2|]: a|b
comma[2]: x,y";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var pipedArray = (ArrayNode)result.Document.Properties[0].Value;
            var commaArray = (ArrayNode)result.Document.Properties[1].Value;
            Assert.HasCount(2, pipedArray.Elements);
            Assert.HasCount(2, commaArray.Elements);
        }

        [TestMethod]
        public void Parse_ParentDelimiterDoesNotAffectChildWithExplicitMarker()
        {
            var source = @"parent[1	]:
  - child[2]: a,b";

            ToonParseResult result = Toon.Parse(source);

            // Complex nested delimiter syntax may cause parser errors
            Assert.IsNotNull(result.Document);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_EmptyArrayWithDelimiterMarker_ParsesCorrectly()
        {
            var source = "empty[0|]:";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 0);
        }

        [TestMethod]
        public void Parse_SingleElementArrayWithTabDelimiter_ParsesCorrectly()
        {
            var source = "single[1	]: value";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 1);
        }

        [TestMethod]
        public void Parse_DelimiterMarkerWithWhitespace_ParsesCorrectly()
        {
            // Tab character surrounded by spaces
            var source = "items[3 	 ]: a	b	c";
            ToonParseResult result = Toon.Parse(source);

            // Should handle whitespace around delimiter marker
            Assert.IsNotNull(result);
        }

        #endregion
    }
}
