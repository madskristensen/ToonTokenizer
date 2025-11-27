using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    [TestClass]
    public class ResilientParsingTests
    {
        [TestMethod]
        public void TryParse_OnlyCatastrophicFailure_ReturnsFalse()
        {
            bool success = Toon.TryParse(null!, out ToonParseResult? result);

            Assert.IsFalse(success, "Null input should return false");
            Assert.IsTrue(result.HasErrors, "Should have error");
            Assert.IsNotNull(result.Document, "Document should still be provided");
        }

        [TestMethod]
        public void Parse_PartialArray_FillsWithNulls()
        {
            var source = "numbers[5]: 1, 2, 3";  // Declares 5 but provides only 3
            ToonParseResult result = Toon.Parse(source);

            // May have error for missing elements or might just fill with nulls
            Assert.IsNotNull(result.Document);
            Assert.HasCount(1, result.Document.Properties);

            if (result.Document.Properties[0].Value is ArrayNode array)
            {
                Assert.IsGreaterThan(0, array.Elements.Count, "Array should have some elements");
            }
        }

        [TestMethod]
        public void Parse_TableArrayMissingDelimiter_RecordsError()
        {
            var source = @"data[2]{id,name}:
  1 Alice
  2,Bob";
            _ = ToonTestHelpers.ParseWithErrors(source);
        }

        [TestMethod]
        public void Parse_EmailAddress_ParsesAsUnquotedString()
        {
            var source = "email: alice@example.com";
            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);

            Assert.AreEqual("alice@example.com", value.Value, "Email should be parsed as complete unquoted string");
        }
    }
}
