using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Parse_EmptyString_ReturnsEmptyDocument()
        {
            ToonParseResult result = Toon.Parse("");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsEmpty(result.Document.Properties);
        }

        [TestMethod]
        public void Parse_SingleProperty_ReturnsDocumentWithOneProperty()
        {
            var source = "name: John";
            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(1, doc.Document.Properties);
            Assert.AreEqual("name", doc.Document.Properties[0].Key);
        }

        [TestMethod]
        public void Parse_MultipleProperties_ReturnsAllProperties()
        {
            var source = @"name: John
age: 30
active: true";
            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(3, doc.Document.Properties);
            Assert.AreEqual("name", doc.Document.Properties[0].Key);
            Assert.AreEqual("age", doc.Document.Properties[1].Key);
            Assert.AreEqual("active", doc.Document.Properties[2].Key);
        }

        [TestMethod]
        public void Parse_PropertyWithStringValue_ParsesCorrectly()
        {
            var source = "name: John";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("John", value.Value);
        }

        [TestMethod]
        public void Parse_PropertyWithQuotedString_ParsesCorrectly()
        {
            var source = "name: \"John Doe\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("John Doe", value.Value);
        }

        [TestMethod]
        public void Parse_PropertyWithNumber_ParsesCorrectly()
        {
            var source = "age: 30";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(30.0, value.Value);
            Assert.IsTrue(value.IsInteger);
        }

        [TestMethod]
        public void Parse_PropertyWithFloat_ParsesCorrectly()
        {
            var source = "temperature: 98.6";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(98.6, value.Value, 0.001);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_PropertyWithTrue_ParsesCorrectly()
        {
            var source = "active: true";
            BooleanValueNode value = ToonTestHelpers.ParseAndGetValue<BooleanValueNode>(source);

            Assert.IsTrue(value.Value);
        }

        [TestMethod]
        public void Parse_PropertyWithFalse_ParsesCorrectly()
        {
            var source = "active: false";
            BooleanValueNode value = ToonTestHelpers.ParseAndGetValue<BooleanValueNode>(source);

            Assert.IsFalse(value.Value);
        }

        [TestMethod]
        public void Parse_PropertyWithNull_ParsesCorrectly()
        {
            var source = "value: null";
            NullValueNode value = ToonTestHelpers.ParseAndGetValue<NullValueNode>(source);

            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void Parse_NestedObject_ParsesCorrectly()
        {
            var source = @"user:
  name: John
  age: 30";
            ObjectNode userObj = ToonTestHelpers.ParseAndGetValue<ObjectNode>(source);

            Assert.HasCount(2, userObj.Properties);
            Assert.AreEqual("name", userObj.Properties[0].Key);
            Assert.AreEqual("age", userObj.Properties[1].Key);
        }

        [TestMethod]
        public void Parse_DeeplyNestedObject_ParsesCorrectly()
        {
            var source = @"root:
  level1:
    level2:
      value: deep";
            ObjectNode root = ToonTestHelpers.ParseAndGetValue<ObjectNode>(source);

            var level1 = ((ObjectNode)root.Properties[0].Value);
            Assert.AreEqual("level1", root.Properties[0].Key);

            var level2 = ((ObjectNode)level1.Properties[0].Value);
            Assert.AreEqual("level2", level1.Properties[0].Key);

            PropertyNode value = level2.Properties[0];
            Assert.AreEqual("value", value.Key);
        }

        [TestMethod]
        public void Parse_InlineArray_ParsesCorrectly()
        {
            var source = "colors[3]: red,green,blue";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            ToonTestHelpers.AssertArraySize(array, 3);

            var firstElement = (StringValueNode)array.Elements[0];
            Assert.AreEqual("red", firstElement.Value);
        }

        [TestMethod]
        public void Parse_TableArray_ParsesCorrectly()
        {
            var source = @"users[2]{id,name}:
  1,Alice
  2,Bob";
            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name");
        }

        [TestMethod]
        public void Parse_TableArrayWithMultipleFields_ParsesAllFields()
        {
            var source = @"data[1]{a,b,c,d}:
  1,2,3,4";
            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);

            ToonTestHelpers.AssertTableStructure(table, 1, "a", "b", "c", "d");
            Assert.HasCount(4, table.Rows[0]);
        }

        [TestMethod]
        public void Parse_MixedContent_ParsesCorrectly()
        {
            var source = @"title: Example
count: 5
items[2]: a,b
details:
  info: data";
            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(4, doc.Document.Properties);
            Assert.IsInstanceOfType<StringValueNode>(doc.Document.Properties[0].Value);
            Assert.IsInstanceOfType<NumberValueNode>(doc.Document.Properties[1].Value);
            Assert.IsInstanceOfType<ArrayNode>(doc.Document.Properties[2].Value);
            Assert.IsInstanceOfType<ObjectNode>(doc.Document.Properties[3].Value);
        }

        [TestMethod]
        public void Parse_PropertyWithComment_IgnoresComment()
        {
            var source = @"name: John # This is a comment
age: 30";
            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(2, doc.Document.Properties);
        }

        [TestMethod]
        public void Parse_ComplexExample_ParsesCorrectly()
        {
            var source = @"context:
  task: Our favorite hikes together
  location: Boulder

friends[3]: ana,luis,sam

hikes[2]{id,name,distance}:
  1,Blue Lake Trail,7.5
  2,Ridge Overlook,9.2";

            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(3, doc.Document.Properties);

            // Check context object
            PropertyNode context = doc.Document.Properties[0];
            Assert.AreEqual("context", context.Key);
            Assert.IsInstanceOfType<ObjectNode>(context.Value);
            Assert.HasCount(2, ((ObjectNode)context.Value).Properties);

            // Check friends array
            PropertyNode friends = doc.Document.Properties[1];
            Assert.AreEqual("friends", friends.Key);
            Assert.IsInstanceOfType<ArrayNode>(friends.Value);
            Assert.HasCount(3, ((ArrayNode)friends.Value).Elements);

            // Check hikes table
            PropertyNode hikes = doc.Document.Properties[2];
            Assert.AreEqual("hikes", hikes.Key);
            Assert.IsInstanceOfType<TableArrayNode>(hikes.Value);
            var hikesTable = (TableArrayNode)hikes.Value;
            Assert.HasCount(3, hikesTable.Schema);
            Assert.HasCount(2, hikesTable.Rows);
        }

        [TestMethod]
        public void TryParse_ValidInput_ReturnsTrue()
        {
            var source = "name: John";
            bool success = Toon.TryParse(source, out ToonParseResult? result);

            Assert.IsTrue(success);
            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void TryParse_InvalidInput_ReturnsTrue()
        {
            var source = "invalid syntax without colon";
            bool success = Toon.TryParse(source, out ToonParseResult? result);

            Assert.IsTrue(success, "TryParse should return true for completed parse even with errors");
            Assert.IsNotEmpty(result.Errors);
        }

        [TestMethod]
        public void Parse_AstNodePositions_AreTracked()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            PropertyNode property = result.Document.Properties[0];
            Assert.IsGreaterThan(0, property.StartLine);
            Assert.IsGreaterThan(0, property.StartColumn);
            Assert.IsGreaterThanOrEqualTo(0, property.StartPosition);
        }
    }
}
