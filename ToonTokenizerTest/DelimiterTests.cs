using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest
{
    [TestClass]
    public class DelimiterTests
    {
        [TestMethod]
        public void Parse_CommaDelimiter_Default_ParsesCorrectly()
        {
            var source = "tags[3]: reading,gaming,coding";
            var document = Toon.Parse(source);

            var property = document.Properties[0];
            Assert.AreEqual("tags", property.Key);
            Assert.IsInstanceOfType(property.Value, typeof(ArrayNode));

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
            var document = Toon.Parse(source);

            var property = document.Properties[0];
            Assert.AreEqual("tags", property.Key);
            Assert.IsInstanceOfType(property.Value, typeof(ArrayNode));

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
            var document = Toon.Parse(source);

            var property = document.Properties[0];
            Assert.AreEqual("tags", property.Key);
            Assert.IsInstanceOfType(property.Value, typeof(ArrayNode));

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
            var document = Toon.Parse(source);

            var property = document.Properties[0];
            Assert.AreEqual("items", property.Key);
            Assert.IsInstanceOfType(property.Value, typeof(TableArrayNode));

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
            var document = Toon.Parse(source);

            var property = document.Properties[0];
            Assert.IsInstanceOfType(property.Value, typeof(ArrayNode));

            var array = (ArrayNode)property.Value;
            Assert.HasCount(3, array.Elements);
            Assert.AreEqual("reading", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("gaming", ((StringValueNode)array.Elements[1]).Value);
            Assert.AreEqual("coding", ((StringValueNode)array.Elements[2]).Value);
        }

        [TestMethod, Ignore]
        public void Parse_NestedArrays_WithDifferentDelimiters_ParsesCorrectly()
        {
            var source = "pairs[2]:\n  - [2]: 1,2\n  - [2]: 3,4";
            var document = Toon.Parse(source);

            var property = document.Properties[0];
            Assert.AreEqual("pairs", property.Key);
            Assert.IsInstanceOfType(property.Value, typeof(ArrayNode));

            var outerArray = (ArrayNode)property.Value;
            Assert.AreEqual(2, outerArray.DeclaredSize);
            Assert.HasCount(2, outerArray.Elements);

            // Verify first inner array
            var innerArray1 = (ArrayNode)outerArray.Elements[0];
            Assert.AreEqual(2, innerArray1.DeclaredSize);
            Assert.HasCount(2, innerArray1.Elements);
            Assert.AreEqual("1", ((StringValueNode)innerArray1.Elements[0]).Value);
            Assert.AreEqual("2", ((StringValueNode)innerArray1.Elements[1]).Value);

            // Verify second inner array
            var innerArray2 = (ArrayNode)outerArray.Elements[1];
            Assert.AreEqual(2, innerArray2.DeclaredSize);
            Assert.HasCount(2, innerArray2.Elements);
            Assert.AreEqual("3", ((StringValueNode)innerArray2.Elements[0]).Value);
            Assert.AreEqual("4", ((StringValueNode)innerArray2.Elements[1]).Value);
        }

        [TestMethod, Ignore]
        public void Parse_NestedArraysWithTabDelimiter_ParsesCorrectly()
        {
            var source = "pairs[2]:\n  - [2\t]: 1\t2\n  - [2\t]: 3\t4";
            var document = Toon.Parse(source);

            var property = document.Properties[0];
            var outerArray = (ArrayNode)property.Value;
            Assert.AreEqual(2, outerArray.DeclaredSize);
            Assert.HasCount(2, outerArray.Elements);

            // Verify first inner array uses tab delimiter
            var innerArray1 = (ArrayNode)outerArray.Elements[0];
            Assert.HasCount(2, innerArray1.Elements);
            Assert.AreEqual("1", ((StringValueNode)innerArray1.Elements[0]).Value);
            Assert.AreEqual("2", ((StringValueNode)innerArray1.Elements[1]).Value);
        }

        [TestMethod]
        public void Parse_DelimiterScoping_ParentAndChildDifferent_ParsesCorrectly()
        {
            // Outer array uses pipe, inner arrays use comma (default)
            var source = "data[2|]: a|b";
            var document = Toon.Parse(source);

            var property = document.Properties[0];
            var array = (ArrayNode)property.Value;
            Assert.HasCount(2, array.Elements);
            Assert.AreEqual("a", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("b", ((StringValueNode)array.Elements[1]).Value);
        }
    }
}
