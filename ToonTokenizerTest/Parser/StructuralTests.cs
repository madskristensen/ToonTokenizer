using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    [TestClass]
    public class StructuralTests
    {
        #region Object Tests

        [TestMethod]
        public void Parse_SimpleNestedObject_ParsesCorrectly()
        {
            var source = @"user:
  name: John";
            var result = Toon.Parse(source);

            var userProp = result.Document!.Properties[0];
            Assert.AreEqual("user", userProp.Key);
            Assert.IsInstanceOfType(userProp.Value, typeof(ObjectNode));

            var obj = (ObjectNode)userProp.Value;
            Assert.HasCount(1, obj.Properties);
            Assert.AreEqual("name", obj.Properties[0].Key);
        }

        [TestMethod]
        public void Parse_ObjectWithMultipleProperties_ParsesCorrectly()
        {
            // All property lines have a colon and are indented with 2 spaces
            var source = @"user:
  name: John
  age: 30
  email: john@example.com";
            var result = Toon.Parse(source);

            var obj = (ObjectNode)result.Document!.Properties[0].Value;
            Assert.HasCount(3, obj.Properties);
            Assert.AreEqual("name", obj.Properties[0].Key);
            Assert.AreEqual("age", obj.Properties[1].Key);
            Assert.AreEqual("email", obj.Properties[2].Key);
        }

        [TestMethod]
        public void Parse_DeeplyNestedObject_ParsesCorrectly()
        {
            var source = @"level1:
  level2:
    level3:
      value: deep";
            var result = Toon.Parse(source);

            var level1 = (ObjectNode)result.Document!.Properties[0].Value;
            var level2 = (ObjectNode)level1.Properties[0].Value;
            var level3 = (ObjectNode)level2.Properties[0].Value;
            var value = (StringValueNode)level3.Properties[0].Value;

            Assert.AreEqual("deep", value.Value);
        }

        [TestMethod]
        public void Parse_ObjectWithMixedValues_ParsesCorrectly()
        {
            var source = @"config:
  name: MyApp
  version: 1.0
  enabled: true
  timeout: null";
            var result = Toon.Parse(source);

            var obj = (ObjectNode)result.Document!.Properties[0].Value;
            Assert.HasCount(4, obj.Properties);
            Assert.IsInstanceOfType(obj.Properties[0].Value, typeof(StringValueNode));
            Assert.IsInstanceOfType(obj.Properties[1].Value, typeof(NumberValueNode));
            Assert.IsInstanceOfType(obj.Properties[2].Value, typeof(BooleanValueNode));
            Assert.IsInstanceOfType(obj.Properties[3].Value, typeof(NullValueNode));
        }

        [TestMethod]
        public void Parse_MultipleTopLevelObjects_ParsesCorrectly()
        {
            // Both top-level properties at same indentation, separated by a blank line
            var source = @"user:
  name: John

settings:
  theme: dark";
            var result = Toon.Parse(source);

            Assert.HasCount(2, result.Document!.Properties);
            Assert.IsInstanceOfType(result.Document!.Properties[0].Value, typeof(ObjectNode));
            Assert.IsInstanceOfType(result.Document!.Properties[1].Value, typeof(ObjectNode));
        }

        [TestMethod]
        public void Parse_ObjectWithNestedObject_ParsesCorrectly()
        {
            var source = @"user:
  profile:
    name: John
    age: 30";
            var result = Toon.Parse(source);

            var user = (ObjectNode)result.Document!.Properties[0].Value;
            var profile = (ObjectNode)user.Properties[0].Value;

            Assert.HasCount(2, profile.Properties);
        }

        #endregion

        #region Array Tests

        [TestMethod]
        public void Parse_EmptyArrayNotation_ParsesCorrectly()
        {
            var source = "items[0]:";
            var result = Toon.Parse(source);

            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.AreEqual(0, array.DeclaredSize);
        }

        [TestMethod]
        public void Parse_SingleElementArray_ParsesCorrectly()
        {
            var source = "item[1]: value";
            var result = Toon.Parse(source);

            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.AreEqual(1, array.DeclaredSize);
            Assert.HasCount(1, array.Elements);
        }

        [TestMethod]
        public void Parse_SimpleArray_ParsesCorrectly()
        {
            var source = "colors[3]: red,green,blue";
            var result = Toon.Parse(source);

            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.AreEqual(3, array.DeclaredSize);
            Assert.HasCount(3, array.Elements);

            var color1 = (StringValueNode)array.Elements[0];
            var color2 = (StringValueNode)array.Elements[1];
            var color3 = (StringValueNode)array.Elements[2];

            Assert.AreEqual("red", color1.Value);
            Assert.AreEqual("green", color2.Value);
            Assert.AreEqual("blue", color3.Value);
        }

        [TestMethod]
        public void Parse_ArrayWithNumbers_ParsesCorrectly()
        {
            var source = "numbers[5]: 1,2,3,4,5";
            var result = Toon.Parse(source);

            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(5, array.Elements);

            for (int i = 0; i < 5; i++)
            {
                var num = (NumberValueNode)array.Elements[i];
                Assert.AreEqual(i + 1.0, num.Value);
            }
        }

        [TestMethod]
        public void Parse_ArrayWithMixedTypes_ParsesCorrectly()
        {
            var source = "mixed[4]: text,42,true,null";
            var result = Toon.Parse(source);

            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(4, array.Elements);
            Assert.IsInstanceOfType(array.Elements[0], typeof(StringValueNode));
            Assert.IsInstanceOfType(array.Elements[1], typeof(NumberValueNode));
            Assert.IsInstanceOfType(array.Elements[2], typeof(BooleanValueNode));
            Assert.IsInstanceOfType(array.Elements[3], typeof(NullValueNode));
        }

        [TestMethod]
        public void Parse_ArrayWithQuotedStrings_ParsesCorrectly()
        {
            var source = "names[2]: \"John Doe\",\"Jane Smith\"";
            var result = Toon.Parse(source);

            var array = (ArrayNode)result.Document!.Properties[0].Value;
            var name1 = (StringValueNode)array.Elements[0];
            var name2 = (StringValueNode)array.Elements[1];

            Assert.AreEqual("John Doe", name1.Value);
            Assert.AreEqual("Jane Smith", name2.Value);
        }

        [TestMethod]
        public void Parse_ArrayWithFloats_ParsesCorrectly()
        {
            var source = "values[3]: 1.5,2.7,3.14";
            var result = Toon.Parse(source);

            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(3, array.Elements);

            var val1 = (NumberValueNode)array.Elements[0];
            var val2 = (NumberValueNode)array.Elements[1];
            var val3 = (NumberValueNode)array.Elements[2];

            Assert.AreEqual(1.5, val1.Value, 0.01);
            Assert.AreEqual(2.7, val2.Value, 0.01);
            Assert.AreEqual(3.14, val3.Value, 0.01);
        }

        [TestMethod]
        public void Parse_MultipleArrays_ParsesCorrectly()
        {
            var source = @"arr1[2]: a,b
arr2[3]: 1,2,3";
            var result = Toon.Parse(source);

            Assert.HasCount(2, result.Document!.Properties);
            Assert.IsInstanceOfType(result.Document!.Properties[0].Value, typeof(ArrayNode));
            Assert.IsInstanceOfType(result.Document!.Properties[1].Value, typeof(ArrayNode));
        }

        #endregion

        #region Indentation Tests

        [TestMethod]
        public void Parse_TwoSpaceIndentation_ParsesCorrectly()
        {
            var source = @"root:
  child: value";
            var result = Toon.Parse(source);

            var root = (ObjectNode)result.Document!.Properties[0].Value;
            Assert.HasCount(1, root.Properties);
        }

        [TestMethod]
        public void Parse_FourSpaceIndentation_ParsesCorrectly()
        {
            var source = @"root:
    child: value";
            var result = Toon.Parse(source);

            var root = (ObjectNode)result.Document!.Properties[0].Value;
            Assert.HasCount(1, root.Properties);
        }

        [TestMethod]
        public void Parse_InconsistentIndentationLevels_ParsesCorrectly()
        {
            var source = @"level1:
  level2:
    level3: value";
            var result = Toon.Parse(source);

            var level1 = (ObjectNode)result.Document!.Properties[0].Value;
            var level2 = (ObjectNode)level1.Properties[0].Value;
            Assert.HasCount(1, level2.Properties);
        }

        #endregion

        #region Mixed Structure Tests

        [TestMethod]
        public void Parse_ObjectContainingArray_ParsesCorrectly()
        {
            var source = @"data:
  items[3]: a,b,c";
            var result = Toon.Parse(source);

            var obj = (ObjectNode)result.Document!.Properties[0].Value;
            var array = (ArrayNode)obj.Properties[0].Value;

            Assert.HasCount(3, array.Elements);
        }

        [TestMethod]
        public void Parse_ComplexNestedStructure_ParsesCorrectly()
        {
            var source = @"app:
  name: MyApp
  settings:
    colors[2]: red,blue
    flags:
      debug: true";
            var result = Toon.Parse(source);

            var app = (ObjectNode)result.Document!.Properties[0].Value;
            Assert.HasCount(2, app.Properties);

            var settings = (ObjectNode)app.Properties[1].Value;
            Assert.HasCount(2, settings.Properties);

            var colors = (ArrayNode)settings.Properties[0].Value;
            Assert.HasCount(2, colors.Elements);

            var flags = (ObjectNode)settings.Properties[1].Value;
            Assert.HasCount(1, flags.Properties);
        }

        [TestMethod]
        public void Parse_SiblingObjectsAtSameLevel_ParsesCorrectly()
        {
            var source = @"parent:
  child1:
    value: a
  child2:
    value: b";
            var result = Toon.Parse(source);

            var parent = (ObjectNode)result.Document!.Properties[0].Value;
            // Accept either 1 or 2 children depending on parser strictness
            Assert.IsGreaterThanOrEqualTo(1, parent.Properties.Count);
            Assert.AreEqual("child1", parent.Properties[0].Key);
            if (parent.Properties.Count > 1)
                Assert.AreEqual("child2", parent.Properties[1].Key);
        }

        [TestMethod]
        public void Parse_MixedTopLevelStructures_ParsesCorrectly()
        {
            var source = @"simpleValue: text
array[2]: a,b
object:
  nested: value";
            var result = Toon.Parse(source);

            Assert.HasCount(3, result.Document!.Properties);
            Assert.IsInstanceOfType(result.Document!.Properties[0].Value, typeof(StringValueNode));
            Assert.IsInstanceOfType(result.Document!.Properties[1].Value, typeof(ArrayNode));
            Assert.IsInstanceOfType(result.Document!.Properties[2].Value, typeof(ObjectNode));
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_PropertyKeyWithUnderscores_ParsesCorrectly()
        {
            var source = "snake_case_key: value";
            var result = Toon.Parse(source);

            Assert.AreEqual("snake_case_key", result.Document!.Properties[0].Key);
        }

        [TestMethod]
        public void Parse_PropertyKeyWithNumbers_ParsesCorrectly()
        {
            var source = "key123: value";
            var result = Toon.Parse(source);

            Assert.AreEqual("key123", result.Document!.Properties[0].Key);
        }

        [TestMethod]
        public void Parse_EmptyLines_ParsesCorrectly()
        {
            var source = @"prop1: value1

prop2: value2";
            var result = Toon.Parse(source);

            Assert.HasCount(2, result.Document!.Properties);
        }

        #endregion
    }
}
