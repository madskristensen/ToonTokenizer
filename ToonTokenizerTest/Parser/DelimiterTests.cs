using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    [TestClass]
    public class DelimiterTests
    {
        [TestMethod]
        public void Parse_CommaDelimiter_Default_ParsesCorrectly()
        {
            var source = "tags[3]: reading,gaming,coding";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 3);
            ToonTestHelpers.AssertArrayElements(array, "reading", "gaming", "coding");
        }

        [TestMethod]
        public void Parse_TabDelimiter_InlineArray_ParsesCorrectly()
        {
            var source = "tags[3\t]: reading\tgaming\tcoding";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 3);
            ToonTestHelpers.AssertArrayElements(array, "reading", "gaming", "coding");
        }

        [TestMethod]
        public void Parse_PipeDelimiter_InlineArray_ParsesCorrectly()
        {
            var source = "tags[3|]: reading|gaming|coding";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 3);
            ToonTestHelpers.AssertArrayElements(array, "reading", "gaming", "coding");
        }

        [TestMethod]
        public void Parse_TabDelimiter_TableArray_ParsesCorrectly()
        {
            var source = "items[2\t]{sku\tname\tqty\tprice}:\n  A1\tWidget\t2\t9.99\n  B2\tGadget\t1\t14.5";
            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            ToonTestHelpers.AssertTableStructure(table, 2, "sku", "name", "qty", "price");

            // Verify first row
            ToonTestHelpers.AssertTableCellValue(table, 0, 0, "A1");
            ToonTestHelpers.AssertTableCellValue(table, 0, 1, "Widget");
            Assert.AreEqual(2.0, ((NumberValueNode)table.Rows[0][2]).Value);
            Assert.AreEqual(9.99, ((NumberValueNode)table.Rows[0][3]).Value);

            // Verify second row
            ToonTestHelpers.AssertTableCellValue(table, 1, 0, "B2");
            ToonTestHelpers.AssertTableCellValue(table, 1, 1, "Gadget");
            Assert.AreEqual(1.0, ((NumberValueNode)table.Rows[1][2]).Value);
            Assert.AreEqual(14.5, ((NumberValueNode)table.Rows[1][3]).Value);
        }

        [TestMethod]
        public void Parse_PipeDelimiter_TableArray_ParsesCorrectly()
        {
            var source = "tags[3|]: reading|gaming|coding";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArrayElements(array, "reading", "gaming", "coding");
        }

        [TestMethod]
        public void Parse_DelimiterScoping_ParentAndChildDifferent_ParsesCorrectly()
        {
            // Outer array uses pipe, inner arrays use comma (default)
            var source = "data[2|]: a|b";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArrayElements(array, "a", "b");
        }
    }
}
