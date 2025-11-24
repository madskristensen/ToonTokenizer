using ToonTokenizer;

namespace ToonTokenizerTest
{
    /// <summary>
    /// Tests for TOON encoder compliance with v3.0 specification.
    /// </summary>
    [TestClass]
    public class EncoderSpecComplianceTests
    {
        #region Number Formatting (§2)

        [TestMethod]
        public void Encode_ExponentialNumber_ProducesDecimalForm()
        {
            // Spec §2: No exponent notation (e.g., 1e6 MUST be rendered as 1000000)
            var json = @"{""large"":1e6,""small"":1e-6}";
            var toon = Toon.Encode(json);

            Assert.Contains("large: 1000000", toon, "Large number should be in decimal form");
            Assert.Contains("small: 0.000001", toon, "Small number should be in decimal form");
        }

        [TestMethod]
        public void Encode_NumberWithTrailingZeros_RemovesTrailingZeros()
        {
            // Spec §2: No trailing zeros in fractional part
            var json = @"{""value"":1.5000}";
            var toon = Toon.Encode(json);

            Assert.Contains("value: 1.5", toon, "Should remove trailing zeros");
        }

        [TestMethod]
        public void Encode_NumberWithZeroFraction_ProducesInteger()
        {
            // Spec §2: If fractional part is zero, emit as integer (1.0 → 1)
            var json = @"{""value"":1.0}";
            var toon = Toon.Encode(json);

            Assert.Contains("value: 1", toon, "Should emit integer when fraction is zero");
            Assert.DoesNotContain("1.0", toon, "Should not have decimal point");
        }

        [TestMethod]
        public void Encode_NegativeZero_NormalizesToZero()
        {
            // Spec §2: -0 MUST be normalized to 0
            var json = @"{""value"":-0.0}";
            var toon = Toon.Encode(json);

            Assert.Contains("value: 0", toon, "Negative zero should normalize to 0");
            Assert.DoesNotContain("-0", toon, "Should not have negative zero");
        }

        #endregion

        #region Line Endings (§12)

        [TestMethod]
        public void Encode_MultilineOutput_UsesLFOnly()
        {
            // Spec §12: Encoders MUST use LF (U+000A), not CRLF
            var json = @"{""a"":1,""b"":2}";
            var toon = Toon.Encode(json);

            Assert.DoesNotContain("\r\n", toon, "Should not contain CRLF");
            Assert.DoesNotContain("\r", toon, "Should not contain CR");
        }

        [TestMethod]
        public void Encode_Document_NoTrailingNewline()
        {
            // Spec §12: No trailing newline at end of document
            var json = @"{""value"":1}";
            var toon = Toon.Encode(json);

            Assert.DoesNotEndWith("\n", toon, "Should not have trailing newline");
        }

        #endregion

        #region String Quoting (§7.2)

        [TestMethod]
        public void Encode_NumericLikeStringWithExponent_Quotes()
        {
            // Spec §7.2: Strings matching numeric patterns must be quoted
            var json = @"{""value"":""1e-6""}";
            var toon = Toon.Encode(json);

            Assert.Contains("\"1e-6\"", toon, "Should quote numeric-like string with exponent");
        }

        [TestMethod]
        public void Encode_LeadingZeroString_Quotes()
        {
            // Spec §7.2: Strings like "05" or "0001" must be quoted
            var json = @"{""value"":""05""}";
            var toon = Toon.Encode(json);

            Assert.Contains("\"05\"", toon, "Should quote leading-zero string");
        }

        [TestMethod]
        public void Encode_HyphenString_Quotes()
        {
            // Spec §7.2: "-" or strings starting with "-" must be quoted
            var json = @"{""dash"":""-"",""negative"":""-abc""}";
            var toon = Toon.Encode(json);

            Assert.Contains("\"-\"", toon, "Should quote single hyphen");
            Assert.Contains("\"-abc\"", toon, "Should quote string starting with hyphen");
        }

        [TestMethod]
        public void Encode_BackslashInString_Quotes()
        {
            // Spec §7.2: Strings containing backslash must be quoted
            var json = @"{""path"":""c:\\temp""}";
            var toon = Toon.Encode(json);

            Assert.Contains("\"c:\\\\temp\"", toon, "Should quote and escape backslash");
        }

        [TestMethod]
        public void Encode_DoubleQuoteInString_Quotes()
        {
            // Spec §7.2: Strings containing double quote must be quoted
            var json = @"{""quote"":""say \""hello\""""}";
            var toon = Toon.Encode(json);

            Assert.Contains("\"say \\\"hello\\\"\"", toon, "Should quote and escape double quote");
        }

        #endregion

        #region Key Encoding (§7.3)

        [TestMethod]
        public void Encode_KeyWithDash_Quotes()
        {
            // Spec §7.3: Keys must match ^[A-Za-z_][A-Za-z0-9_.]*$ or be quoted
            var json = @"{""my-key"":1}";
            var toon = Toon.Encode(json);

            Assert.Contains("\"my-key\":", toon, "Should quote key with dash");
        }

        [TestMethod]
        public void Encode_KeyStartingWithDigit_Quotes()
        {
            // Spec §7.3: Keys starting with digit must be quoted
            var json = @"{""9lives"":1}";
            var toon = Toon.Encode(json);

            Assert.Contains("\"9lives\":", toon, "Should quote key starting with digit");
        }

        [TestMethod]
        public void Encode_ValidUnquotedKey_NoQuotes()
        {
            // Spec §7.3: Valid pattern keys can be unquoted
            var json = @"{""valid_key123"":1}";
            var toon = Toon.Encode(json);

            Assert.Contains("valid_key123:", toon, "Should not quote valid key");
            Assert.DoesNotContain("\"valid_key123\"", toon, "Should not quote valid key");
        }

        [TestMethod]
        public void Encode_KeyWithDot_NoQuotes()
        {
            // Spec §7.3: Dots are allowed in unquoted keys
            var json = @"{""data.value"":1}";
            var toon = Toon.Encode(json);

            Assert.Contains("data.value:", toon, "Should not quote key with dot");
        }

        #endregion

        #region Objects as List Items (§10 v3.0)

        [TestMethod]
        public void Encode_ListItemObject_FirstFieldOnHyphenLine()
        {
            // Spec §10: Encoders SHOULD place first field on hyphen line for non-tabular objects
            var json = @"{""items"":[{""id"":1,""name"":""Test""}]}";
            var options = new ToonEncoderOptions { UseTableArrays = false }; // Disable tabular to test list-item format
            var toon = Toon.Encode(json, options);

            Assert.Contains("- id: 1", toon, "Should place first field on hyphen line");
            Assert.Contains("  name: Test", toon, "Should place other fields at depth +1");
        }

        [TestMethod]
        public void Encode_ListItemWithTabularFirst_HeaderOnHyphenLine()
        {
            // Spec §10 v3.0: When first field is tabular array, emit header on hyphen line
            var json = @"{""items"":[{""users"":[{""id"":1,""name"":""Alice""},{""id"":2,""name"":""Bob""}],""status"":""active""}]}";
            var toon = Toon.Encode(json);

            Assert.Contains("- users[2]{id,name}:", toon, "Should place tabular header on hyphen line");
            // Rows at depth +2 (relative to hyphen)
            Assert.Contains("    1,Alice", toon, "Should place rows at depth +2");
            Assert.Contains("    2,Bob", toon, "Should place rows at depth +2");
            // Other fields at depth +1
            Assert.Contains("  status: active", toon, "Should place other fields at depth +1");
        }

        [TestMethod]
        public void Encode_EmptyListItemObject_SingleHyphen()
        {
            // Spec §10: Empty object list item is a single "-"
            var json = @"{""items"":[{}]}";
            var toon = Toon.Encode(json);

            var lines = toon.Split('\n');
            var hyphenLine = Array.Find(lines, l => l.TrimStart().StartsWith("-"));
            Assert.IsNotNull(hyphenLine, "Should have hyphen line");
            Assert.AreEqual("-", hyphenLine.TrimStart(), "Empty object should be single hyphen");
        }

        #endregion

        #region Whitespace (§12)

        [TestMethod]
        public void Encode_Document_NoTrailingSpaces()
        {
            // Spec §12: No trailing spaces at end of any line
            var json = @"{""a"":1,""b"":2}";
            var toon = Toon.Encode(json);

            var lines = toon.Split('\n');
            foreach (var line in lines)
            {
                Assert.DoesNotEndWith(" ", line, $"Line should not have trailing space: '{line}'");
            }
        }

        #endregion
    }
}
