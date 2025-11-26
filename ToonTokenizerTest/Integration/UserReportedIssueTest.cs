using ToonTokenizer;

namespace ToonTokenizerTest.Integration
{
    /// <summary>
    /// Test for the user-reported issue where TryParse throws ParseException
    /// for input like: task: Our favorite hikes together"
    /// With resilient parsing, TryParse should return true but with errors in the result.
    /// </summary>
    [TestClass]
    public class UserReportedIssueTest
    {
        [TestMethod]
        public void TryParse_UserReportedCase_ShouldReturnTrueWithErrors()
        {
            // This is the exact case the user reported
            var source = "task: Our favorite hikes together\"";
            
            // TryParse should NOT throw an exception and should return true
            // (resilient parsing - it can still tokenize most of the input)
            bool success = Toon.TryParse(source, out var result);
            
            // Should return true because parsing completed (even with errors)
            Assert.IsTrue(success, "TryParse should return true for recoverable errors");
            
            // Should have error information about the unterminated string
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.HasErrors, "Result should have errors");
            Assert.IsNotEmpty(result.Errors, "Errors collection should not be empty");
            Assert.Contains("Unterminated", result.Errors[0].Message, "Error message should mention unterminated string");
            
            // Document should still be provided (might have partial content)
            Assert.IsNotNull(result.Document, "Document should not be null");
        }

        [TestMethod]
        public void TryParse_MultilineWithUnterminatedString_ShouldReturnTrueWithErrors()
        {
            var source = @"name: John
task: Our favorite hikes together""";
            
            bool success = Toon.TryParse(source, out var result);
            
            // Should return true - parser continues after line 1
            Assert.IsTrue(success, "TryParse should return true for recoverable errors");
            Assert.IsTrue(result.HasErrors, "Result should have errors");
            Assert.Contains("Unterminated", result.Errors[0].Message);
            
            // Should have parsed the first line successfully
            Assert.IsNotNull(result.Document);
            // The parser should be able to recover and parse "name: John"
            Assert.IsGreaterThanOrEqualTo(1, result.Document.Properties.Count, "Should parse at least one property before the error");
        }

        [TestMethod]
        public void TryParse_ErrorOnLine2_ParsesLine1Successfully()
        {
            var source = @"name: John
bad-syntax-line: Our favorite hikes together""
city: Boston";
            
            bool success = Toon.TryParse(source, out var result);
            
            Assert.IsTrue(success, "TryParse should return true");
            Assert.IsTrue(result.HasErrors, "Should have errors");
            
            // Should still parse valid lines
            Assert.IsNotNull(result.Document);
            var keys = result.Document.Properties.Select(p => p.Key).ToList();
            Assert.Contains("name", keys, "Should parse line 1");
            // May or may not parse line 3 depending on error recovery
        }
    }
}

