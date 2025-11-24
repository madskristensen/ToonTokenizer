using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest
{
    [TestClass]
    public class ValueTypeTests
    {
        [TestMethod]
        public void Parse_StringValue_WithoutQuotes_ParsesCorrectly()
        {
            var source = "name: John";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("John", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithDoubleQuotes_ParsesCorrectly()
        {
            var source = "name: \"John Doe\"";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("John Doe", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithSingleQuotes_ParsesCorrectly()
        {
            var source = "name: 'John Doe'";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("John Doe", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithEscapedNewline_ParsesCorrectly()
        {
            var source = "text: \"Line1\\nLine2\"";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("Line1\nLine2", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithEscapedTab_ParsesCorrectly()
        {
            var source = "text: \"Col1\\tCol2\"";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("Col1\tCol2", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithEscapedBackslash_ParsesCorrectly()
        {
            var source = "path: \"C:\\\\Users\\\\Name\"";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("C:\\Users\\Name", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_WithEscapedQuote_ParsesCorrectly()
        {
            var source = "quote: \"He said \\\"Hello\\\"\"";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("He said \"Hello\"", value.Value);
        }

        [TestMethod]
        public void Parse_StringValue_Empty_ParsesCorrectly()
        {
            var source = "text: \"\"";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("", value.Value);
        }

        [TestMethod]
        public void Parse_NumberValue_PositiveInteger_ParsesCorrectly()
        {
            var source = "count: 42";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(42.0, value.Value);
            Assert.IsTrue(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_NegativeInteger_ParsesCorrectly()
        {
            var source = "temperature: -15";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(-15.0, value.Value);
            Assert.IsTrue(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_Zero_ParsesCorrectly()
        {
            var source = "value: 0";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(0.0, value.Value);
            Assert.IsTrue(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_PositiveFloat_ParsesCorrectly()
        {
            var source = "pi: 3.14159";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(3.14159, value.Value, 0.00001);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_NegativeFloat_ParsesCorrectly()
        {
            var source = "value: -2.5";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(-2.5, value.Value, 0.0001);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_ScientificNotationPositive_ParsesCorrectly()
        {
            var source = "avogadro: 6.022e23";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(6.022e23, value.Value, 1e20);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_ScientificNotationNegative_ParsesCorrectly()
        {
            var source = "small: 1.5e-10";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(1.5e-10, value.Value, 1e-15);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_NumberValue_ScientificNotationUppercaseE_ParsesCorrectly()
        {
            var source = "value: 2.5E3";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(2500.0, value.Value, 0.1);
            Assert.IsFalse(value.IsInteger);
        }

        [TestMethod]
        public void Parse_BooleanValue_True_ParsesCorrectly()
        {
            var source = "active: true";
            var document = Toon.Parse(source);
            var value = (BooleanValueNode)document.Properties[0].Value;

            Assert.IsTrue(value.Value);
            Assert.AreEqual("true", value.RawValue);
        }

        [TestMethod]
        public void Parse_BooleanValue_False_ParsesCorrectly()
        {
            var source = "active: false";
            var document = Toon.Parse(source);
            var value = (BooleanValueNode)document.Properties[0].Value;

            Assert.IsFalse(value.Value);
            Assert.AreEqual("false", value.RawValue);
        }

        [TestMethod]
        public void Parse_NullValue_ParsesCorrectly()
        {
            var source = "value: null";
            var document = Toon.Parse(source);
            var value = document.Properties[0].Value;

            Assert.IsInstanceOfType(value, typeof(NullValueNode));
            Assert.AreEqual("null", ((NullValueNode)value).RawValue);
        }

        [TestMethod]
        public void Parse_MixedPrimitiveTypes_ParsesCorrectly()
        {
            var source = @"stringVal: Hello
intVal: 42
floatVal: 3.14
boolVal: true
nullVal: null";
            var document = Toon.Parse(source);

            Assert.HasCount(5, document.Properties);
            Assert.IsInstanceOfType(document.Properties[0].Value, typeof(StringValueNode));
            Assert.IsInstanceOfType(document.Properties[1].Value, typeof(NumberValueNode));
            Assert.IsInstanceOfType(document.Properties[2].Value, typeof(NumberValueNode));
            Assert.IsInstanceOfType(document.Properties[3].Value, typeof(BooleanValueNode));
            Assert.IsInstanceOfType(document.Properties[4].Value, typeof(NullValueNode));
        }

        [TestMethod]
        public void Parse_ValueNode_RawValue_IsPreserved()
        {
            var source = "count: 42";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual("42", value.RawValue);
        }

        [TestMethod]
        public void Parse_StringValue_WithUnderscores_ParsesCorrectly()
        {
            var source = "name: snake_case_value";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.IsTrue(value.Value.Contains("snake") || value.Value.Contains("case"));
        }

        [TestMethod]
        public void Parse_StringValue_WithHyphens_ParsesCorrectly()
        {
            // Valid TOON: colon after property key
            var source = "name: kebab-case-value";
            var document = Toon.Parse(source);
            var value = (StringValueNode)document.Properties[0].Value;

            Assert.AreEqual("kebab-case-value", value.Value);
        }

        [TestMethod]
        public void Parse_NumberValue_VeryLarge_ParsesCorrectly()
        {
            var source = "large: 999999999999";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(999999999999.0, value.Value);
        }

        [TestMethod]
        public void Parse_NumberValue_VerySmall_ParsesCorrectly()
        {
            var source = "small: 0.000001";
            var document = Toon.Parse(source);
            var value = (NumberValueNode)document.Properties[0].Value;

            Assert.AreEqual(0.000001, value.Value, 0.0000001);
        }
        [TestMethod]
        public void Parse_NumberValue_ForbiddenLeadingZeros_ParsesAsString()
        {
            var source1 = "val: 05";
            var source2 = "val: 0001";
            var source3 = "val: -01";
            var doc1 = Toon.Parse(source1);
            var doc2 = Toon.Parse(source2);
            var doc3 = Toon.Parse(source3);
            Assert.IsInstanceOfType(doc1.Properties[0].Value, typeof(StringValueNode));
            Assert.AreEqual("05", ((StringValueNode)doc1.Properties[0].Value).Value);
            Assert.IsInstanceOfType(doc2.Properties[0].Value, typeof(StringValueNode));
            Assert.AreEqual("0001", ((StringValueNode)doc2.Properties[0].Value).Value);
            Assert.IsInstanceOfType(doc3.Properties[0].Value, typeof(StringValueNode));
            Assert.AreEqual("-01", ((StringValueNode)doc3.Properties[0].Value).Value);
        }

        [TestMethod]
        public void Parse_StringValue_InvalidEscape_ThrowsParseException()
        {
            var source = "text: \"Hello\\xWorld\"";
            Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
        }
    }
}
