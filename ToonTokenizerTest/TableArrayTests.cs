using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest
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
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.AreEqual(2, table.DeclaredSize);
            Assert.HasCount(2, table.Schema);
            Assert.HasCount(2, table.Rows);
        }

        [TestMethod]
        public void Parse_TableArray_SchemaFieldsCorrect()
        {
            var source = @"data[1]{id,name,age}:
  1,John,30";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.AreEqual("id", table.Schema[0]);
            Assert.AreEqual("name", table.Schema[1]);
            Assert.AreEqual("age", table.Schema[2]);
        }

        [TestMethod]
        public void Parse_TableArray_RowDataCorrect()
        {
            var source = @"users[1]{id,name}:
  1,Alice";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            var row = table.Rows[0];

            var id = (NumberValueNode)row[0];
            var name = (StringValueNode)row[1];

            Assert.AreEqual(1.0, id.Value);
            Assert.AreEqual("Alice", name.Value);
        }

        [TestMethod]
        public void Parse_TableArray_MultipleRows_ParsesCorrectly()
        {
            var source = @"items[3]{id,value}:
  1,alpha
  2,beta
  3,gamma";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.HasCount(3, table.Rows);

            for (int i = 0; i < 3; i++)
            {
                Assert.HasCount(2, table.Rows[i]);
            }
        }

        [TestMethod]
        public void Parse_TableArray_WithQuotedStrings_ParsesCorrectly()
        {
            var source = @"people[2]{id,name}:
  1,""John Doe""
  2,""Jane Smith""";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            var name1 = (StringValueNode)table.Rows[0][1];
            var name2 = (StringValueNode)table.Rows[1][1];

            Assert.AreEqual("John Doe", name1.Value);
            Assert.AreEqual("Jane Smith", name2.Value);
        }

        [TestMethod]
        public void Parse_TableArray_WithFloats_ParsesCorrectly()
        {
            var source = @"measurements[2]{id,value}:
  1,3.14
  2,2.718";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            var val1 = (NumberValueNode)table.Rows[0][1];
            var val2 = (NumberValueNode)table.Rows[1][1];

            Assert.AreEqual(3.14, val1.Value, 0.01);
            Assert.AreEqual(2.718, val2.Value, 0.001);
        }

        [TestMethod]
        public void Parse_TableArray_WithBooleans_ParsesCorrectly()
        {
            var source = @"flags[2]{id,enabled}:
  1,true
  2,false";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            var flag1 = (BooleanValueNode)table.Rows[0][1];
            var flag2 = (BooleanValueNode)table.Rows[1][1];

            Assert.IsTrue(flag1.Value);
            Assert.IsFalse(flag2.Value);
        }

        [TestMethod]
        public void Parse_TableArray_WithNullValues_ParsesCorrectly()
        {
            var source = @"data[2]{id,optional}:
  1,null
  2,value";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.IsInstanceOfType(table.Rows[0][1], typeof(NullValueNode));
            Assert.IsInstanceOfType(table.Rows[1][1], typeof(StringValueNode));
        }

        [TestMethod]
        public void Parse_TableArray_WithMixedTypes_ParsesCorrectly()
        {
            var source = @"mixed[2]{id,str,num,bool,nullVal}:
  1,text,42,true,null
  2,data,3.14,false,null";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.HasCount(5, table.Schema);
            Assert.HasCount(2, table.Rows);

            var row1 = table.Rows[0];
            Assert.IsInstanceOfType(row1[0], typeof(NumberValueNode));
            Assert.IsInstanceOfType(row1[1], typeof(StringValueNode));
            Assert.IsInstanceOfType(row1[2], typeof(NumberValueNode));
            Assert.IsInstanceOfType(row1[3], typeof(BooleanValueNode));
            Assert.IsInstanceOfType(row1[4], typeof(NullValueNode));
        }

        [TestMethod]
        public void Parse_TableArray_SingleField_ParsesCorrectly()
        {
            // Property at column 1, all rows indented with 2 spaces
            var source = @"items[3]{value}:
  alpha
  beta
  gamma";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.HasCount(1, table.Schema);
            Assert.HasCount(3, table.Rows);

            foreach (var row in table.Rows)
            {
                Assert.HasCount(1, row);
            }
        }

        [TestMethod]
        public void Parse_TableArray_ManyFields_ParsesCorrectly()
        {
            var source = @"data[1]{a,b,c,d,e,f}:
  1,2,3,4,5,6";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.HasCount(6, table.Schema);
            Assert.HasCount(1, table.Rows);
            Assert.HasCount(6, table.Rows[0]);
        }

        [TestMethod]
        public void Parse_TableArray_ComplexExample_ParsesCorrectly()
        {
            var source = @"hikes[3]{id,name,distanceKm,elevationGain,companion,wasSunny}:
  1,Blue Lake Trail,7.5,320,ana,true
  2,Ridge Overlook,9.2,540,luis,false
  3,Wildflower Loop,5.1,180,sam,true";

            var document = Toon.Parse(source);
            var table = (TableArrayNode)document.Properties[0].Value;

            Assert.AreEqual(3, table.DeclaredSize);
            Assert.HasCount(6, table.Schema);
            Assert.HasCount(3, table.Rows);

            // Check schema
            Assert.AreEqual("id", table.Schema[0]);
            Assert.AreEqual("name", table.Schema[1]);
            Assert.AreEqual("distanceKm", table.Schema[2]);
            Assert.AreEqual("elevationGain", table.Schema[3]);
            Assert.AreEqual("companion", table.Schema[4]);
            Assert.AreEqual("wasSunny", table.Schema[5]);

            // Check first row
            var row1 = table.Rows[0];
            Assert.AreEqual(1.0, ((NumberValueNode)row1[0]).Value);
            Assert.AreEqual("Blue Lake Trail", ((StringValueNode)row1[1]).Value);
            Assert.AreEqual(7.5, ((NumberValueNode)row1[2]).Value);
            Assert.AreEqual(320.0, ((NumberValueNode)row1[3]).Value);
            Assert.AreEqual("ana", ((StringValueNode)row1[4]).Value);
            Assert.IsTrue(((BooleanValueNode)row1[5]).Value);
        }

        [TestMethod]
        public void Parse_TableArray_WithSchemaSpaces_ParsesCorrectly()
        {
            var source = @"data[1]{field1, field2, field3}:
  1,2,3";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.HasCount(3, table.Schema);
        }

        [TestMethod]
        public void Parse_MultipleTableArrays_ParsesCorrectly()
        {
            // Valid TOON: all rows have commas, blank line between arrays is allowed, and indentation is consistent
            var source = @"table1[1]{a,b}:
  1,2

table2[1]{x,y}:
  3,4";
            var document = Toon.Parse(source);

            Assert.HasCount(2, document.Properties);
            Assert.IsInstanceOfType(document.Properties[0].Value, typeof(TableArrayNode));
            Assert.IsInstanceOfType(document.Properties[1].Value, typeof(TableArrayNode));

            var table1 = (TableArrayNode)document.Properties[0].Value;
            var table2 = (TableArrayNode)document.Properties[1].Value;

            Assert.HasCount(2, table1.Schema);
            Assert.HasCount(2, table2.Schema);
        }

        [TestMethod]
        public void Parse_TableArray_ZeroRows_ParsesCorrectly()
        {
            // Add a newline after the colon to ensure the parser sees the end of the table
            var source = "empty[0]{id,name}:\n";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.AreEqual(0, table.DeclaredSize);
            Assert.HasCount(2, table.Schema);
            Assert.IsEmpty(table.Rows);
        }

        [TestMethod]
        public void Parse_TableArray_WithNegativeNumbers_ParsesCorrectly()
        {
            var source = @"temps[2]{day,celsius}:
  1,-5
  2,-10";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            var temp1 = (NumberValueNode)table.Rows[0][1];
            var temp2 = (NumberValueNode)table.Rows[1][1];

            Assert.AreEqual(-5.0, temp1.Value);
            Assert.AreEqual(-10.0, temp2.Value);
        }

        [TestMethod]
        public void Parse_TableArray_WithScientificNotation_ParsesCorrectly()
        {
            var source = @"data[1]{id,value}:
  1,1.5e-10";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            var value = (NumberValueNode)table.Rows[0][1];

            Assert.AreEqual(1.5e-10, value.Value, 1e-15);
        }

        [TestMethod]
        public void Parse_TableArray_LongRowData_ParsesCorrectly()
        {
            var source = @"data[1]{a,b,c,d,e,f,g,h,i,j}:
  1,2,3,4,5,6,7,8,9,10";
            var document = Toon.Parse(source);

            var table = (TableArrayNode)document.Properties[0].Value;
            Assert.HasCount(10, table.Schema);
            Assert.HasCount(10, table.Rows[0]);

            for (int i = 0; i < 10; i++)
            {
                var val = (NumberValueNode)table.Rows[0][i];
                Assert.AreEqual(i + 1.0, val.Value);
            }
        }

        [TestMethod]
        public void Parse_NestedObjectContainingTableArray_ParsesCorrectly()
        {
            // Valid TOON: nested object containing a table array
            var source = @"root:
  data[2]{id,value}:
    1,alpha
    2,beta";
            var document = Toon.Parse(source);

            var root = (ObjectNode)document.Properties[0].Value;
            var table = (TableArrayNode)root.Properties[0].Value;

            Assert.HasCount(2, table.Rows);
        }
    }
}
