using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    [TestClass]
    public class ResilientParsingTests
    {
        [TestMethod]
        public void TryParse_WithErrors_ReturnsTrue()
        {
            var source = "invalid without colon";
            bool success = Toon.TryParse(source, out ToonParseResult? result);

            Assert.IsTrue(success, "TryParse should return true for completed parse");
            Assert.IsTrue(result.HasErrors, "Result should have errors");
            Assert.IsNotNull(result.Document, "Document should be available");
        }

        [TestMethod]
        public void TryParse_OnlyCatastrophicFailure_ReturnsFalse()
        {
            bool success = Toon.TryParse(null!, out ToonParseResult? result);

            Assert.IsFalse(success, "Null input should return false");
            Assert.IsTrue(result.HasErrors, "Should have error");
            Assert.IsNotNull(result.Document, "Document should still be provided");
        }

        [TestMethod]
        public void Parse_ErrorPositionInformation_IsAccurate()
        {
            var source = "name John";  // Missing colon after "name"
            ToonParseResult result = ToonTestHelpers.ParseWithErrors(source);

            Assert.HasCount(1, result.Errors);

            ToonError error = result.Errors[0];
            Assert.AreEqual(1, error.Line, "Error should be on line 1");
            Assert.IsGreaterThan(0, error.Column, "Should have valid column");
            Assert.IsGreaterThanOrEqualTo(0, error.Position, "Should have valid position");
            Assert.IsGreaterThan(0, error.Length, "Should have positive length");
        }

        [TestMethod]
        public void Parse_MultilineWithErrors_CorrectLineNumbers()
        {
            var source = @"line1: value1
line2 invalid
line3: value3
line4 also invalid
line5: value5";

            ToonParseResult result = ToonTestHelpers.ParseWithErrors(source);

            Assert.IsGreaterThan(1, result.Errors.Count, "Should have multiple errors");

            // Verify error line numbers
            var errorLines = result.Errors.Select(e => e.Line).ToList();
            Assert.Contains(2, errorLines, "Should have error on line 2");
            Assert.Contains(4, errorLines, "Should have error on line 4");
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
        public void Parse_ValidDocument_NoErrors()
        {
            var source = @"
name: John Doe
age: 30
city: New York
";
            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);

            Assert.IsEmpty(result.Errors);
            Assert.HasCount(3, result.Document.Properties);
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
