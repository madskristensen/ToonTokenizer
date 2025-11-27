using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Extensions
{
    /// <summary>
    /// Tests for AstExtensions methods for traversing and inspecting AST structures.
    /// </summary>
    [TestClass]
    public class AstExtensionsTests
    {
        #region GetAllProperties Tests

        [TestMethod]
        public void GetAllProperties_EmptyDocument_ReturnsEmptyList()
        {
            var doc = new ToonDocument();

            var properties = doc.GetAllProperties();

            Assert.IsNotNull(properties);
            Assert.IsEmpty(properties);
        }

        [TestMethod]
        public void GetAllProperties_SingleLevelProperties_ReturnsAll()
        {
            var doc = new ToonDocument();
            doc.Properties.Add(new PropertyNode { Key = "name", Value = new StringValueNode { Value = "John" } });
            doc.Properties.Add(new PropertyNode { Key = "age", Value = new NumberValueNode { Value = 30 } });

            var properties = doc.GetAllProperties();

            Assert.HasCount(2, properties);
            Assert.AreEqual("name", properties[0].Key);
            Assert.AreEqual("age", properties[1].Key);
        }

        [TestMethod]
        public void GetAllProperties_NestedObjects_ReturnsAllRecursively()
        {
            var doc = new ToonDocument();
            var obj = new ObjectNode();
            obj.Properties.Add(new PropertyNode { Key = "city", Value = new StringValueNode { Value = "NYC" } });
            obj.Properties.Add(new PropertyNode { Key = "zip", Value = new StringValueNode { Value = "10001" } });
            doc.Properties.Add(new PropertyNode { Key = "address", Value = obj });
            doc.Properties.Add(new PropertyNode { Key = "name", Value = new StringValueNode { Value = "John" } });

            var properties = doc.GetAllProperties();

            Assert.HasCount(4, properties); // address, city, zip, name
            Assert.IsTrue(properties.Any(p => p.Key == "address"));
            Assert.IsTrue(properties.Any(p => p.Key == "city"));
            Assert.IsTrue(properties.Any(p => p.Key == "zip"));
            Assert.IsTrue(properties.Any(p => p.Key == "name"));
        }

        [TestMethod]
        public void GetAllProperties_DeeplyNested_ReturnsAll()
        {
            var doc = new ToonDocument();
            var level3 = new ObjectNode();
            level3.Properties.Add(new PropertyNode { Key = "level3", Value = new StringValueNode { Value = "deep" } });

            var level2 = new ObjectNode();
            level2.Properties.Add(new PropertyNode { Key = "level2", Value = level3 });

            var level1 = new ObjectNode();
            level1.Properties.Add(new PropertyNode { Key = "level1", Value = level2 });

            doc.Properties.Add(new PropertyNode { Key = "root", Value = level1 });

            var properties = doc.GetAllProperties();

            Assert.HasCount(4, properties); // root, level1, level2, level3
            Assert.IsTrue(properties.Any(p => p.Key == "root"));
            Assert.IsTrue(properties.Any(p => p.Key == "level1"));
            Assert.IsTrue(properties.Any(p => p.Key == "level2"));
            Assert.IsTrue(properties.Any(p => p.Key == "level3"));
        }

        #endregion

        #region FindProperty Tests

        [TestMethod]
        public void FindProperty_EmptyDocument_ReturnsNull()
        {
            var doc = new ToonDocument();

            var property = doc.FindProperty("name");

            Assert.IsNull(property);
        }

        [TestMethod]
        public void FindProperty_TopLevelProperty_ReturnsProperty()
        {
            var doc = new ToonDocument();
            var prop = new PropertyNode { Key = "name", Value = new StringValueNode { Value = "John" } };
            doc.Properties.Add(prop);

            var found = doc.FindProperty("name");

            Assert.AreSame(prop, found);
        }

        [TestMethod]
        public void FindProperty_DotNotationPath_ReturnsNestedProperty()
        {
            var doc = new ToonDocument();
            var obj = new ObjectNode();
            var cityProp = new PropertyNode { Key = "city", Value = new StringValueNode { Value = "NYC" } };
            obj.Properties.Add(cityProp);
            doc.Properties.Add(new PropertyNode { Key = "address", Value = obj });

            var found = doc.FindProperty("address.city");

            Assert.AreSame(cityProp, found);
        }

        [TestMethod]
        public void FindProperty_DeepPath_ReturnsCorrectProperty()
        {
            var doc = new ToonDocument();
            var level3 = new ObjectNode();
            var deepProp = new PropertyNode { Key = "value", Value = new StringValueNode { Value = "found" } };
            level3.Properties.Add(deepProp);

            var level2 = new ObjectNode();
            level2.Properties.Add(new PropertyNode { Key = "inner", Value = level3 });

            var level1 = new ObjectNode();
            level1.Properties.Add(new PropertyNode { Key = "middle", Value = level2 });

            doc.Properties.Add(new PropertyNode { Key = "outer", Value = level1 });

            var found = doc.FindProperty("outer.middle.inner.value");

            Assert.AreSame(deepProp, found);
        }

        [TestMethod]
        public void FindProperty_NonExistentPath_ReturnsNull()
        {
            var doc = new ToonDocument();
            doc.Properties.Add(new PropertyNode { Key = "name", Value = new StringValueNode { Value = "John" } });

            var found = doc.FindProperty("address.city");

            Assert.IsNull(found);
        }

        [TestMethod]
        public void FindProperty_EmptyPath_ReturnsNull()
        {
            var doc = new ToonDocument();
            doc.Properties.Add(new PropertyNode { Key = "name", Value = new StringValueNode { Value = "John" } });

            var found = doc.FindProperty("");

            Assert.IsNull(found);
        }

        [TestMethod]
        public void FindProperty_PathWithSpaces_HandlesCorrectly()
        {
            var doc = new ToonDocument();
            var obj = new ObjectNode();
            var prop = new PropertyNode { Key = "full name", Value = new StringValueNode { Value = "John Doe" } };
            obj.Properties.Add(prop);
            doc.Properties.Add(new PropertyNode { Key = "person", Value = obj });

            var found = doc.FindProperty("person.full name");

            Assert.AreSame(prop, found);
        }

        #endregion

        #region GetDepth Tests

        [TestMethod]
        public void GetDepth_VariousIndents_CalculatesCorrectly()
        {
            Assert.AreEqual(0, new PropertyNode { IndentLevel = 0 }.GetDepth());
            Assert.AreEqual(1, new PropertyNode { IndentLevel = 2 }.GetDepth());
            Assert.AreEqual(2, new PropertyNode { IndentLevel = 4 }.GetDepth());
            Assert.AreEqual(3, new PropertyNode { IndentLevel = 6 }.GetDepth());
            Assert.AreEqual(5, new PropertyNode { IndentLevel = 10 }.GetDepth());
        }

        #endregion

        #region ToDebugString Tests

        [TestMethod]
        public void ToDebugString_NullValueNode_ReturnsNullString()
        {
            var node = new NullValueNode();

            string debug = node.ToDebugString();

            Assert.IsNotNull(debug);
            Assert.IsTrue(debug.Contains("null") || debug.Contains("Null"));
        }

        [TestMethod]
        public void ToDebugString_StringValueNode_ReturnsStringRepresentation()
        {
            var node = new StringValueNode { Value = "test" };

            string debug = node.ToDebugString();

            Assert.IsNotNull(debug);
            Assert.Contains("test", debug);
        }

        [TestMethod]
        public void ToDebugString_NumberValueNode_ReturnsNumberRepresentation()
        {
            var node = new NumberValueNode { Value = 42 };

            string debug = node.ToDebugString();

            Assert.IsNotNull(debug);
            Assert.Contains("42", debug);
        }

        [TestMethod]
        public void ToDebugString_BooleanValueNode_ReturnsBooleanRepresentation()
        {
            var node = new BooleanValueNode { Value = true };

            string debug = node.ToDebugString();

            Assert.IsNotNull(debug);
            Assert.IsTrue(debug.Contains("true") || debug.Contains("True"));
        }

        [TestMethod]
        public void ToDebugString_PropertyNode_ReturnsKeyAndValue()
        {
            var prop = new PropertyNode
            {
                Key = "name",
                Value = new StringValueNode { Value = "John" }
            };

            string debug = prop.ToDebugString();

            Assert.IsNotNull(debug);
            Assert.Contains("name", debug);
            Assert.Contains("John", debug);
        }

        [TestMethod]
        public void ToDebugString_ObjectNode_ReturnsStructuredRepresentation()
        {
            var obj = new ObjectNode();
            obj.Properties.Add(new PropertyNode { Key = "city", Value = new StringValueNode { Value = "NYC" } });
            obj.Properties.Add(new PropertyNode { Key = "zip", Value = new StringValueNode { Value = "10001" } });

            string debug = obj.ToDebugString();

            Assert.IsNotNull(debug);
            Assert.Contains("city", debug);
            Assert.Contains("zip", debug);
        }

        [TestMethod]
        public void ToDebugString_ArrayNode_ReturnsArrayRepresentation()
        {
            var array = new ArrayNode { DeclaredSize = 2 };
            array.Elements.Add(new StringValueNode { Value = "item1" });
            array.Elements.Add(new StringValueNode { Value = "item2" });

            string debug = array.ToDebugString();

            Assert.IsNotNull(debug);
            Assert.IsTrue(debug.Contains("item1") || debug.Contains('2') || debug.Contains("Array"));
        }

        [TestMethod]
        public void ToDebugString_TableArrayNode_ReturnsTableRepresentation()
        {
            var table = new TableArrayNode { DeclaredSize = 1 };
            table.Schema.Add("id");
            table.Schema.Add("name");
            var row = new List<AstNode>
            {
                new NumberValueNode { Value = 1 },
                new StringValueNode { Value = "John" }
            };
            table.Rows.Add(row);

            string debug = table.ToDebugString();

            Assert.IsNotNull(debug);
            // Should contain schema or row information
            Assert.IsGreaterThan(0, debug.Length);
        }

        [TestMethod]
        public void ToDebugString_NestedStructure_ReturnsHierarchicalRepresentation()
        {
            var obj = new ObjectNode();
            obj.Properties.Add(new PropertyNode
            {
                Key = "address",
                Value = new ObjectNode()
            });
            var prop = new PropertyNode { Key = "person", Value = obj };

            string debug = prop.ToDebugString();

            Assert.IsNotNull(debug);
            Assert.Contains("person", debug);
            Assert.Contains("address", debug);
        }

        #endregion
    }
}
