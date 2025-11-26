using ToonTokenizer;

namespace ToonTokenizerTest.Lexer
{
    [TestClass]
    public class UnmatchedQuoteTests
    {
        [TestMethod]
        public void Parse_TrailingUnmatchedDoubleQuote_ReturnsErrorInResult()
        {
            // Input: task: Our favorite hikes together"
            // The trailing quote has no matching opening quote
            // With resilient parsing, Parse returns a result with errors instead of throwing
            var source = "task: Our favorite hikes together\"";
            
            var result = Toon.Parse(source);
            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.Contains("Unterminated", result.Errors[0].Message);
        }

        [TestMethod]
        public void TryParse_TrailingUnmatchedDoubleQuote_ReturnsTrueWithError()
        {
            // TryParse should return true (parsing completed) with error info
            var source = "task: Our favorite hikes together\"";
            
            bool success = Toon.TryParse(source, out var result);
            
            Assert.IsTrue(success, "TryParse should return true for recoverable errors");
            Assert.IsTrue(result.HasErrors, "Result should contain errors");
            Assert.Contains("Unterminated", result.Errors[0].Message);
        }

        [TestMethod]
        public void Parse_TrailingUnmatchedSingleQuote_ReturnsErrorInResult()
        {
            var source = "task: Our favorite hikes together'";
            
            var result = Toon.Parse(source);
            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.Contains("Unterminated", result.Errors[0].Message);
        }

        [TestMethod]
        public void TryParse_TrailingUnmatchedSingleQuote_ReturnsTrueWithError()
        {
            var source = "task: Our favorite hikes together'";
            
            bool success = Toon.TryParse(source, out var result);
            
            Assert.IsTrue(success, "TryParse should return true for recoverable errors");
            Assert.IsTrue(result.HasErrors, "Result should contain errors");
            Assert.Contains("Unterminated", result.Errors[0].Message);
        }

        [TestMethod]
        public void Parse_StandaloneDoubleQuote_ReturnsErrorInResult()
        {
            var source = "value: \"";
            
            var result = Toon.Parse(source);
            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.Contains("Unterminated", result.Errors[0].Message);
        }

        [TestMethod]
        public void TryParse_StandaloneDoubleQuote_ReturnsTrueWithError()
        {
            var source = "value: \"";
            
            bool success = Toon.TryParse(source, out var result);
            
            Assert.IsTrue(success, "TryParse should return true for recoverable errors");
            Assert.IsTrue(result.HasErrors, "Result should contain errors");
            Assert.Contains("Unterminated", result.Errors[0].Message);
        }
    }
}


