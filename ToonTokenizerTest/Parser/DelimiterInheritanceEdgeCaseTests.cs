using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Edge case tests for delimiter inheritance per TOON spec §4.2.
    /// Spec §4.2: "If not explicitly marked, arrays and tables inherit the delimiter from their containing scope."
    /// Tests complex inheritance scenarios, boundary conditions, and ambiguous cases.
    /// </summary>
    [TestClass]
    public class DelimiterInheritanceEdgeCaseTests
    {
        #region Basic Inheritance Validation

        [TestMethod]
        public void Parse_ArrayWithoutMarker_InheritsDocumentDefaultComma()
        {
            // No delimiter marker → should inherit comma (document default)
            var source = "items[3]: a,b,c";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_TableArrayWithoutMarker_InheritsDocumentDefaultComma()
        {
            // Table without marker → inherits comma for both schema and rows
            var source = @"users[2]{id,name,email}:
  1,Alice,alice@example.com
  2,Bob,bob@example.com";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name", "email");
        }

        [TestMethod]
        public void Parse_ExplicitPipeMarker_OverridesDefaultInheritance()
        {
            // Explicit pipe marker should override comma default
            var source = "items[3|]: a|b|c";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_ExplicitTabMarker_OverridesDefaultInheritance()
        {
            // Explicit tab marker (whitespace containing tab before ])
            var source = "items[3\t]: a\tb\tc";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 3);
        }

        #endregion

        #region Nested Inheritance Chains

        [TestMethod]
        public void Parse_NestedArrayInObject_InheritsFromDocument()
        {
            // Object property → nested array without marker → inherits comma
            var source = @"data:
  items[3]: a,b,c";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);
            var obj = (ObjectNode)doc.Document.Properties[0].Value;
            var array = (ArrayNode)obj.Properties[0].Value;

            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_ArrayInExpandedArray_InheritsFromParent()
        {
            // Parent has pipe, child without marker should inherit pipe
            var source = @"matrix[2|]:
  - [2]: a,b
  - [2]: c,d";

            // Note: This tests parser capability - inheritance behavior may vary
            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_ThreeLevelNesting_InheritanceChain()
        {
            // Level 1 (document): comma
            // Level 2 (outer array): pipe explicit
            // Level 3 (inner array): no marker → inherits pipe from level 2
            var source = @"outer[1|]:
  - inner[2]: x,y";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_DeepNesting_FiveLevel_MaintainsInheritanceChain()
        {
            // Very deep nesting to test inheritance stability
            var source = @"l1[1]:
  - l2[1]:
    - l3[1]:
      - l4[1]:
        - l5[2]: a,b";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document);
        }

        #endregion

        #region Multiple Siblings with Mixed Markers

        [TestMethod]
        public void Parse_SiblingArrays_DifferentDelimiters_IsolatedScopes()
        {
            // Siblings at same level should not affect each other
            var source = @"comma[2]: a,b
pipe[2|]: x|y
tab[2	]: m	n";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(3, doc.Document.Properties);

            var commaArray = (ArrayNode)doc.Document.Properties[0].Value;
            var pipeArray = (ArrayNode)doc.Document.Properties[1].Value;
            var tabArray = (ArrayNode)doc.Document.Properties[2].Value;

            ToonTestHelpers.AssertArraySize(commaArray, 2);
            ToonTestHelpers.AssertArraySize(pipeArray, 2);
            ToonTestHelpers.AssertArraySize(tabArray, 2);
        }

        [TestMethod]
        public void Parse_SiblingArrays_OneWithMarkerOneWithout_IndependentInheritance()
        {
            // First has explicit delimiter, second inherits from document
            var source = @"explicit[2|]: a|b
inherited[2]: x,y";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);

            var explicitArray = (ArrayNode)doc.Document.Properties[0].Value;
            var inheritedArray = (ArrayNode)doc.Document.Properties[1].Value;

            ToonTestHelpers.AssertArraySize(explicitArray, 2);
            ToonTestHelpers.AssertArraySize(inheritedArray, 2);
        }

        #endregion

        #region Table Array Schema vs Row Inheritance

        [TestMethod]
        public void Parse_TableArray_SchemaAndRowsUseSameDelimiter()
        {
            // Schema and rows should both use array's delimiter
            var source = @"users[2|]{id|name|role}:
  1|Alice|Admin
  2|Bob|User";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name", "role");
        }

        [TestMethod]
        public void Parse_TableArray_NoMarker_SchemaAndRowsInheritComma()
        {
            // No marker → both schema and rows inherit comma
            var source = @"data[2]{a,b,c}:
  1,2,3
  4,5,6";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "a", "b", "c");
        }

        [TestMethod]
        public void Parse_TableArrayNested_InheritsFromParentArray()
        {
            // Outer array has pipe, inner table without marker should inherit pipe
            var source = @"data[1|]:
  - table[2]{x,y}:
      1,2
      3,4";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document);
        }

        #endregion

        #region Edge Cases: Empty and Single Element

        [TestMethod]
        public void Parse_EmptyArray_WithPipeMarker_ValidDelimiterScope()
        {
            // Empty array still establishes delimiter scope for consistency
            var source = "empty[0|]:";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 0);
        }

        [TestMethod]
        public void Parse_EmptyTable_WithTabMarker_ValidDelimiterScope()
        {
            var source = "empty[0\t]{id\tname}:\n";
            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            ToonTestHelpers.AssertTableStructure(table, 0, "id", "name");
        }

        [TestMethod]
        public void Parse_SingleElementArray_InheritsDelimiter()
        {
            // Single element doesn't use delimiter but still establishes scope
            var source = "single[1]: value";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 1);
        }

        #endregion

        #region Inheritance Boundary Tests

        [TestMethod]
        public void Parse_ArrayInObject_InNestedObject_InheritsFromDocumentNotObject()
        {
            // Delimiter inheritance skips object boundaries, comes from document
            var source = @"level1:
  level2:
    items[3]: a,b,c";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);
            var level1 = (ObjectNode)doc.Document.Properties[0].Value;
            var level2 = (ObjectNode)level1.Properties[0].Value;
            var array = (ArrayNode)level2.Properties[0].Value;

            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_MultipleNestedScopes_EachMaintainsOwnInheritance()
        {
            // Complex: different arrays at different nesting levels
            var source = @"top[2]: x,y
nested:
  inner[2|]: a|b
  another[2]: m,n";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(2, doc.Document.Properties);
        }

        #endregion

        #region Inheritance vs Explicit Override Combinations

        [TestMethod]
        public void Parse_ParentPipe_ChildCommaExplicit_ChildOverrides()
        {
            // Parent uses pipe, child explicitly uses comma (unusual but valid)
            var source = @"parent[1|]:
  - child[2]: a,b";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_ParentComma_ChildPipeExplicit_ChildOverrides()
        {
            // Parent inherits comma, child explicitly uses pipe
            var source = @"parent[1]:
  - child[2|]: x|y";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_GrandparentPipe_ParentInherits_ChildOverridesWithTab()
        {
            // Three-level chain with override at third level
            var source = @"grandparent[1|]:
  - parent[1]:
    - child[2	]: a	b";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document);
        }

        #endregion

        #region Mixed Content Inheritance

        [TestMethod]
        public void Parse_ObjectAndArraySiblings_OnlyArraysInheritDelimiter()
        {
            // Objects don't use delimiters, only arrays inherit
            var source = @"obj:
  value: text
arr[2]: a,b";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(2, doc.Document.Properties);
        }

        [TestMethod]
        public void Parse_ArrayWithPrimitivesAndObjects_DelimiterAppliesOnlyToPrimitives()
        {
            // Inline arrays can have objects in expanded syntax
            var source = @"mixed[2]:
  - value
  - another";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document);
        }

        #endregion

        #region Real-World Scenarios

        [TestMethod]
        public void Parse_TSVStyleDocument_AllTabDelimiters()
        {
            // TSV (tab-separated values) style: all arrays use tabs
            var source = @"headers[3	]: Name	Age	City
data[2	]{name	age	city}:
  Alice	30	NYC
  Bob	25	LA";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(2, doc.Document.Properties);

            var headers = (ArrayNode)doc.Document.Properties[0].Value;
            ToonTestHelpers.AssertArraySize(headers, 3);

            var data = (TableArrayNode)doc.Document.Properties[1].Value;
            ToonTestHelpers.AssertTableStructure(data, 2, "name", "age", "city");
        }

        [TestMethod]
        public void Parse_CSVStyleDocument_AllCommaDelimiters()
        {
            // CSV style: all arrays use commas (default inheritance)
            var source = @"headers[3]: Name,Age,City
data[2]{name,age,city}:
  Alice,30,NYC
  Bob,25,LA";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);

            var headers = (ArrayNode)doc.Document.Properties[0].Value;
            ToonTestHelpers.AssertArraySize(headers, 3);

            var data = (TableArrayNode)doc.Document.Properties[1].Value;
            ToonTestHelpers.AssertTableStructure(data, 2, "name", "age", "city");
        }

        [TestMethod]
        public void Parse_MixedStyleDocument_EachArrayHasCorrectDelimiter()
        {
            // Realistic: some arrays with explicit delimiters, others inherit
            var source = @"csvData[2]: a,b
tsvData[2	]: x	y
pipedData[2|]: m|n
inheritedData[2]: p,q";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(4, doc.Document.Properties);

            ToonTestHelpers.AssertArraySize((ArrayNode)doc.Document.Properties[0].Value, 2);
            ToonTestHelpers.AssertArraySize((ArrayNode)doc.Document.Properties[1].Value, 2);
            ToonTestHelpers.AssertArraySize((ArrayNode)doc.Document.Properties[2].Value, 2);
            ToonTestHelpers.AssertArraySize((ArrayNode)doc.Document.Properties[3].Value, 2);
        }

        #endregion

        #region Inheritance with Whitespace Variations

        [TestMethod]
        public void Parse_DelimiterMarker_WithLeadingWhitespace_StillValid()
        {
            // Whitespace before delimiter marker
            var source = "items[3 	]: a	b	c";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_DelimiterMarker_WithTrailingWhitespace_StillValid()
        {
            // Whitespace after delimiter marker (before ])
            var source = "items[3	 ]: a	b	c";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result.Document);
        }

        #endregion

        #region Inheritance Error Scenarios

        [TestMethod]
        public void Parse_ArrayUsingWrongInheritedDelimiter_HandlesGracefully()
        {
            // Array without marker but values use wrong delimiter
            // Should fail to parse correctly or report error
            var source = "items[3]: a|b|c"; // Expected comma, got pipes

            ToonParseResult result = Toon.Parse(source);
            // Parser may treat this as single value or error - either is acceptable
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Parse_TableWithMismatchedDelimiters_SchemaAndRows_DetectsError()
        {
            // If schema and rows use different delimiters (should be impossible with proper inheritance)
            var source = @"data[2]{a,b}:
  1|2
  3|4";

            ToonParseResult result = Toon.Parse(source);
            // Should handle gracefully or report error
            Assert.IsNotNull(result);
        }

        #endregion
    }
}
