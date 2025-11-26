using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Integration
{
    [TestClass]
    public class TokenAstNavigationTests
    {
        [TestMethod]
        public void GetAstNode_SimpleProperty_ReturnsPropertyNode()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            Token? token = result.Tokens.Find(t => t.Value == "John");
            Assert.IsNotNull(token);

            AstNode? node = token.GetAstNode(result.Document);
            Assert.IsNotNull(node);
            Assert.IsInstanceOfType<StringValueNode>(node);
        }

        [TestMethod]
        public void GetPropertyNode_FromValueToken_ReturnsContainingProperty()
        {
            var source = "age: 30";
            ToonParseResult result = Toon.Parse(source);

            Token? token = result.Tokens.Find(t => t.Value == "30");
            Assert.IsNotNull(token);

            PropertyNode? property = token.GetPropertyNode(result.Document);
            Assert.IsNotNull(property);
            Assert.AreEqual("age", property.Key);
        }

        [TestMethod]
        public void GetPropertyNode_FromKeyToken_ReturnsProperty()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            Token? token = result.Tokens.Find(t => t.Value == "name" && t.Type == TokenType.Identifier);
            Assert.IsNotNull(token);

            PropertyNode? property = token.GetPropertyNode(result.Document);
            Assert.IsNotNull(property);
            Assert.AreEqual("name", property.Key);
        }

        [TestMethod]
        public void GetNodeAtPosition_ValidPosition_ReturnsNode()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            // Position of "John" token
            Token? token = result.Tokens.Find(t => t.Value == "John");
            Assert.IsNotNull(token);

            AstNode? node = result.GetNodeAtPosition(token.Position);
            Assert.IsNotNull(node);
            Assert.IsInstanceOfType<StringValueNode>(node);
        }

        [TestMethod]
        public void GetPropertyAt_ValidLineColumn_ReturnsProperty()
        {
            var source = "name: John\nage: 30";
            ToonParseResult result = Toon.Parse(source);

            PropertyNode? property = result.GetPropertyAt(line: 2, column: 1);
            Assert.IsNotNull(property);
            Assert.AreEqual("age", property.Key);
        }

        [TestMethod]
        public void GetPropertyAt_InvalidPosition_ReturnsNull()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            PropertyNode? property = result.GetPropertyAt(line: 99, column: 99);
            Assert.IsNull(property);
        }

        [TestMethod]
        public void GetAllProperties_FlatDocument_ReturnsAllProperties()
        {
            var source = "name: John\nage: 30\ncity: Boulder";
            ToonParseResult result = Toon.Parse(source);

            List<PropertyNode> allProps = result.GetAllProperties();
            Assert.HasCount(3, allProps);
            Assert.AreEqual("name", allProps[0].Key);
            Assert.AreEqual("age", allProps[1].Key);
            Assert.AreEqual("city", allProps[2].Key);
        }

        [TestMethod]
        public void GetAllProperties_NestedDocument_ReturnsAllPropertiesIncludingNested()
        {
            var source = @"
user:
  name: John
  settings:
    theme: dark
city: Boulder
";
            ToonParseResult result = Toon.Parse(source);

            List<PropertyNode> allProps = result.GetAllProperties();
            // user (top), name (nested in user), settings (nested in user), theme (nested in settings), city (top) = 5 total
            Assert.HasCount(5, allProps);

            // Check we have nested properties
            PropertyNode? themeProperty = allProps.Find(p => p.Key == "theme");
            Assert.IsNotNull(themeProperty);

            PropertyNode? cityProperty = allProps.Find(p => p.Key == "city");
            Assert.IsNotNull(cityProperty);
        }

        [TestMethod]
        public void FindPropertyByPath_SimplePath_ReturnsProperty()
        {
            var source = "name: John\nage: 30";
            ToonParseResult result = Toon.Parse(source);

            PropertyNode? property = result.FindPropertyByPath("name");
            Assert.IsNotNull(property);
            Assert.AreEqual("name", property.Key);

            var value = property.Value as StringValueNode;
            Assert.IsNotNull(value);
            Assert.AreEqual("John", value.Value);
        }

        [TestMethod]
        public void FindPropertyByPath_NestedPath_ReturnsNestedProperty()
        {
            var source = @"
user:
  name: John
  settings:
    theme: dark
";
            ToonParseResult result = Toon.Parse(source);

            PropertyNode? property = result.FindPropertyByPath("user.settings.theme");
            Assert.IsNotNull(property);
            Assert.AreEqual("theme", property.Key);

            var value = property.Value as StringValueNode;
            Assert.IsNotNull(value);
            Assert.AreEqual("dark", value.Value);
        }

        [TestMethod]
        public void FindPropertyByPath_InvalidPath_ReturnsNull()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            PropertyNode? property = result.FindPropertyByPath("nonexistent.path");
            Assert.IsNull(property);
        }

        [TestMethod]
        public void FindPropertyByPath_PartialPath_ReturnsNull()
        {
            var source = @"
user:
  name: John
";
            ToonParseResult result = Toon.Parse(source);

            // Try to go deeper than possible
            PropertyNode? property = result.FindPropertyByPath("user.name.invalid");
            Assert.IsNull(property);
        }

        [TestMethod]
        public void GetNodeForToken_WithToken_ReturnsNode()
        {
            var source = "enabled: true";
            ToonParseResult result = Toon.Parse(source);

            Token? token = result.Tokens.Find(t => t.Type == TokenType.True);
            Assert.IsNotNull(token);

            AstNode? node = result.GetNodeForToken(token);
            Assert.IsNotNull(node);
            Assert.IsInstanceOfType<BooleanValueNode>(node);
        }

        [TestMethod]
        public void GetAstNode_ArrayElement_ReturnsValueNode()
        {
            var source = "colors[3]: red,green,blue";
            ToonParseResult result = Toon.Parse(source);

            Token? token = result.Tokens.Find(t => t.Value == "green");
            Assert.IsNotNull(token);

            AstNode? node = token.GetAstNode(result.Document);
            Assert.IsNotNull(node);
            Assert.IsInstanceOfType<StringValueNode>(node);
        }

        [TestMethod]
        public void GetPropertyNode_TableArrayValue_ReturnsContainingProperty()
        {
            var source = @"
users[2]{id,name}:
  1,Alice
  2,Bob
";
            ToonParseResult result = Toon.Parse(source);

            Token? token = result.Tokens.Find(t => t.Value == "Alice");
            Assert.IsNotNull(token);

            PropertyNode? property = token.GetPropertyNode(result.Document);
            Assert.IsNotNull(property);
            Assert.AreEqual("users", property.Key);
        }

        [TestMethod]
        public void GetAllProperties_WithArrays_OnlyReturnsPropertyNodes()
        {
            var source = @"
names[2]: John,Jane
users[2]{id,name}:
  1,Alice
  2,Bob
";
            ToonParseResult result = Toon.Parse(source);

            List<PropertyNode> allProps = result.GetAllProperties();

            // Should have exactly 2 properties: names and users
            Assert.HasCount(2, allProps);
            Assert.IsTrue(allProps.All(p => p is not null));
        }

        [TestMethod]
        public void GetPropertyAt_OnNestedProperty_ReturnsCorrectProperty()
        {
            var source = @"
user:
  email: john@example.com
  settings:
    notifications: true
";
            ToonParseResult result = Toon.Parse(source);

            // Find the "notifications" property
            Token? token = result.Tokens.Find(t => t.Value == "notifications");
            Assert.IsNotNull(token);

            PropertyNode? property = result.GetPropertyAt(token.Line, token.Column);
            Assert.IsNotNull(property);
            Assert.AreEqual("notifications", property.Key);
        }

        [TestMethod]
        public void GetNodeAtPosition_DocumentRoot_ReturnsDocument()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            // Position 0 should return the document or first property
            AstNode? node = result.GetNodeAtPosition(0);
            Assert.IsNotNull(node);
            // Should be either ToonDocument or PropertyNode
            Assert.IsTrue(node is ToonDocument || node is PropertyNode);
        }

        [TestMethod]
        public void FindPropertyByPath_EmptyPath_ReturnsNull()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            PropertyNode? property = result.FindPropertyByPath("");
            Assert.IsNull(property);
        }

        [TestMethod]
        public void FindPropertyByPath_NullPath_ReturnsNull()
        {
            var source = "name: John";
            ToonParseResult result = Toon.Parse(source);

            PropertyNode? property = result.FindPropertyByPath(null!);
            Assert.IsNull(property);
        }

        [TestMethod]
        public void GetPropertyNode_NullDocument_ReturnsNull()
        {
            var token = new Token(TokenType.String, "test", 1, 1, 0, 4);
            PropertyNode? property = token.GetPropertyNode(null!);
            Assert.IsNull(property);
        }

        [TestMethod]
        public void GetNodeAtPosition_NullResult_ReturnsNull()
        {
            ToonParseResult? result = null;
            AstNode? node = result!.GetNodeAtPosition(0);
            Assert.IsNull(node);
        }
    }
}
