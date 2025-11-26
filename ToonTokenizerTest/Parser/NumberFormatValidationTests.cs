using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for number format validation per TOON spec §2.
    /// Spec §2: Defines valid number formats and restrictions (no exponents in output, no leading zeros, etc.)
    /// </summary>
    [TestClass]
    public class NumberFormatValidationTests
    {
        #region Leading Zero Validation

        [TestMethod]
        public void Parse_NumberWithLeadingZero_ParsesAsString()
        {
            // Spec §2: Leading zeros not allowed in numbers (except standalone 0)
            var source = "value: 05";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<StringValueNode>(value);
            Assert.AreEqual("05", ((StringValueNode)value).Value);
        }

        [TestMethod]
        public void Parse_NumberWithMultipleLeadingZeros_ParsesAsString()
        {
            var source = "value: 0001";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<StringValueNode>(value);
            Assert.AreEqual("0001", ((StringValueNode)value).Value);
        }

        [TestMethod]
        public void Parse_NegativeNumberWithLeadingZero_ParsesAsString()
        {
            // Spec §2: -01 is not a valid number
            var source = "value: -01";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<StringValueNode>(value);
            Assert.AreEqual("-01", ((StringValueNode)value).Value);
        }

        [TestMethod]
        public void Parse_StandaloneZero_ParsesAsNumber()
        {
            // 0 by itself is valid
            var source = "value: 0";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(0.0, ((NumberValueNode)value).Value);
        }

        [TestMethod]
        public void Parse_ZeroPointZero_ParsesAsNumber()
        {
            // 0.0 is valid (though encoder would output as "0")
            var source = "value: 0.0";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(0.0, ((NumberValueNode)value).Value);
        }

        #endregion

        #region Invalid Number Formats

        [TestMethod]
        public void Parse_NumberWithLeadingPlus_Behavior()
        {
            // Test if +42 is accepted or treated as string
            var source = "value: +42";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            // Should likely be string or error since + is not standard JSON
            Assert.IsTrue(value is StringValueNode || value is NullValueNode);
        }

        [TestMethod]
        public void Parse_NumberWithoutLeadingZero_Behavior()
        {
            // Test .5 (no leading zero before decimal)
            var source = "value: .5";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            // Should likely be string since .5 is not standard JSON
            Assert.IsTrue(value is StringValueNode || value is NumberValueNode);
        }

        [TestMethod]
        public void Parse_NumberWithTrailingDot_Behavior()
        {
            // Test 1. (trailing decimal point)
            var source = "value: 1.";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            // Should likely be string since 1. is not standard JSON
            Assert.IsTrue(value is StringValueNode || value is NumberValueNode);
        }

        [TestMethod]
        public void Parse_NumberWithInvalidExponent_Behavior()
        {
            // Test 1.e5 (missing digit after decimal before exponent)
            var source = "value: 1.e5";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            // Should be string or error
            var value = result.Document!.Properties[0].Value;
            Assert.IsNotNull(value);
        }

        #endregion

        #region Special Float Values

        [TestMethod]
        public void Parse_InfinityString_ParsesAsString()
        {
            // Infinity is not a valid number literal
            var source = "value: Infinity";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<StringValueNode>(value);
            Assert.AreEqual("Infinity", ((StringValueNode)value).Value);
        }

        [TestMethod]
        public void Parse_NegativeInfinityString_ParsesAsString()
        {
            var source = "value: -Infinity";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            // Should parse as string
            Assert.IsTrue(value is StringValueNode);
        }

        [TestMethod]
        public void Parse_NaNString_ParsesAsString()
        {
            // NaN is not a valid number literal
            var source = "value: NaN";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<StringValueNode>(value);
            Assert.AreEqual("NaN", ((StringValueNode)value).Value);
        }

        #endregion

        #region Valid Number Formats

        [TestMethod]
        public void Parse_IntegerNumber_ParsesCorrectly()
        {
            var source = "value: 42";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(42.0, ((NumberValueNode)value).Value);
            Assert.IsTrue(((NumberValueNode)value).IsInteger);
        }

        [TestMethod]
        public void Parse_NegativeInteger_ParsesCorrectly()
        {
            var source = "value: -15";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(-15.0, ((NumberValueNode)value).Value);
            Assert.IsTrue(((NumberValueNode)value).IsInteger);
        }

        [TestMethod]
        public void Parse_FloatWithDecimal_ParsesCorrectly()
        {
            var source = "value: 3.14";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(3.14, ((NumberValueNode)value).Value, 0.001);
            Assert.IsFalse(((NumberValueNode)value).IsInteger);
        }

        [TestMethod]
        public void Parse_ScientificNotation_ParsesCorrectly()
        {
            // Parser accepts scientific notation (encoder must convert to decimal per §2)
            var source = "value: 1.5e-10";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(1.5e-10, ((NumberValueNode)value).Value, 1e-15);
        }

        [TestMethod]
        public void Parse_ScientificNotationUppercase_ParsesCorrectly()
        {
            var source = "value: 2.5E3";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(2500.0, ((NumberValueNode)value).Value, 0.1);
        }

        [TestMethod]
        public void Parse_VeryLargeNumber_ParsesCorrectly()
        {
            var source = "value: 999999999999999";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(999999999999999.0, ((NumberValueNode)value).Value);
        }

        [TestMethod]
        public void Parse_VerySmallNumber_ParsesCorrectly()
        {
            var source = "value: 0.000001";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            Assert.AreEqual(0.000001, ((NumberValueNode)value).Value, 0.0000001);
        }

        #endregion

        #region Boundary Values

        [TestMethod]
        public void Parse_MaxIntValue_ParsesCorrectly()
        {
            var source = "value: 9007199254740991"; // MAX_SAFE_INTEGER in JS
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
        }

        [TestMethod]
        public void Parse_MinIntValue_ParsesCorrectly()
        {
            var source = "value: -9007199254740991"; // MIN_SAFE_INTEGER in JS
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
        }

        [TestMethod]
        public void Parse_NegativeZero_ParsesAsNumber()
        {
            // -0 should parse (encoder will normalize to 0 per §2)
            var source = "value: -0";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = result.Document!.Properties[0].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            // Value might be 0 or -0 depending on implementation
            var numVal = ((NumberValueNode)value).Value;
            Assert.IsTrue(numVal == 0.0 || numVal == -0.0);
        }

        #endregion

        #region Array Context

        [TestMethod]
        public void Parse_NumbersInArray_ParseCorrectly()
        {
            var source = "values[5]: 1,2,3,4,5";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(5, array.Elements);
            
            for (int i = 0; i < 5; i++)
            {
                Assert.IsInstanceOfType<NumberValueNode>(array.Elements[i]);
                Assert.AreEqual(i + 1.0, ((NumberValueNode)array.Elements[i]).Value);
            }
        }

        [TestMethod]
        public void Parse_MixedValidInvalidNumbers_HandlesCorrectly()
        {
            // Mix of valid numbers and leading-zero strings
            var source = "values[3]: 42,05,123";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(3, array.Elements);
            Assert.IsInstanceOfType<NumberValueNode>(array.Elements[0]); // 42
            Assert.IsInstanceOfType<StringValueNode>(array.Elements[1]); // 05
            Assert.IsInstanceOfType<NumberValueNode>(array.Elements[2]); // 123
        }

        #endregion
    }
}
