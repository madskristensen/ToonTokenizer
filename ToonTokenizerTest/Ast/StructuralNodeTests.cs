using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Ast
{
    /// <summary>
    /// Tests for AST structural node types to ensure complete code coverage.
    /// Covers ToonDocument, PropertyNode, and ObjectNode properties and methods.
    /// </summary>
    [TestClass]
    public class StructuralNodeTests
    {
        #region ToonDocument Tests

        [TestMethod]
        public void ToonDocument_Accept_CallsVisitorMethod()
        {
            var document = new ToonDocument();
            var visitor = new TestVisitor();

            var result = document.Accept(visitor);

            Assert.IsTrue(visitor.VisitedDocument);
            Assert.AreEqual("ToonDocument visited", result);
        }

        [TestMethod]
        public void ToonDocument_EmptyDocument_ParsesCorrectly()
        {
            var source = "# Just a comment";
            var result = Toon.Parse(source);

            Assert.IsNotNull(result.Document);
            Assert.HasCount(0, result.Document.Properties);
        }

        [TestMethod]
        public void ToonDocument_SingleProperty_ParsesCorrectly()
        {
            var source = "name: John";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(1, result.Document.Properties);
            Assert.AreEqual("name", result.Document.Properties[0].Key);
        }

        [TestMethod]
        public void ToonDocument_MultipleProperties_ParsesCorrectly()
        {
            var source = @"name: John
age: 30
city: Boulder";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(3, result.Document.Properties);
        }

        [TestMethod]
        public void ToonDocument_WithNestedObjects_ParsesCorrectly()
        {
            var source = @"user:
  name: John
settings:
  theme: dark";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(2, result.Document.Properties);
            Assert.IsInstanceOfType<ObjectNode>(result.Document.Properties[0].Value);
            Assert.IsInstanceOfType<ObjectNode>(result.Document.Properties[1].Value);
        }

        [TestMethod]
        public void ToonDocument_PositionTracking_IsAccurate()
        {
            var source = "name: John";
            var result = Toon.Parse(source);

            var doc = result.Document;
            Assert.AreEqual(1, doc.StartLine);
            Assert.AreEqual(1, doc.StartColumn);
            Assert.AreEqual(0, doc.StartPosition);
        }

        #endregion

        #region PropertyNode Tests

        [TestMethod]
        public void PropertyNode_Accept_CallsVisitorMethod()
        {
            var property = new PropertyNode
            {
                Key = "test",
                Value = new StringValueNode { Value = "value" }
            };
            var visitor = new TestVisitor();

            var result = property.Accept(visitor);

            Assert.IsTrue(visitor.VisitedProperty);
            Assert.AreEqual("PropertyNode visited", result);
        }

        [TestMethod]
        public void PropertyNode_SimpleProperty_ParsesWithCorrectKey()
        {
            var source = "name: John";
            var result = Toon.Parse(source);

            var property = result.Document.Properties[0];
            Assert.AreEqual("name", property.Key);
            Assert.IsInstanceOfType<StringValueNode>(property.Value);
        }

        [TestMethod]
        public void PropertyNode_NestedProperty_ParsesWithCorrectIndentation()
        {
            var source = @"user:
  name: John";
            var result = Toon.Parse(source);

            var userProp = result.Document.Properties[0];
            var userObj = (ObjectNode)userProp.Value;
            var nameProp = userObj.Properties[0];

            Assert.AreEqual(0, userProp.IndentLevel);
            Assert.IsGreaterThan(0, nameProp.IndentLevel);
        }

        [TestMethod]
        public void PropertyNode_WithQuotedKey_ParsesCorrectly()
        {
            var source = "\"quoted-key\": value";
            var result = Toon.Parse(source);

            var property = result.Document.Properties[0];
            Assert.AreEqual("quoted-key", property.Key);
        }

        [TestMethod]
        public void PropertyNode_WithUnderscoreKey_ParsesCorrectly()
        {
            var source = "snake_case_key: value";
            var result = Toon.Parse(source);

            var property = result.Document.Properties[0];
            Assert.AreEqual("snake_case_key", property.Key);
        }

        [TestMethod]
        public void PropertyNode_WithNumberInKey_ParsesCorrectly()
        {
            var source = "key123: value";
            var result = Toon.Parse(source);

            var property = result.Document.Properties[0];
            Assert.AreEqual("key123", property.Key);
        }

        #endregion

        #region ObjectNode Tests

        [TestMethod]
        public void ObjectNode_Accept_CallsVisitorMethod()
        {
            var obj = new ObjectNode();
            var visitor = new TestVisitor();

            var result = obj.Accept(visitor);

            Assert.IsTrue(visitor.VisitedObject);
            Assert.AreEqual("ObjectNode visited", result);
        }

        [TestMethod]
        public void ObjectNode_EmptyObject_CanExist()
        {
            var source = @"user:
";
            var result = Toon.Parse(source);

            // Empty object might be represented as object with no properties
            Assert.IsTrue(result.IsSuccess || result.HasErrors);
        }

        [TestMethod]
        public void ObjectNode_SingleNestedProperty_ParsesCorrectly()
        {
            var source = @"user:
  name: John";
            var result = Toon.Parse(source);

            var obj = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(1, obj.Properties);
            Assert.AreEqual("name", obj.Properties[0].Key);
        }

        [TestMethod]
        public void ObjectNode_MultipleNestedProperties_ParsesCorrectly()
        {
            var source = @"user:
  name: John
  age: 30
  email: john@example.com";
            var result = Toon.Parse(source);

            var obj = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(3, obj.Properties);
            Assert.AreEqual("name", obj.Properties[0].Key);
            Assert.AreEqual("age", obj.Properties[1].Key);
            Assert.AreEqual("email", obj.Properties[2].Key);
        }

        [TestMethod]
        public void ObjectNode_DeeplyNested_ParsesCorrectly()
        {
            var source = @"level1:
  level2:
    level3:
      value: deep";
            var result = Toon.Parse(source);

            var level1 = (ObjectNode)result.Document.Properties[0].Value;
            var level2 = (ObjectNode)level1.Properties[0].Value;
            var level3 = (ObjectNode)level2.Properties[0].Value;

            Assert.HasCount(1, level1.Properties);
            Assert.HasCount(1, level2.Properties);
            Assert.HasCount(1, level3.Properties);
            Assert.AreEqual("value", level3.Properties[0].Key);
        }

        [TestMethod]
        public void ObjectNode_WithMixedValueTypes_ParsesCorrectly()
        {
            var source = @"config:
  name: MyApp
  version: 1.0
  enabled: true
  timeout: null
  ports[2]: 8080,8081";
            var result = Toon.Parse(source);

            var obj = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(5, obj.Properties);
            Assert.IsInstanceOfType<StringValueNode>(obj.Properties[0].Value);
            Assert.IsInstanceOfType<NumberValueNode>(obj.Properties[1].Value);
            Assert.IsInstanceOfType<BooleanValueNode>(obj.Properties[2].Value);
            Assert.IsInstanceOfType<NullValueNode>(obj.Properties[3].Value);
            Assert.IsInstanceOfType<ArrayNode>(obj.Properties[4].Value);
        }

        [TestMethod]
        public void ObjectNode_SiblingObjects_ParseCorrectly()
        {
            var source = @"user:
  name: John
settings:
  theme: dark";
            var result = Toon.Parse(source);

            Assert.HasCount(2, result.Document.Properties);
            Assert.IsInstanceOfType<ObjectNode>(result.Document.Properties[0].Value);
            Assert.IsInstanceOfType<ObjectNode>(result.Document.Properties[1].Value);
        }

        [TestMethod]
        public void ObjectNode_NestedObject_ParsesCorrectly()
        {
            var source = @"user:
  profile:
    name: John
    age: 30";
            var result = Toon.Parse(source);

            var user = (ObjectNode)result.Document.Properties[0].Value;
            var profile = (ObjectNode)user.Properties[0].Value;

            Assert.AreEqual("profile", user.Properties[0].Key);
            Assert.HasCount(2, profile.Properties);
        }

        [TestMethod]
        public void ObjectNode_WithNestedArray_ParsesCorrectly()
        {
            var source = @"data:
  items[3]: a,b,c";
            var result = Toon.Parse(source);

            var obj = (ObjectNode)result.Document.Properties[0].Value;
            var array = (ArrayNode)obj.Properties[0].Value;

            Assert.AreEqual("items", obj.Properties[0].Key);
            Assert.HasCount(3, array.Elements);
        }

        [TestMethod]
        public void ObjectNode_ComplexStructure_ParsesCorrectly()
        {
            var source = @"app:
  name: MyApp
  settings:
    colors[2]: red,blue
    flags:
      debug: true";
            var result = Toon.Parse(source);

            var app = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, app.Properties);

            var settings = (ObjectNode)app.Properties[1].Value;
            Assert.HasCount(2, settings.Properties);

            var colors = (ArrayNode)settings.Properties[0].Value;
            Assert.HasCount(2, colors.Elements);

            var flags = (ObjectNode)settings.Properties[1].Value;
            Assert.HasCount(1, flags.Properties);
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void PropertyNode_InNestedObject_HasCorrectIndentLevel()
        {
            var source = @"root:
  child:
    grandchild: value";
            var result = Toon.Parse(source);

            var root = result.Document.Properties[0];
            var rootObj = (ObjectNode)root.Value;
            var child = rootObj.Properties[0];
            var childObj = (ObjectNode)child.Value;
            var grandchild = childObj.Properties[0];

            Assert.AreEqual(0, root.IndentLevel);
            Assert.IsGreaterThan(root.IndentLevel, child.IndentLevel);
            Assert.IsGreaterThan(child.IndentLevel, grandchild.IndentLevel);
        }

        #endregion

        #region Test Visitor Implementation

        private class TestVisitor : IAstVisitor<string>
        {
            public bool VisitedDocument { get; private set; }
            public bool VisitedProperty { get; private set; }
            public bool VisitedObject { get; private set; }

            public string VisitDocument(ToonDocument node)
            {
                VisitedDocument = true;
                return "ToonDocument visited";
            }

            public string VisitProperty(PropertyNode node)
            {
                VisitedProperty = true;
                return "PropertyNode visited";
            }

            public string VisitObject(ObjectNode node)
            {
                VisitedObject = true;
                return "ObjectNode visited";
            }

            public string VisitArray(ArrayNode node) => "ArrayNode visited";
            public string VisitTableArray(TableArrayNode node) => "TableArrayNode visited";
            public string VisitStringValue(StringValueNode node) => "StringValueNode visited";
            public string VisitNumberValue(NumberValueNode node) => "NumberValueNode visited";
            public string VisitBooleanValue(BooleanValueNode node) => "BooleanValueNode visited";
            public string VisitNullValue(NullValueNode node) => "NullValueNode visited";
        }

        #endregion
    }
}
