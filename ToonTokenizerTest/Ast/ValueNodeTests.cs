using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Ast
{
    /// <summary>
    /// Tests for AST value node types to ensure complete code coverage.
    /// Covers NullValueNode, BooleanValueNode, StringValueNode, and NumberValueNode properties and methods.
    /// </summary>
    [TestClass]
    public class ValueNodeTests
    {
        #region NullValueNode Tests

        [TestMethod]
        public void NullValueNode_RawValue_CanBeSetAndRetrieved()
        {
            var node = new NullValueNode { RawValue = "null" };
            Assert.AreEqual("null", node.RawValue);
        }

        [TestMethod]
        public void NullValueNode_RawValue_DefaultIsEmpty()
        {
            var node = new NullValueNode();
            Assert.AreEqual(string.Empty, node.RawValue);
        }

        [TestMethod]
        public void NullValueNode_Accept_CallsVisitorMethod()
        {
            var node = new NullValueNode { RawValue = "null" };
            var visitor = new TestVisitor();
            
            var result = node.Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedNullValue);
            Assert.AreEqual("NullValueNode visited", result);
        }

        [TestMethod]
        public void NullValueNode_InDocument_ParsesCorrectly()
        {
            var source = "value: null";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = result.Document.Properties[0].Value;
            Assert.IsInstanceOfType<NullValueNode>(node);
            
            var nullNode = (NullValueNode)node;
            Assert.AreEqual("null", nullNode.RawValue);
        }

        [TestMethod]
        public void NullValueNode_InArray_ParsesCorrectly()
        {
            var source = "values[3]: null,null,null";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var array = (ArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(3, array.Elements);
            
            foreach (var element in array.Elements)
            {
                Assert.IsInstanceOfType<NullValueNode>(element);
                Assert.AreEqual("null", ((NullValueNode)element).RawValue);
            }
        }

        [TestMethod]
        public void NullValueNode_InTableArray_ParsesCorrectly()
        {
            var source = @"data[2]{id,value}:
  1,null
  2,null";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var table = (TableArrayNode)result.Document.Properties[0].Value;
            
            // Check second column (value) in both rows
            Assert.IsInstanceOfType<NullValueNode>(table.Rows[0][1]);
            Assert.IsInstanceOfType<NullValueNode>(table.Rows[1][1]);
        }

        #endregion

        #region BooleanValueNode Tests

        [TestMethod]
        public void BooleanValueNode_Value_CanBeSetToTrue()
        {
            var node = new BooleanValueNode { Value = true, RawValue = "true" };
            Assert.IsTrue(node.Value);
            Assert.AreEqual("true", node.RawValue);
        }

        [TestMethod]
        public void BooleanValueNode_Value_CanBeSetToFalse()
        {
            var node = new BooleanValueNode { Value = false, RawValue = "false" };
            Assert.IsFalse(node.Value);
            Assert.AreEqual("false", node.RawValue);
        }

        [TestMethod]
        public void BooleanValueNode_RawValue_DefaultIsEmpty()
        {
            var node = new BooleanValueNode();
            Assert.AreEqual(string.Empty, node.RawValue);
        }

        [TestMethod]
        public void BooleanValueNode_Accept_CallsVisitorMethod()
        {
            var node = new BooleanValueNode { Value = true, RawValue = "true" };
            var visitor = new TestVisitor();
            
            var result = node.Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedBooleanValue);
            Assert.AreEqual("BooleanValueNode visited", result);
        }

        [TestMethod]
        public void BooleanValueNode_TrueValue_ParsesCorrectly()
        {
            var source = "enabled: true";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (BooleanValueNode)result.Document.Properties[0].Value;
            Assert.IsTrue(node.Value);
            Assert.AreEqual("true", node.RawValue);
        }

        [TestMethod]
        public void BooleanValueNode_FalseValue_ParsesCorrectly()
        {
            var source = "enabled: false";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (BooleanValueNode)result.Document.Properties[0].Value;
            Assert.IsFalse(node.Value);
            Assert.AreEqual("false", node.RawValue);
        }

        [TestMethod]
        public void BooleanValueNode_InArray_ParsesCorrectly()
        {
            var source = "flags[4]: true,false,true,false";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var array = (ArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(4, array.Elements);
            
            Assert.IsTrue(((BooleanValueNode)array.Elements[0]).Value);
            Assert.IsFalse(((BooleanValueNode)array.Elements[1]).Value);
            Assert.IsTrue(((BooleanValueNode)array.Elements[2]).Value);
            Assert.IsFalse(((BooleanValueNode)array.Elements[3]).Value);
        }

        [TestMethod]
        public void BooleanValueNode_InTableArray_ParsesCorrectly()
        {
            var source = @"users[2]{id,active}:
  1,true
  2,false";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var table = (TableArrayNode)result.Document.Properties[0].Value;
            
            var row1Active = (BooleanValueNode)table.Rows[0][1];
            var row2Active = (BooleanValueNode)table.Rows[1][1];
            
            Assert.IsTrue(row1Active.Value);
            Assert.IsFalse(row2Active.Value);
        }

        #endregion

        #region StringValueNode Tests

        [TestMethod]
        public void StringValueNode_Value_CanBeSet()
        {
            var node = new StringValueNode { Value = "test", RawValue = "test" };
            Assert.AreEqual("test", node.Value);
            Assert.AreEqual("test", node.RawValue);
        }

        [TestMethod]
        public void StringValueNode_Value_DefaultIsEmpty()
        {
            var node = new StringValueNode();
            Assert.AreEqual(string.Empty, node.Value);
            Assert.AreEqual(string.Empty, node.RawValue);
        }

        [TestMethod]
        public void StringValueNode_Accept_CallsVisitorMethod()
        {
            var node = new StringValueNode { Value = "test", RawValue = "test" };
            var visitor = new TestVisitor();
            
            var result = node.Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedStringValue);
            Assert.AreEqual("StringValueNode visited", result);
        }

        [TestMethod]
        public void StringValueNode_UnquotedString_ParsesCorrectly()
        {
            var source = "name: John";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("John", node.Value);
        }

        [TestMethod]
        public void StringValueNode_QuotedString_ParsesCorrectly()
        {
            var source = "name: \"John Doe\"";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("John Doe", node.Value);
        }

        [TestMethod]
        public void StringValueNode_EmptyString_ParsesCorrectly()
        {
            var source = "name: \"\"";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("", node.Value);
        }

        [TestMethod]
        public void StringValueNode_WithEscapeSequences_ParsesCorrectly()
        {
            var source = "path: \"C:\\\\Users\\\\Name\"";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("C:\\Users\\Name", node.Value);
        }

        [TestMethod]
        public void StringValueNode_MultiWordUnquoted_ParsesCorrectly()
        {
            var source = "name: John Doe";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("John Doe", node.Value);
        }

        [TestMethod]
        public void StringValueNode_InArray_ParsesCorrectly()
        {
            var source = "colors[3]: red,green,blue";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var array = (ArrayNode)result.Document.Properties[0].Value;
            
            Assert.AreEqual("red", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("green", ((StringValueNode)array.Elements[1]).Value);
            Assert.AreEqual("blue", ((StringValueNode)array.Elements[2]).Value);
        }

        #endregion

        #region NumberValueNode Tests

        [TestMethod]
        public void NumberValueNode_IntegerValue_CanBeSet()
        {
            var node = new NumberValueNode { Value = 42, IsInteger = true, RawValue = "42" };
            Assert.AreEqual(42, node.Value);
            Assert.IsTrue(node.IsInteger);
            Assert.AreEqual("42", node.RawValue);
        }

        [TestMethod]
        public void NumberValueNode_FloatValue_CanBeSet()
        {
            var node = new NumberValueNode { Value = 3.14, IsInteger = false, RawValue = "3.14" };
            Assert.AreEqual(3.14, node.Value);
            Assert.IsFalse(node.IsInteger);
            Assert.AreEqual("3.14", node.RawValue);
        }

        [TestMethod]
        public void NumberValueNode_DefaultValues()
        {
            var node = new NumberValueNode();
            Assert.AreEqual(0, node.Value);
            Assert.IsFalse(node.IsInteger);
            Assert.AreEqual(string.Empty, node.RawValue);
        }

        [TestMethod]
        public void NumberValueNode_Accept_CallsVisitorMethod()
        {
            var node = new NumberValueNode { Value = 42, IsInteger = true, RawValue = "42" };
            var visitor = new TestVisitor();
            
            var result = node.Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedNumberValue);
            Assert.AreEqual("NumberValueNode visited", result);
        }

        [TestMethod]
        public void NumberValueNode_PositiveInteger_ParsesCorrectly()
        {
            var source = "count: 42";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(42, node.Value);
            Assert.IsTrue(node.IsInteger);
            Assert.AreEqual("42", node.RawValue);
        }

        [TestMethod]
        public void NumberValueNode_NegativeInteger_ParsesCorrectly()
        {
            var source = "temp: -15";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(-15, node.Value);
            Assert.IsTrue(node.IsInteger);
        }

        [TestMethod]
        public void NumberValueNode_Zero_ParsesCorrectly()
        {
            var source = "value: 0";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(0, node.Value);
            Assert.IsTrue(node.IsInteger);
        }

        [TestMethod]
        public void NumberValueNode_Float_ParsesCorrectly()
        {
            var source = "pi: 3.14159";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(3.14159, node.Value, 0.00001);
            Assert.IsFalse(node.IsInteger);
        }

        [TestMethod]
        public void NumberValueNode_ScientificNotation_ParsesCorrectly()
        {
            var source = "large: 1.5e10";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var node = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(1.5e10, node.Value, 1e7);
            Assert.IsFalse(node.IsInteger);
        }

        [TestMethod]
        public void NumberValueNode_InArray_ParsesCorrectly()
        {
            var source = "numbers[3]: 1,2,3";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var array = (ArrayNode)result.Document.Properties[0].Value;
            
            Assert.AreEqual(1, ((NumberValueNode)array.Elements[0]).Value);
            Assert.AreEqual(2, ((NumberValueNode)array.Elements[1]).Value);
            Assert.AreEqual(3, ((NumberValueNode)array.Elements[2]).Value);
        }

        [TestMethod]
        public void NumberValueNode_InTableArray_ParsesCorrectly()
        {
            var source = @"data[2]{id,score}:
  1,95.5
  2,87.3";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var table = (TableArrayNode)result.Document.Properties[0].Value;
            
            var row1Id = (NumberValueNode)table.Rows[0][0];
            var row1Score = (NumberValueNode)table.Rows[0][1];
            
            Assert.AreEqual(1, row1Id.Value);
            Assert.IsTrue(row1Id.IsInteger);
            Assert.AreEqual(95.5, row1Score.Value, 0.1);
            Assert.IsFalse(row1Score.IsInteger);
        }

        #endregion

        #region ValueNode Base Class Tests

        [TestMethod]
        public void ValueNode_RawValue_InheritedByAllTypes()
        {
            ValueNode stringNode = new StringValueNode { RawValue = "test" };
            ValueNode numberNode = new NumberValueNode { RawValue = "42" };
            ValueNode boolNode = new BooleanValueNode { RawValue = "true" };
            ValueNode nullNode = new NullValueNode { RawValue = "null" };
            
            Assert.AreEqual("test", stringNode.RawValue);
            Assert.AreEqual("42", numberNode.RawValue);
            Assert.AreEqual("true", boolNode.RawValue);
            Assert.AreEqual("null", nullNode.RawValue);
        }

        [TestMethod]
        public void ValueNode_AllTypes_ImplementAccept()
        {
            var visitor = new TestVisitor();
            
            new StringValueNode().Accept(visitor);
            new NumberValueNode().Accept(visitor);
            new BooleanValueNode().Accept(visitor);
            new NullValueNode().Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedStringValue);
            Assert.IsTrue(visitor.VisitedNumberValue);
            Assert.IsTrue(visitor.VisitedBooleanValue);
            Assert.IsTrue(visitor.VisitedNullValue);
        }

        #endregion

        #region Mixed Value Types Tests

        [TestMethod]
        public void MixedValueTypes_InSingleDocument_ParsesCorrectly()
        {
            var source = @"stringVal: Hello
intVal: 42
floatVal: 3.14
boolTrue: true
boolFalse: false
nullVal: null";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(6, result.Document.Properties);
            
            Assert.IsInstanceOfType<StringValueNode>(result.Document.Properties[0].Value);
            Assert.IsInstanceOfType<NumberValueNode>(result.Document.Properties[1].Value);
            Assert.IsInstanceOfType<NumberValueNode>(result.Document.Properties[2].Value);
            Assert.IsInstanceOfType<BooleanValueNode>(result.Document.Properties[3].Value);
            Assert.IsInstanceOfType<BooleanValueNode>(result.Document.Properties[4].Value);
            Assert.IsInstanceOfType<NullValueNode>(result.Document.Properties[5].Value);
        }

        [TestMethod]
        public void MixedValueTypes_InArray_ParsesCorrectly()
        {
            var source = "mixed[5]: hello,42,3.14,true,null";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var array = (ArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(5, array.Elements);
            
            Assert.IsInstanceOfType<StringValueNode>(array.Elements[0]);
            Assert.IsInstanceOfType<NumberValueNode>(array.Elements[1]);
            Assert.IsInstanceOfType<NumberValueNode>(array.Elements[2]);
            Assert.IsInstanceOfType<BooleanValueNode>(array.Elements[3]);
            Assert.IsInstanceOfType<NullValueNode>(array.Elements[4]);
        }

        #endregion

        #region Test Visitor Implementation

        private class TestVisitor : IAstVisitor<string>
        {
            public bool VisitedStringValue { get; private set; }
            public bool VisitedNumberValue { get; private set; }
            public bool VisitedBooleanValue { get; private set; }
            public bool VisitedNullValue { get; private set; }

            public string VisitDocument(ToonDocument node) => "ToonDocument visited";
            public string VisitProperty(PropertyNode node) => "PropertyNode visited";
            public string VisitObject(ObjectNode node) => "ObjectNode visited";
            public string VisitArray(ArrayNode node) => "ArrayNode visited";
            public string VisitTableArray(TableArrayNode node) => "TableArrayNode visited";

            public string VisitStringValue(StringValueNode node)
            {
                VisitedStringValue = true;
                return "StringValueNode visited";
            }

            public string VisitNumberValue(NumberValueNode node)
            {
                VisitedNumberValue = true;
                return "NumberValueNode visited";
            }

            public string VisitBooleanValue(BooleanValueNode node)
            {
                VisitedBooleanValue = true;
                return "BooleanValueNode visited";
            }

            public string VisitNullValue(NullValueNode node)
            {
                VisitedNullValue = true;
                return "NullValueNode visited";
            }
        }

        #endregion
    }
}
