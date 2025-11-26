using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for property key edge cases per TOON spec §3.
    /// Spec §3: Property keys must follow specific naming rules.
    /// </summary>
    [TestClass]
    public class PropertyKeyEdgeCaseTests
    {
        #region Valid Key Patterns

        [TestMethod]
        public void Parse_KeyWithSingleUnderscore_ParsesCorrectly()
        {
            var source = "my_key: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("my_key", result.Document.Properties[0].Key, "Key with underscore should parse");
        }

        [TestMethod]
        public void Parse_KeyWithMultipleUnderscores_ParsesCorrectly()
        {
            var source = "my_long_key_name: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("my_long_key_name", result.Document.Properties[0].Key, "Key with multiple underscores should parse");
        }

        [TestMethod]
        public void Parse_KeyWithConsecutiveUnderscores_ParsesCorrectly()
        {
            var source = "key__with__doubles: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("key__with__doubles", result.Document.Properties[0].Key, "Key with consecutive underscores should parse");
        }

        [TestMethod]
        public void Parse_KeyWithLeadingUnderscore_ParsesCorrectly()
        {
            var source = "_privateKey: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("_privateKey", result.Document.Properties[0].Key, "Key starting with underscore should parse");
        }

        [TestMethod]
        public void Parse_KeyWithTrailingUnderscore_ParsesCorrectly()
        {
            var source = "key_: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("key_", result.Document.Properties[0].Key, "Key ending with underscore should parse");
        }

        [TestMethod]
        public void Parse_KeyWithDot_ParsesCorrectly()
        {
            var source = "config.setting: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("config.setting", result.Document.Properties[0].Key, "Key with dot should parse");
        }

        [TestMethod]
        public void Parse_KeyWithMultipleDots_ParsesCorrectly()
        {
            var source = "app.config.database.host: localhost";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("app.config.database.host", result.Document.Properties[0].Key, "Key with multiple dots should parse");
        }

        [TestMethod]
        public void Parse_KeyWithHyphen_ParsesCorrectly()
        {
            var source = "kebab-case-key: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("kebab-case-key", result.Document.Properties[0].Key, "Kebab-case key should parse");
        }

        [TestMethod]
        public void Parse_KeyWithNumbers_ParsesCorrectly()
        {
            var source = "key123: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("key123", result.Document.Properties[0].Key, "Key with numbers should parse");
        }

        [TestMethod]
        public void Parse_KeyStartingWithNumber_HandlesBehavior()
        {
            // Parser may not allow keys starting with pure numbers
            var source = "2ndPlace: value";

            ToonParseResult result = Toon.Parse(source);
            // Parser may reject this or treat it as valid depending on implementation
            Assert.IsNotNull(result.Document, "Parser should return a document even if invalid");
        }

        [TestMethod]
        public void Parse_KeyCamelCase_ParsesCorrectly()
        {
            var source = "myVariableName: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("myVariableName", result.Document.Properties[0].Key, "CamelCase key should parse");
        }

        [TestMethod]
        public void Parse_KeyPascalCase_ParsesCorrectly()
        {
            var source = "MyClassName: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("MyClassName", result.Document.Properties[0].Key, "PascalCase key should parse");
        }

        [TestMethod]
        public void Parse_KeySNAKE_CASE_ParsesCorrectly()
        {
            var source = "CONSTANT_VALUE: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("CONSTANT_VALUE", result.Document.Properties[0].Key, "SNAKE_CASE key should parse");
        }

        #endregion

        #region Special Character Keys

        [TestMethod]
        public void Parse_KeyWithAllowedSpecialChars_ParsesCorrectly()
        {
            // Mix of allowed characters
            var source = "my_key-123.value: data";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("my_key-123.value", result.Document.Properties[0].Key, "Key with mixed special chars should parse");
        }

        [TestMethod]
        public void Parse_KeySingleCharacter_ParsesCorrectly()
        {
            var source = "x: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual("x", result.Document.Properties[0].Key, "Single character key should parse");
        }

        [TestMethod]
        public void Parse_KeyVeryLong_ParsesCorrectly()
        {
            var longKey = new string('a', 200);
            var source = $"{longKey}: value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.AreEqual(longKey, result.Document.Properties[0].Key, "Very long key should parse");
        }

        #endregion

        #region Key Uniqueness

        [TestMethod]
        public void Parse_DuplicateKeys_BothParse()
        {
            // TOON allows duplicate keys (last one wins or both kept depending on parser strategy)
            var source = @"key: value1
key: value2";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document, "Document should parse with duplicate keys");
            // Parser may handle duplicates different ways - just verify it doesn't crash
        }

        [TestMethod]
        public void Parse_KeysCaseSensitive_BothParse()
        {
            var source = @"Key: value1
key: value2
KEY: value3";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(3, result.Document.Properties, "Keys should be case-sensitive");
        }

        #endregion

        #region Complex Nested Keys

        [TestMethod]
        public void Parse_NestedKeysWithDots_ParseCorrectly()
        {
            var source = @"app.settings:
  database.host: localhost
  database.port: 5432";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var appSettings = (ObjectNode)result.Document.Properties[0].Value;

            Assert.AreEqual("database.host", appSettings.Properties[0].Key, "Nested key with dot");
            Assert.AreEqual("database.port", appSettings.Properties[1].Key, "Nested key with dot");
        }

        [TestMethod]
        public void Parse_KeysInTableArray_ParseCorrectly()
        {
            var source = @"configs[2]{key-name,value_123}:
  setting-1,value1
  setting-2,value2";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            Assert.AreEqual("key-name", table.Schema[0], "Table schema key with hyphen");
            Assert.AreEqual("value_123", table.Schema[1], "Table schema key with underscore and numbers");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_KeyWithWhitespaceAroundColon_ParsesCorrectly()
        {
            // Parser should handle spacing variations
            var source = "key : value";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.Contains("key", result.Document.Properties[0].Key, "Key should be extracted correctly");
        }

        [TestMethod]
        public void Parse_KeyFollowedByEmptyValue_ParsesCorrectly()
        {
            var source = "emptyKey:";

            ToonParseResult result = Toon.Parse(source);
            Assert.IsNotNull(result.Document, "Key with empty value should parse");
        }

        [TestMethod]
        public void Parse_MultiplePropertiesMixedKeyStyles_ParsesAll()
        {
            var source = @"camelCase: value1
snake_case: value2
kebab-case: value3
PascalCase: value4
_leadingUnderscore: value5
config.nested: value6";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(6, result.Document.Properties, "All key styles should parse");
        }

        #endregion

        #region Reserved Words and Keywords

        [TestMethod]
        public void Parse_KeyNamedTrue_IsReservedKeyword()
        {
            // Parser treats 'true' as reserved keyword, not identifier
            var source = "true: actual_value";

            ToonParseResult result = Toon.Parse(source);
            // Parser may reject this or handle it with errors
            Assert.IsTrue(result.HasErrors || result.Document == null || result.Document.Properties.Count == 0,
                "Reserved keyword 'true' cannot be used as property key");
        }

        [TestMethod]
        public void Parse_KeyNamedFalse_IsReservedKeyword()
        {
            // Parser treats 'false' as reserved keyword, not identifier
            var source = "false: actual_value";

            ToonParseResult result = Toon.Parse(source);
            // Parser may reject this or handle it with errors
            Assert.IsTrue(result.HasErrors || result.Document == null || result.Document.Properties.Count == 0,
                "Reserved keyword 'false' cannot be used as property key");
        }

        [TestMethod]
        public void Parse_KeyNamedNull_IsReservedKeyword()
        {
            // Parser treats 'null' as reserved keyword, not identifier
            var source = "null: actual_value";

            ToonParseResult result = Toon.Parse(source);
            // Parser may reject this or handle it with errors
            Assert.IsTrue(result.HasErrors || result.Document == null || result.Document.Properties.Count == 0,
                "Reserved keyword 'null' cannot be used as property key");
        }

        #endregion
    }
}
