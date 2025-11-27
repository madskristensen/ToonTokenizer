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
        public void NullValueNode_Accept_CallsVisitorMethod()
        {
            var node = new NullValueNode { RawValue = "null" };
            var visitor = new TestVisitor();
            
            var result = node.Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedNullValue);
            Assert.AreEqual("NullValueNode visited", result);
        }

        #endregion

        #region BooleanValueNode Tests

        [TestMethod]
        public void BooleanValueNode_Accept_CallsVisitorMethod()
        {
            var node = new BooleanValueNode { Value = true, RawValue = "true" };
            var visitor = new TestVisitor();
            
            var result = node.Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedBooleanValue);
            Assert.AreEqual("BooleanValueNode visited", result);
        }

        #endregion

        #region StringValueNode Tests

        [TestMethod]
        public void StringValueNode_Accept_CallsVisitorMethod()
        {
            var node = new StringValueNode { Value = "test", RawValue = "test" };
            var visitor = new TestVisitor();
            
            var result = node.Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedStringValue);
            Assert.AreEqual("StringValueNode visited", result);
        }

        #endregion

        #region NumberValueNode Tests

        [TestMethod]
        public void NumberValueNode_Accept_CallsVisitorMethod()
        {
            var node = new NumberValueNode { Value = 42, IsInteger = true, RawValue = "42" };
            var visitor = new TestVisitor();
            
            var result = node.Accept(visitor);
            
            Assert.IsTrue(visitor.VisitedNumberValue);
            Assert.AreEqual("NumberValueNode visited", result);
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
