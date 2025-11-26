using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for unquoted string validation per TOON spec §7.2.
    /// Spec §7.2: Defines what characters are allowed in unquoted strings and when quoting is required.
    /// </summary>
    [TestClass]
    public class UnquotedStringValidationTests
    {
        #region Prohibited Starting Characters

        [TestMethod]
        public void Parse_UnquotedStringStartingWithBrace_RequiresQuotes()
        {
            // Spec §7.2: Unquoted strings cannot start with structural characters
            var source = "value: \"{test\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("{test", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringStartingWithBracket_RequiresQuotes()
        {
            var source = "value: \"[test\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("[test", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringStartingWithColon_Behavior()
        {
            // Test if string starting with : needs quotes
            var source = "value: \":test\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual(":test", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringStartingWithComma_RequiresQuotes()
        {
            var source = "value: \",test\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual(",test", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringStartingWithHash_IsComment()
        {
            // # at start of value position should be treated as comment
            var source = "value: # not a value";
            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            // Value should be null or empty since rest of line is comment
            Assert.HasCount(1, result.Document.Properties);
            AstNode value = result.Document.Properties[0].Value;
            Assert.IsInstanceOfType<NullValueNode>(value);
        }

        #endregion

        #region Prohibited Characters in Unquoted Strings

        [TestMethod]
        public void Parse_UnquotedStringContainingColon_ParsesUpToColon()
        {
            // Colons are structural characters in TOON - time values like "12:30:45" need quotes
            // Unquoted "12:30:45" will parse "12" as a number and stop at the colon
            var source = "time: \"12:30:45\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("12:30:45", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringWithBackslash_RequiresQuotes()
        {
            // Spec §7.2: Backslashes require quoting
            var source = "path: \"c:\\\\temp\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("c:\\temp", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringWithDoubleQuote_RequiresQuotes()
        {
            // Spec §7.2: Double quotes require quoting
            var source = "text: \"say \\\"hello\\\"\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("say \"hello\"", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringWithSingleQuote_ParsesCorrectly()
        {
            // Single quotes (apostrophes) are string delimiters in TOON - must use double quotes for strings containing them
            var source = "text: \"It's working\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("It", value.Value);
            Assert.Contains("working", value.Value);
        }

        #endregion

        #region Numeric-like Strings

        [TestMethod]
        public void Parse_NumericLikeStringWithExponent_RequiresQuotes()
        {
            // Spec §7.2: Strings that look like numbers with exponents must be quoted
            var source = "value: \"1e-6\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("1e-6", value.Value);
        }

        [TestMethod]
        public void Parse_LeadingZeroString_RequiresQuotes()
        {
            // Spec §7.2: Strings like "05" must be quoted to distinguish from numbers
            var source = "zipcode: \"05401\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("05401", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedLeadingZeroString_ParsesAsString()
        {
            // Unquoted leading zero should parse as string per spec
            var source = "zipcode: 05401";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("05401", value.Value);
        }

        [TestMethod]
        public void Parse_HyphenOnlyString_RequiresQuotes()
        {
            // Spec §7.2: Single hyphen must be quoted
            var source = "dash: \"-\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("-", value.Value);
        }

        [TestMethod]
        public void Parse_StringStartingWithHyphenNotNumber_RequiresQuotes()
        {
            // Spec §7.2: Strings starting with hyphen (not followed by digit) must be quoted
            var source = "value: \"-abc\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("-abc", value.Value);
        }

        #endregion

        #region Reserved Words

        [TestMethod]
        public void Parse_TrueAsUnquotedString_ParsesAsBoolean()
        {
            // Reserved word "true" should parse as boolean
            var source = "flag: true";
            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            AstNode value = result.Document.Properties[0].Value;
            Assert.IsInstanceOfType<BooleanValueNode>(value);
            Assert.IsTrue(((BooleanValueNode)value).Value);
        }

        [TestMethod]
        public void Parse_TrueAsQuotedString_ParsesAsString()
        {
            // Quoted "true" should parse as string
            var source = "word: \"true\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("true", value.Value);
        }

        [TestMethod]
        public void Parse_FalseAsUnquotedString_ParsesAsBoolean()
        {
            var source = "flag: false";
            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            AstNode value = result.Document.Properties[0].Value;
            Assert.IsInstanceOfType<BooleanValueNode>(value);
            Assert.IsFalse(((BooleanValueNode)value).Value);
        }

        [TestMethod]
        public void Parse_NullAsUnquotedString_ParsesAsNull()
        {
            var source = "value: null";
            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            AstNode value = result.Document.Properties[0].Value;
            Assert.IsInstanceOfType<NullValueNode>(value);
        }

        [TestMethod]
        public void Parse_NullAsQuotedString_ParsesAsString()
        {
            var source = "word: \"null\"";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("null", value.Value);
        }

        #endregion

        #region Valid Unquoted Strings

        [TestMethod]
        public void Parse_SimpleUnquotedString_ParsesCorrectly()
        {
            var source = "name: John";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("John", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringWithUnderscores_ParsesCorrectly()
        {
            var source = "value: snake_case_value";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("snake", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringWithHyphens_ParsesCorrectly()
        {
            var source = "value: kebab-case-value";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("kebab-case-value", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedStringWithDots_ParsesCorrectly()
        {
            var source = "filename: document.txt";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.Contains("document", value.Value);
            Assert.Contains("txt", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedAlphanumeric_ParsesCorrectly()
        {
            var source = "code: ABC123XYZ";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("ABC123XYZ", value.Value);
        }

        #endregion

        #region Array Context

        [TestMethod]
        public void Parse_UnquotedStringsInArray_ParseCorrectly()
        {
            // Unquoted strings in array should work
            var source = "items[3]: apple,banana,cherry";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            Assert.HasCount(3, array.Elements);
            Assert.AreEqual("apple", ((StringValueNode)array.Elements[0]).Value);
        }

        [TestMethod]
        public void Parse_MixedQuotedUnquotedInArray_ParsesCorrectly()
        {
            var source = "items[3]: unquoted,\"quoted value\",another";
            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);

            Assert.HasCount(3, array.Elements);
            Assert.AreEqual("unquoted", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("quoted value", ((StringValueNode)array.Elements[1]).Value);
            Assert.AreEqual("another", ((StringValueNode)array.Elements[2]).Value);
        }

        #endregion
    }
}
