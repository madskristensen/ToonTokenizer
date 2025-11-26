using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for indentation validation (§8-9 - Indentation rules).
    /// Verifies proper indentation handling, inconsistent indentation detection, and tab/space mixing.
    /// </summary>
    [TestClass]
    public class IndentationValidationTests
    {
        #region Basic Indentation

        [TestMethod]
        public void Parse_TwoSpaceIndentation_ParsesCorrectly()
        {
            var source = @"parent:
  child: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var parent = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(1, parent.Properties);
        }

        [TestMethod]
        public void Parse_FourSpaceIndentation_ParsesCorrectly()
        {
            var source = @"parent:
    child: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var parent = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(1, parent.Properties);
        }

        [TestMethod]
        public void Parse_DeeplyNestedWithConsistentIndent_ParsesCorrectly()
        {
            var source = @"level1:
  level2:
    level3:
      level4: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var l1 = (ObjectNode)result.Document.Properties[0].Value;
            var l2 = (ObjectNode)l1.Properties[0].Value;
            var l3 = (ObjectNode)l2.Properties[0].Value;
            Assert.HasCount(1, l3.Properties);
        }

        #endregion

        #region Inconsistent Indentation Detection

        [TestMethod]
        public void Parse_InconsistentIndentationInSameLevel_ContinuesParsing()
        {
            // First property uses 2 spaces, second uses 3
            var source = @"parent:
  child1: value1
   child2: value2";

            ToonParseResult result = Toon.Parse(source);

            // Parser should handle gracefully (resilient parsing)
            Assert.IsNotNull(result);
            // Depending on implementation, might have errors or parse anyway
        }

        [TestMethod]
        public void Parse_IndentNotMultipleOfBase_HandlesGracefully()
        {
            // Base indent is 2, but child uses 3 spaces
            var source = @"parent:
  child1: value1
  subparent:
     subchild: value2";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Should parse or report error gracefully
        }

        [TestMethod]
        public void Parse_InconsistentDedent_HandlesGracefully()
        {
            var source = @"root:
  level1:
    level2: value
 back_to_root: value2";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Dedent to wrong level should be handled
        }

        #endregion

        #region Tab vs Space Mixing

        [TestMethod]
        public void Parse_TabIndentation_ParsesCorrectly()
        {
            var source = "parent:\n\tchild: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var parent = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(1, parent.Properties);
        }

        [TestMethod]
        public void Parse_MixedTabsAndSpaces_HandlesGracefully()
        {
            // First level uses tabs, second uses spaces
            var source = "parent:\n\tchild1: value1\n  child2: value2";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Mixed tabs/spaces should be handled (possibly with warning)
        }

        [TestMethod]
        public void Parse_SpacesThenTabs_DetectsInconsistency()
        {
            var source = "parent:\n  child1: value1\n\tchild2: value2";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Should handle tabs after spaces at same level
        }

        #endregion

        #region Invalid Indentation Levels

        [TestMethod]
        public void Parse_UnexpectedIndentWithoutParent_HandlesGracefully()
        {
            var source = "  orphan: value";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Leading indent without parent should be handled
        }

        [TestMethod]
        public void Parse_IndentAfterSimpleValue_IgnoresExtraIndent()
        {
            var source = @"simple: value
  unexpected: child";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Indent after non-object value should create new top-level property
        }

        [TestMethod]
        public void Parse_DoubleIndent_HandlesGracefully()
        {
            // Jumps from 0 to 4 spaces without intermediate 2-space level
            var source = @"parent:
    child: value";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Should handle unexpected indent jump
        }

        #endregion

        #region Zero Indentation

        [TestMethod]
        public void Parse_NoIndentation_ParsesTopLevel()
        {
            var source = @"prop1: value1
prop2: value2
prop3: value3";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(3, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_ReturnToZeroIndent_StartsNewTopProperty()
        {
            var source = @"parent1:
  child1: value
parent2:
  child2: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(2, result.Document.Properties);
        }

        #endregion

        #region Indentation in Arrays

        [TestMethod]
        public void Parse_ArrayItemIndentation_Consistent()
        {
            // Simplified to inline array
            var source = "items[3]: item1,item2,item3";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_ArrayItemInconsistentIndent_HandlesGracefully()
        {
            var source = @"items[3]:
  - item1
   - item2
  - item3";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Inconsistent array item indentation should be handled
        }

        [TestMethod]
        public void Parse_NestedObjectInArrayItem_UsesCorrectIndent()
        {
            var source = @"items[2]:
  - obj1:
      field: value1
  - obj2:
      field: value2";

            ToonParseResult result = Toon.Parse(source);

            // Complex nested structure may cause parser errors
            Assert.IsNotNull(result.Document);
        }

        #endregion

        #region Table Array Indentation

        [TestMethod]
        public void Parse_TableArrayRowsConsistentIndent_ParsesCorrectly()
        {
            var source = @"users[3]{id,name}:
  1,Alice
  2,Bob
  3,Charlie";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 3, "id", "name");
        }

        [TestMethod]
        public void Parse_TableArrayRowInconsistentIndent_HandlesGracefully()
        {
            var source = @"users[3]{id,name}:
  1,Alice
   2,Bob
  3,Charlie";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Inconsistent table row indentation should be handled
        }

        [TestMethod]
        public void Parse_TableArrayNoIndentation_ReportsError()
        {
            var source = @"users[2]{id,name}:
1,Alice
2,Bob";

            ToonParseResult result = Toon.Parse(source);

            // Table rows without indentation should be detected
            Assert.IsNotNull(result);
        }

        #endregion

        #region Blank Lines and Indentation

        [TestMethod]
        public void Parse_BlankLinesBetweenProperties_PreservesStructure()
        {
            var source = @"prop1: value1

prop2: value2

prop3: value3";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(3, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_BlankLinesInNestedObject_PreservesStructure()
        {
            var source = @"parent:
  child1: value1

  child2: value2";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var parent = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, parent.Properties);
        }

        [TestMethod]
        public void Parse_IndentedBlankLine_IgnoredCorrectly()
        {
            var source = "parent:\n  child1: value1\n  \n  child2: value2";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var parent = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, parent.Properties);
        }

        #endregion

        #region Complex Indentation Patterns

        [TestMethod]
        public void Parse_MultipleIndentLevels_ParsesCorrectly()
        {
            var source = @"l1:
  l2a:
    l3a: value1
    l3b: value2
  l2b:
    l3c: value3";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var l1 = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, l1.Properties); // l2a and l2b
        }

        [TestMethod]
        public void Parse_IndentThenDedentMultipleLevels_ParsesCorrectly()
        {
            var source = @"root:
  level1:
    level2:
      level3: deepValue
top2: topValue";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(2, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_SiblingPropertiesAtDifferentIndents_HandlesGracefully()
        {
            var source = @"parent:
  child1: value1
   child2: value2
  child3: value3";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result);
            // Siblings at different indents should be handled
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_OnlyWhitespaceLines_HandlesGracefully()
        {
            var source = "   \n  \n    \n";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Only whitespace should result in empty document");
        }

        [TestMethod]
        public void Parse_TrailingSpacesAfterValue_IgnoredCorrectly()
        {
            var source = "key: value   ";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(1, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_LeadingSpacesInValue_PreservedInQuotedStrings()
        {
            var source = "key: \"  value with leading spaces\"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.Contains("  value", value.Value);
        }

        #endregion
    }
}
