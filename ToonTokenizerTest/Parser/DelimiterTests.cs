using ToonTokenizer;
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
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.AreEqual("tags", property.Key);
            Assert.IsInstanceOfType<ArrayNode>(property.Value);

            var array = (ArrayNode)property.Value;
            Assert.AreEqual(3, array.DeclaredSize);
            Assert.HasCount(3, array.Elements);

            Assert.AreEqual("reading", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("gaming", ((StringValueNode)array.Elements[1]).Value);
            Assert.AreEqual("coding", ((StringValueNode)array.Elements[2]).Value);
        }

        [TestMethod]
        public void Parse_TabDelimiter_InlineArray_ParsesCorrectly()
        {
            var source = "tags[3\t]: reading\tgaming\tcoding";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.AreEqual("tags", property.Key);
            Assert.IsInstanceOfType<ArrayNode>(property.Value);

            var array = (ArrayNode)property.Value;
            Assert.AreEqual(3, array.DeclaredSize);
            Assert.HasCount(3, array.Elements);

            Assert.AreEqual("reading", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("gaming", ((StringValueNode)array.Elements[1]).Value);
            Assert.AreEqual("coding", ((StringValueNode)array.Elements[2]).Value);
        }

        [TestMethod]
        public void Parse_PipeDelimiter_InlineArray_ParsesCorrectly()
        {
            var source = "tags[3|]: reading|gaming|coding";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.AreEqual("tags", property.Key);
            Assert.IsInstanceOfType<ArrayNode>(property.Value);

            var array = (ArrayNode)property.Value;
            Assert.AreEqual(3, array.DeclaredSize);
            Assert.HasCount(3, array.Elements);

            Assert.AreEqual("reading", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("gaming", ((StringValueNode)array.Elements[1]).Value);
            Assert.AreEqual("coding", ((StringValueNode)array.Elements[2]).Value);
        }

        [TestMethod]
        public void Parse_TabDelimiter_TableArray_ParsesCorrectly()
        {
            var source = "items[2\t]{sku\tname\tqty\tprice}:\n  A1\tWidget\t2\t9.99\n  B2\tGadget\t1\t14.5";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.AreEqual("items", property.Key);
            Assert.IsInstanceOfType<TableArrayNode>(property.Value);

            var table = (TableArrayNode)property.Value;
            Assert.AreEqual(2, table.DeclaredSize);
            Assert.HasCount(4, table.Schema);
            Assert.AreEqual("sku", table.Schema[0]);
            Assert.AreEqual("name", table.Schema[1]);
            Assert.AreEqual("qty", table.Schema[2]);
            Assert.AreEqual("price", table.Schema[3]);
            Assert.HasCount(2, table.Rows);

            // Verify first row
            Assert.AreEqual("A1", ((StringValueNode)table.Rows[0][0]).Value);
            Assert.AreEqual("Widget", ((StringValueNode)table.Rows[0][1]).Value);
            Assert.AreEqual(2.0, ((NumberValueNode)table.Rows[0][2]).Value);
            Assert.AreEqual(9.99, ((NumberValueNode)table.Rows[0][3]).Value);

            // Verify second row
            Assert.AreEqual("B2", ((StringValueNode)table.Rows[1][0]).Value);
            Assert.AreEqual("Gadget", ((StringValueNode)table.Rows[1][1]).Value);
            Assert.AreEqual(1.0, ((NumberValueNode)table.Rows[1][2]).Value);
            Assert.AreEqual(14.5, ((NumberValueNode)table.Rows[1][3]).Value);
        }

        [TestMethod]
        public void Parse_PipeDelimiter_TableArray_ParsesCorrectly()
        {
            var source = "tags[3|]: reading|gaming|coding";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsInstanceOfType<ArrayNode>(property.Value);

            var array = (ArrayNode)property.Value;
            Assert.HasCount(3, array.Elements);
            Assert.AreEqual("reading", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("gaming", ((StringValueNode)array.Elements[1]).Value);
            Assert.AreEqual("coding", ((StringValueNode)array.Elements[2]).Value);
        }

        [TestMethod]
        public void Parse_DelimiterScoping_ParentAndChildDifferent_ParsesCorrectly()
        {
            // Outer array uses pipe, inner arrays use comma (default)
            var source = "data[2|]: a|b";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            var array = (ArrayNode)property.Value;
            Assert.HasCount(2, array.Elements);
            Assert.AreEqual("a", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("b", ((StringValueNode)array.Elements[1]).Value);
        }
    }
}
