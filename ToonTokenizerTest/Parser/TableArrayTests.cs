using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    [TestClass]
    public class TableArrayTests
    {
        [TestMethod]
        public void Parse_SimpleTableArray_ParsesCorrectly()
        {
            var source = @"users[2]{id,name}:
  1,Alice
  2,Bob";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name");
        }

        [TestMethod]
        public void Parse_TableArray_SchemaFieldsCorrect()
        {
            var source = @"data[1]{id,name,age}:
  1,John,30";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 1, "id", "name", "age");
        }

        [TestMethod]
        public void Parse_TableArray_RowDataCorrect()
        {
            var source = @"users[1]{id,name}:
  1,Alice";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            var id = (NumberValueNode)table.Rows[0][0];
            var name = (StringValueNode)table.Rows[0][1];

            Assert.AreEqual(1.0, id.Value, "ID value should be 1.0");
            Assert.AreEqual("Alice", name.Value, "Name should be 'Alice'");
        }

        [TestMethod]
        public void Parse_TableArray_MultipleRows_ParsesCorrectly()
        {
            var source = @"items[3]{id,value}:
  1,alpha
  2,beta
  3,gamma";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 3, "id", "value");

            for (int i = 0; i < 3; i++)
            {
                Assert.HasCount(2, table.Rows[i], $"Row {i} should have 2 fields");
            }
        }

        [TestMethod]
        public void Parse_TableArray_WithQuotedStrings_ParsesCorrectly()
        {
            var source = @"people[2]{id,name}:
  1,""John Doe""
  2,""Jane Smith""";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableCellValue(table, 0, 1, "John Doe");
            ToonTestHelpers.AssertTableCellValue(table, 1, 1, "Jane Smith");
        }

        [TestMethod]
        public void Parse_TableArray_WithFloats_ParsesCorrectly()
        {
            var source = @"measurements[2]{id,value}:
  1,3.14
  2,2.718";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            var val1 = (NumberValueNode)table.Rows[0][1];
            var val2 = (NumberValueNode)table.Rows[1][1];

            Assert.AreEqual(3.14, val1.Value, 0.01, "First measurement should be 3.14");
            Assert.AreEqual(2.718, val2.Value, 0.001, "Second measurement should be 2.718");
        }

        [TestMethod]
        public void Parse_TableArray_WithBooleans_ParsesCorrectly()
        {
            var source = @"flags[2]{id,enabled}:
  1,true
  2,false";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            var flag1 = (BooleanValueNode)table.Rows[0][1];
            var flag2 = (BooleanValueNode)table.Rows[1][1];

            Assert.IsTrue(flag1.Value, "First flag should be true");
            Assert.IsFalse(flag2.Value, "Second flag should be false");
        }

        [TestMethod]
        public void Parse_TableArray_WithNullValues_ParsesCorrectly()
        {
            var source = @"data[2]{id,optional}:
  1,null
  2,value";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            Assert.IsInstanceOfType<NullValueNode>(table.Rows[0][1], "First row should have null value");
            Assert.IsInstanceOfType<StringValueNode>(table.Rows[1][1], "Second row should have string value");
        }

        [TestMethod]
        public void Parse_TableArray_WithMixedTypes_ParsesCorrectly()
        {
            var source = @"mixed[2]{id,str,num,bool,nullVal}:
  1,text,42,true,null
  2,data,3.14,false,null";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "str", "num", "bool", "nullVal");

            List<AstNode> row1 = table.Rows[0];
            Assert.IsInstanceOfType<NumberValueNode>(row1[0], "Field 0 should be number");
            Assert.IsInstanceOfType<StringValueNode>(row1[1], "Field 1 should be string");
            Assert.IsInstanceOfType<NumberValueNode>(row1[2], "Field 2 should be number");
            Assert.IsInstanceOfType<BooleanValueNode>(row1[3], "Field 3 should be boolean");
            Assert.IsInstanceOfType<NullValueNode>(row1[4], "Field 4 should be null");
        }

        [TestMethod]
        public void Parse_TableArray_SingleField_ParsesCorrectly()
        {
            var source = @"items[3]{value}:
  alpha
  beta
  gamma";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 3, "value");

            foreach (List<AstNode> row in table.Rows)
            {
                Assert.HasCount(1, row, "Each row should have exactly 1 field");
            }
        }

        [TestMethod]
        public void Parse_TableArray_ManyFields_ParsesCorrectly()
        {
            var source = @"data[1]{a,b,c,d,e,f}:
  1,2,3,4,5,6";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 1, "a", "b", "c", "d", "e", "f");
            Assert.HasCount(6, table.Rows[0], "Row should have 6 fields");
        }

        [TestMethod]
        public void Parse_TableArray_ComplexExample_ParsesCorrectly()
        {
            var source = @"hikes[3]{id,name,distanceKm,elevationGain,companion,wasSunny}:
  1,Blue Lake Trail,7.5,320,ana,true
  2,Ridge Overlook,9.2,540,luis,false
  3,Wildflower Loop,5.1,180,sam,true";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 3, "id", "name", "distanceKm", "elevationGain", "companion", "wasSunny");

            // Check first row values
            List<AstNode> row1 = table.Rows[0];
            Assert.AreEqual(1.0, ((NumberValueNode)row1[0]).Value, "Row 1 ID");
            Assert.AreEqual("Blue Lake Trail", ((StringValueNode)row1[1]).Value, "Row 1 name");
            Assert.AreEqual(7.5, ((NumberValueNode)row1[2]).Value, 0.01, "Row 1 distance");
            Assert.AreEqual(320.0, ((NumberValueNode)row1[3]).Value, "Row 1 elevation");
            Assert.AreEqual("ana", ((StringValueNode)row1[4]).Value, "Row 1 companion");
            Assert.IsTrue(((BooleanValueNode)row1[5]).Value, "Row 1 wasSunny");
        }

        [TestMethod]
        public void Parse_TableArray_WithSchemaSpaces_ParsesCorrectly()
        {
            var source = @"data[1]{field1, field2, field3}:
  1,2,3";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            Assert.HasCount(3, table.Schema, "Schema should have 3 fields (spaces should be trimmed)");
        }

        [TestMethod]
        public void Parse_MultipleTableArrays_ParsesCorrectly()
        {
            var source = @"table1[1]{a,b}:
  1,2

table2[1]{x,y}:
  3,4";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(2, result.Document.Properties, "Should have 2 table arrays");
            Assert.IsInstanceOfType<TableArrayNode>(result.Document.Properties[0].Value, "First property should be table");
            Assert.IsInstanceOfType<TableArrayNode>(result.Document.Properties[1].Value, "Second property should be table");

            var table1 = (TableArrayNode)result.Document.Properties[0].Value;
            var table2 = (TableArrayNode)result.Document.Properties[1].Value;

            ToonTestHelpers.AssertTableStructure(table1, 1, "a", "b");
            ToonTestHelpers.AssertTableStructure(table2, 1, "x", "y");
        }

        [TestMethod]
        public void Parse_TableArray_ZeroRows_ParsesCorrectly()
        {
            var source = "empty[0]{id,name}:\n";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 0, "id", "name");
        }

        [TestMethod]
        public void Parse_TableArray_WithNegativeNumbers_ParsesCorrectly()
        {
            var source = @"temps[2]{day,celsius}:
  1,-5
  2,-10";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            var temp1 = (NumberValueNode)table.Rows[0][1];
            var temp2 = (NumberValueNode)table.Rows[1][1];

            Assert.AreEqual(-5.0, temp1.Value, "First temperature should be -5");
            Assert.AreEqual(-10.0, temp2.Value, "Second temperature should be -10");
        }

        [TestMethod]
        public void Parse_TableArray_WithScientificNotation_ParsesCorrectly()
        {
            var source = @"data[1]{id,value}:
  1,1.5e-10";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            var value = (NumberValueNode)table.Rows[0][1];

            Assert.AreEqual(1.5e-10, value.Value, 1e-15, "Scientific notation should parse correctly");
        }

        [TestMethod]
        public void Parse_TableArray_LongRowData_ParsesCorrectly()
        {
            var source = @"data[1]{a,b,c,d,e,f,g,h,i,j}:
  1,2,3,4,5,6,7,8,9,10";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 1, "a", "b", "c", "d", "e", "f", "g", "h", "i", "j");

            for (int i = 0; i < 10; i++)
            {
                var val = (NumberValueNode)table.Rows[0][i];
                Assert.AreEqual(i + 1.0, val.Value, $"Field {i} should be {i + 1}");
            }
        }

        [TestMethod]
        public void Parse_NestedObjectContainingTableArray_ParsesCorrectly()
        {
            var source = @"root:
  data[2]{id,value}:
    1,alpha
    2,beta";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var root = (ObjectNode)result.Document.Properties[0].Value;
            var table = (TableArrayNode)root.Properties[0].Value;

            ToonTestHelpers.AssertTableStructure(table, 2, "id", "value");
        }
    }
}
