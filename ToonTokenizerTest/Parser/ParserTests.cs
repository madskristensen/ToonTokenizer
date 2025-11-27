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
