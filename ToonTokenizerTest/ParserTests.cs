using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Parse_EmptyString_ReturnsEmptyDocument()
        {
            var result = Toon.Parse("");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsEmpty(result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_SingleProperty_ReturnsDocumentWithOneProperty()
        {
            var source = "name: John";
            var result = Toon.Parse(source);

            Assert.HasCount(1, result.Document!.Properties);
            Assert.AreEqual("name", result.Document!.Properties[0].Key);
        }

        [TestMethod]
        public void Parse_MultipleProperties_ReturnsAllProperties()
        {
            var source = @"name: John
age: 30
active: true";
            var result = Toon.Parse(source);

            Assert.HasCount(3, result.Document!.Properties);
            Assert.AreEqual("name", result.Document!.Properties[0].Key);
            Assert.AreEqual("age", result.Document!.Properties[1].Key);
            Assert.AreEqual("active", result.Document!.Properties[2].Key);
        }

        [TestMethod]
        public void Parse_PropertyWithStringValue_ParsesCorrectly()
        {
            var source = "name: John";
            var result = Toon.Parse(source);
            Assert.HasCount(1, result.Document!.Properties);
            Assert.AreEqual("name", result.Document!.Properties[0].Key);
            Assert.AreEqual("John", ((StringValueNode)result.Document!.Properties[0].Value).Value);
        }

        [TestMethod]
        public void Parse_PropertyWithQuotedString_ParsesCorrectly()
        {
            var source = "name: \"John Doe\"";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsInstanceOfType(property.Value, typeof(StringValueNode));

            var stringValue = (StringValueNode)property.Value;
            Assert.AreEqual("John Doe", stringValue.Value);
        }

        [TestMethod]
        public void Parse_PropertyWithNumber_ParsesCorrectly()
        {
            var source = "age: 30";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsInstanceOfType(property.Value, typeof(NumberValueNode));

            var numberValue = (NumberValueNode)property.Value;
            Assert.AreEqual(30.0, numberValue.Value);
            Assert.IsTrue(numberValue.IsInteger);
        }

        [TestMethod]
        public void Parse_PropertyWithFloat_ParsesCorrectly()
        {
            var source = "temperature: 98.6";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsInstanceOfType(property.Value, typeof(NumberValueNode));

            var numberValue = (NumberValueNode)property.Value;
            Assert.AreEqual(98.6, numberValue.Value, 0.001);
            Assert.IsFalse(numberValue.IsInteger);
        }

        [TestMethod]
        public void Parse_PropertyWithTrue_ParsesCorrectly()
        {
            var source = "active: true";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsInstanceOfType(property.Value, typeof(BooleanValueNode));

            var boolValue = (BooleanValueNode)property.Value;
            Assert.IsTrue(boolValue.Value);
        }

        [TestMethod]
        public void Parse_PropertyWithFalse_ParsesCorrectly()
        {
            var source = "active: false";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsInstanceOfType(property.Value, typeof(BooleanValueNode));

            var boolValue = (BooleanValueNode)property.Value;
            Assert.IsFalse(boolValue.Value);
        }

        [TestMethod]
        public void Parse_PropertyWithNull_ParsesCorrectly()
        {
            var source = "value: null";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsInstanceOfType(property.Value, typeof(NullValueNode));
        }

        [TestMethod]
        public void Parse_NestedObject_ParsesCorrectly()
        {
            var source = @"user:
  name: John
  age: 30";
            var result = Toon.Parse(source);

            Assert.HasCount(1, result.Document!.Properties);
            var userProp = result.Document!.Properties[0];
            Assert.AreEqual("user", userProp.Key);
            Assert.IsInstanceOfType(userProp.Value, typeof(ObjectNode));

            var userObj = (ObjectNode)userProp.Value;
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
            var result = Toon.Parse(source);

            var root = result.Document!.Properties[0];
            Assert.IsInstanceOfType(root.Value, typeof(ObjectNode));

            var level1 = ((ObjectNode)root.Value).Properties[0];
            Assert.AreEqual("level1", level1.Key);
            Assert.IsInstanceOfType(level1.Value, typeof(ObjectNode));

            var level2 = ((ObjectNode)level1.Value).Properties[0];
            Assert.AreEqual("level2", level2.Key);
            Assert.IsInstanceOfType(level2.Value, typeof(ObjectNode));

            var value = ((ObjectNode)level2.Value).Properties[0];
            Assert.AreEqual("value", value.Key);
        }

        [TestMethod]
        public void Parse_InlineArray_ParsesCorrectly()
        {
            var source = "colors[3]: red,green,blue";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.AreEqual("colors", property.Key);
            Assert.IsInstanceOfType(property.Value, typeof(ArrayNode));

            var array = (ArrayNode)property.Value;
            Assert.AreEqual(3, array.DeclaredSize);
            Assert.HasCount(3, array.Elements);

            var firstElement = (StringValueNode)array.Elements[0];
            Assert.AreEqual("red", firstElement.Value);
        }

        [TestMethod]
        public void Parse_TableArray_ParsesCorrectly()
        {
            var source = @"users[2]{id,name}:
  1,Alice
  2,Bob";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.AreEqual("users", property.Key);
            Assert.IsInstanceOfType(property.Value, typeof(TableArrayNode));

            var table = (TableArrayNode)property.Value;
            Assert.AreEqual(2, table.DeclaredSize);
            Assert.HasCount(2, table.Schema);
            Assert.AreEqual("id", table.Schema[0]);
            Assert.AreEqual("name", table.Schema[1]);
            Assert.HasCount(2, table.Rows);
        }

        [TestMethod]
        public void Parse_TableArrayWithMultipleFields_ParsesAllFields()
        {
            var source = @"data[1]{a,b,c,d}:
  1,2,3,4";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            var table = (TableArrayNode)property.Value;

            Assert.HasCount(4, table.Schema);
            Assert.HasCount(1, table.Rows);
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
            var result = Toon.Parse(source);

            Assert.HasCount(4, result.Document!.Properties);
            Assert.IsInstanceOfType(result.Document!.Properties[0].Value, typeof(StringValueNode));
            Assert.IsInstanceOfType(result.Document!.Properties[1].Value, typeof(NumberValueNode));
            Assert.IsInstanceOfType(result.Document!.Properties[2].Value, typeof(ArrayNode));
            Assert.IsInstanceOfType(result.Document!.Properties[3].Value, typeof(ObjectNode));
        }

        [TestMethod]
        public void Parse_PropertyWithComment_IgnoresComment()
        {
            var source = @"name: John # This is a comment
age: 30";
            var result = Toon.Parse(source);

            Assert.HasCount(2, result.Document!.Properties);
        }

        [TestMethod, Ignore]
        public void Parse_ComplexExample_ParsesCorrectly()
        {
            var source = @"context:
  task: Our favorite hikes together
  location: Boulder

friends[3]: ana,luis,sam

hikes[2]{id,name,distance}:
  1,Blue Lake Trail,7.5
  2,Ridge Overlook,9.2";

            var result = Toon.Parse(source);

            Assert.HasCount(3, result.Document!.Properties);

            // Check context object
            var context = result.Document!.Properties[0];
            Assert.AreEqual("context", context.Key);
            Assert.IsInstanceOfType(context.Value, typeof(ObjectNode));
            Assert.HasCount(2, ((ObjectNode)context.Value).Properties);

            // Check friends array
            var friends = result.Document!.Properties[1];
            Assert.AreEqual("friends", friends.Key);
            Assert.IsInstanceOfType(friends.Value, typeof(ArrayNode));
            Assert.HasCount(3, ((ArrayNode)friends.Value).Elements);

            // Check hikes table
            var hikes = result.Document!.Properties[2];
            Assert.AreEqual("hikes", hikes.Key);
            Assert.IsInstanceOfType(hikes.Value, typeof(TableArrayNode));
            var hikesTable = (TableArrayNode)hikes.Value;
            Assert.HasCount(3, hikesTable.Schema);
            Assert.HasCount(2, hikesTable.Rows);
        }

        [TestMethod]
        public void TryParse_ValidInput_ReturnsTrue()
        {
            var source = "name: John";
            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success);
            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void TryParse_InvalidInput_ReturnsTrue()
        {
            var source = "invalid syntax without colon";
            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success, "TryParse should return true for completed parse even with errors");
            Assert.IsNotEmpty(result.Errors);
        }

        [TestMethod]
        public void Parse_AstNodePositions_AreTracked()
        {
            var source = "name: John";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsGreaterThan(0, property.StartLine);
            Assert.IsGreaterThan(0, property.StartColumn);
            Assert.IsGreaterThanOrEqualTo(0, property.StartPosition);
        }
    }
}
