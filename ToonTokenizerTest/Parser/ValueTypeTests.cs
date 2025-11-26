using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    [TestClass]
    public class ValueTypeTests
    {
        [TestMethod]
        public void Parse_StringValue_WithoutQuotes_ParsesCorrectly()
        {
            var source = "name: John";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("John", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithDoubleQuotes_ParsesCorrectly()
        {
            var source = "name: \"John Doe\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("John Doe", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithSingleQuotes_ParsesCorrectly()
        {
            var source = "name: 'John Doe'";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("John Doe", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithEscapedNewline_ParsesCorrectly()
        {
            var source = "text: \"Line1\\nLine2\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("Line1\nLine2", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithEscapedTab_ParsesCorrectly()
        {
            var source = "text: \"Col1\\tCol2\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("Col1\tCol2", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithEscapedBackslash_ParsesCorrectly()
        {
            var source = "path: \"C:\\\\Users\\\\Name\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("C:\\Users\\Name", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithEscapedQuote_ParsesCorrectly()
        {
            var source = "quote: \"He said \\\"Hello\\\"\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("He said \"Hello\"", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_Empty_ParsesCorrectly()
        {
            var source = "text: \"\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("", value.Value);
        }

        [TestMethod]
        public void Parse_NumberValue_PositiveInteger_ParsesCorrectly()
        {
            var source = "count: 42";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(42.0, value.Value);
            Assert.IsTrue(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_NegativeInteger_ParsesCorrectly()
        {
            var source = "temperature: -15";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(-15.0, value.Value);
            Assert.IsTrue(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_Zero_ParsesCorrectly()
        {
            var source = "value: 0";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(0.0, value.Value);
            Assert.IsTrue(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_PositiveFloat_ParsesCorrectly()
        {
            var source = "pi: 3.14159";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(3.14159, value.Value, 0.00001);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_NegativeFloat_ParsesCorrectly()
        {
            var source = "value: -2.5";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(-2.5, value.Value, 0.0001);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_ScientificNotationPositive_ParsesCorrectly()
        {
            var source = "avogadro: 6.022e23";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(6.022e23, value.Value, 1e20);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_ScientificNotationNegative_ParsesCorrectly()
        {
            var source = "small: 1.5e-10";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(1.5e-10, value.Value, 1e-15);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_ScientificNotationUppercaseE_ParsesCorrectly()
        {
            var source = "value: 2.5E3";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(2500.0, value.Value, 0.1);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_BooleanValue_True_ParsesCorrectly()
        {
            var source = "active: true";
            BooleanValueNode value = ToonTestHelpers.ParseAndGetValue<BooleanValueNode>(source);

            Assert.IsTrue(value.Value);
            Assert.AreEqual("true", value.RawValue);
        }

        [TestMethod]
        public void Parse_BooleanValue_False_ParsesCorrectly()
        {
            var source = "active: false";
            BooleanValueNode value = ToonTestHelpers.ParseAndGetValue<BooleanValueNode>(source);

            Assert.IsFalse(value.Value);
            Assert.AreEqual("false", value.RawValue);
        }

        [TestMethod]
        public void Parse_NullValue_ParsesCorrectly()
        {
            var source = "value: null";
            NullValueNode value = ToonTestHelpers.ParseAndGetValue<NullValueNode>(source);

            Assert.AreEqual("null", value.RawValue);
        }

        [TestMethod]
        public void Parse_MixedPrimitiveTypes_ParsesCorrectly()
        {
            var source = @"stringVal: Hello
intVal: 42
floatVal: 3.14
boolVal: true
nullVal: null";
            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            Assert.HasCount(5, result.Document.Properties);
            Assert.IsInstanceOfType<StringValueNode>(result.Document.Properties[0].Value);
            Assert.IsInstanceOfType<NumberValueNode>(result.Document.Properties[1].Value);
            Assert.IsInstanceOfType<NumberValueNode>(result.Document.Properties[2].Value);
            Assert.IsInstanceOfType<BooleanValueNode>(result.Document.Properties[3].Value);
            Assert.IsInstanceOfType<NullValueNode>(result.Document.Properties[4].Value);
        }

        [TestMethod]
        public void Parse_ValueNode_RawValue_IsPreserved()
        {
            var source = "count: 42";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual("42", value.RawValue);
        }

        [TestMethod]
        public void Parse_StringValue_WithUnderscores_ParsesCorrectly()
        {
            var source = "name: snake_case_value";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.IsTrue(value.Value.Contains("snake") || value.Value.Contains("case"));
        }

        [TestMethod]
        public void Parse_StringValue_WithHyphens_ParsesCorrectly()
        {
            // Valid TOON: colon after property key
            var source = "name: kebab-case-value";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("kebab-case-value", value.Value);
        }

        [TestMethod]
        public void Parse_NumberValue_VeryLarge_ParsesCorrectly()
        {
            var source = "large: 999999999999";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(999999999999.0, value.Value);
        }

        [TestMethod]
        public void Parse_NumberValue_VerySmall_ParsesCorrectly()
        {
            var source = "small: 0.000001";
            NumberValueNode value = ToonTestHelpers.ParseAndGetValue<NumberValueNode>(source);

            Assert.AreEqual(0.000001, value.Value, 0.0000001);
        }
        [TestMethod]
        public void Parse_NumberValue_ForbiddenLeadingZeros_ParsesAsString()
        {
            var source1 = "val: 05";
            var source2 = "val: 0001";
            var source3 = "val: -01";
            StringValueNode doc1 = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source1);
            StringValueNode doc2 = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source2);
            StringValueNode doc3 = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source3);
            Assert.AreEqual("05", doc1.Value);
            Assert.AreEqual("0001", doc2.Value);
            Assert.AreEqual("-01", doc3.Value);
        }

        [TestMethod]
        public void Parse_StringValue_InvalidEscape_ReturnsError()
        {
            var source = "text: \"Hello\\xWorld\"";
            _ = ToonTestHelpers.ParseWithErrors(source, "Invalid escape sequence");
        }
    }
}
