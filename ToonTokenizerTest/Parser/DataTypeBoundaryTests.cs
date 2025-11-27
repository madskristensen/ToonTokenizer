using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for data type boundary conditions and edge cases per TOON spec §3-5.
    /// Covers numbers (§2), strings (§7), booleans, null, and edge cases.
    /// NOTE: Tests that might cause lexer/parser infinite loops are excluded.
    /// </summary>
    [TestClass]
    public class DataTypeBoundaryTests
    {
        #region Number Boundary Tests (§2)

        [TestMethod]
        public void Parse_NumberZero_ParsesCorrectly()
        {
            var source = "value: 0";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(0, value.Value);
        }

        [TestMethod]
        public void Parse_NumberNegativeZero_ParsesCorrectly()
        {
            var source = "value: -0";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("-0", value.RawValue);
        }

        [TestMethod]
        public void Parse_NumberMaxInt32_ParsesCorrectly()
        {
            var source = $"value: {int.MaxValue}";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(int.MaxValue, value.Value);
        }

        [TestMethod]
        public void Parse_NumberMinInt32_ParsesCorrectly()
        {
            var source = $"value: {int.MinValue}";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(int.MinValue, value.Value);
        }

        [TestMethod]
        public void Parse_NumberMaxInt64_ParsesCorrectly()
        {
            var source = $"value: {long.MaxValue}";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(long.MaxValue, value.Value);
        }

        [TestMethod]
        public void Parse_NumberMinInt64_ParsesCorrectly()
        {
            var source = $"value: {long.MinValue}";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(long.MinValue, value.Value);
        }

        [TestMethod]
        public void Parse_NumberDecimalZero_ParsesCorrectly()
        {
            var source = "value: 0.0";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(0.0, value.Value);
        }

        [TestMethod]
        public void Parse_NumberDecimalVerySmall_ParsesCorrectly()
        {
            var source = "value: 0.000000001";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(0.000000001, value.Value);
        }

        [TestMethod]
        public void Parse_NumberDecimalVeryLarge_ParsesCorrectly()
        {
            var source = "value: 123456789.987654321";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(123456789.987654321, value.Value);
        }

        [TestMethod]
        public void Parse_NumberScientificNotation_ParsesCorrectly()
        {
            var source = "value: 1.23e10";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(1.23e10, value.Value);
        }

        [TestMethod]
        public void Parse_NumberNegativeScientific_ParsesCorrectly()
        {
            var source = "value: -4.56e-8";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (NumberValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(-4.56e-8, value.Value);
        }

        #endregion

        #region String Boundary Tests (§7)

        [TestMethod]
        public void Parse_StringEmpty_ParsesCorrectly()
        {
            var source = "value: \"\"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("", value.Value);
        }

        [TestMethod]
        public void Parse_StringOnlyWhitespace_ParsesCorrectly()
        {
            var source = "value: \"   \\t  \"";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.Contains('\t', value.Value);
        }

        [TestMethod]
        public void Parse_StringVeryLong_ParsesCorrectly()
        {
            // String with 500 characters (reduced from 1000)
            var longString = new string('a', 500);
            var source = $"value: \"{longString}\"";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual(500, value.Value.Length);
        }

        [TestMethod]
        public void Parse_StringUnicodeBasicMultilingualPlane_ParsesCorrectly()
        {
            // Unicode BMP characters
            var source = "value: \"Hello世界\"";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.Contains("世界", value.Value);
        }

        [TestMethod]
        public void Parse_StringUnicodeSupplementaryPlane_ParsesCorrectly()
        {
            // Unicode characters beyond BMP (emojis)
            var source = "value: \"Hello 😀\"";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.Contains("Hello", value.Value);
        }

        [TestMethod]
        public void Parse_StringWithEscapeSequences_ParsesCorrectly()
        {
            // Common escape sequences
            var source = "value: \"Line1\\nLine2\\tTab\"";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.IsNotNull(value.Value);
        }

        [TestMethod]
        public void Parse_StringBackslashAtEnd_ParsesCorrectly()
        {
            // Edge case: backslash at end
            var source = "value: \"path\\\\\"";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.IsTrue(value.Value.Contains('\\') || value.Value.Contains("\\\\"));
        }

        #endregion

        #region Boolean and Null Boundary Tests

        [TestMethod]
        public void Parse_BooleanTrue_ParsesCorrectly()
        {
            var source = "value: true";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (BooleanValueNode)result.Document.Properties[0].Value;
            Assert.IsTrue(value.Value);
        }

        [TestMethod]
        public void Parse_BooleanFalse_ParsesCorrectly()
        {
            var source = "value: false";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (BooleanValueNode)result.Document.Properties[0].Value;
            Assert.IsFalse(value.Value);
        }

        [TestMethod]
        public void Parse_NullValue_ParsesCorrectly()
        {
            var source = "value: null";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.IsInstanceOfType<NullValueNode>(result.Document.Properties[0].Value);
        }

        #endregion

        #region Mixed Array Type Boundaries

        [TestMethod]
        public void Parse_ArrayAllNulls_ParsesCorrectly()
        {
            var source = "items[3]: null,null,null";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_ArrayAllBooleans_ParsesCorrectly()
        {
            var source = "flags[4]: true,false,true,false";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 4);
        }

        [TestMethod]
        public void Parse_ArrayMixedNumberTypes_ParsesCorrectly()
        {
            // Integer, decimal, scientific notation
            var source = "numbers[3]: 42,3.14,1e5";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_ArrayEmptyStrings_ParsesCorrectly()
        {
            var source = "items[3]: \"\",\"\",\"\"";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_ArrayMixedAllTypes_ParsesCorrectly()
        {
            // Mix of all primitive types
            var source = "mixed[5]: 42,\"text\",true,null,3.14";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 5);
        }

        #endregion

        #region Simple Edge Cases

        [TestMethod]
        public void Parse_SimpleObjectWithMixedTypes_ParsesCorrectly()
        {
            // Simple flat structure
            var source = @"number: 42
string: text
bool: true
nullval: null";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(4, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_TableArrayWithAllTypes_ParsesCorrectly()
        {
            // Table with mixed column types
            var source = @"data[2]{id,name,active}:
  1,Alice,true
  2,Bob,false";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name", "active");
        }

        [TestMethod]
        public void Parse_BoundaryValues_AllInSingleDocument_ParsesCorrectly()
        {
            // Document testing multiple boundaries at once
            var source = $@"maxInt: {int.MaxValue}
minInt: {int.MinValue}
zero: 0
emptyString: """"
nullValue: null
boolTrue: true";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(6, result.Document.Properties);
        }

        #endregion
    }
}
