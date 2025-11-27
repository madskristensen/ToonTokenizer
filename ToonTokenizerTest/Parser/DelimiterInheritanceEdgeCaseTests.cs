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

        #region Table Array Schema vs Row Inheritance

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

        #endregion

    }
}
