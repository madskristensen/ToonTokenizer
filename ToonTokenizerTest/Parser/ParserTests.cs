using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Parse_SingleProperty_ReturnsDocumentWithOneProperty()
        {
            var source = "name: John";
            ToonParseResult doc = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(1, doc.Document.Properties);
            Assert.AreEqual("name", doc.Document.Properties[0].Key);
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
    }
}
