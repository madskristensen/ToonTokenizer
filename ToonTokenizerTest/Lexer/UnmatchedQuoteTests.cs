using ToonTokenizer;

namespace ToonTokenizerTest.Lexer
{
    [TestClass]
    public class UnmatchedQuoteTests
    {
        [TestMethod]
        public void Parse_TrailingUnmatchedDoubleQuote_ReturnsErrorInResult()
        {
            var source = "task: Our favorite hikes together\"";
            ToonTestHelpers.ParseWithErrors(source, "Unterminated");
        }

        [TestMethod]
        public void TryParse_TrailingUnmatchedDoubleQuote_ReturnsTrueWithError()
        {
            var source = "task: Our favorite hikes together\"";
            bool success = Toon.TryParse(source, out ToonParseResult? result);
            Assert.IsTrue(success, "TryParse should return true for recoverable errors");
            ToonTestHelpers.AssertHasError(result, "Unterminated");
        }

        [TestMethod]
        public void Parse_TrailingUnmatchedSingleQuote_ReturnsErrorInResult()
        {
            var source = "task: Our favorite hikes together'";
            ToonTestHelpers.ParseWithErrors(source, "Unterminated");
        }

        [TestMethod]
        public void TryParse_TrailingUnmatchedSingleQuote_ReturnsTrueWithError()
        {
            var source = "task: Our favorite hikes together'";
            bool success = Toon.TryParse(source, out ToonParseResult? result);
            Assert.IsTrue(success, "TryParse should return true for recoverable errors");
            ToonTestHelpers.AssertHasError(result, "Unterminated");
        }

        [TestMethod]
        public void Parse_StandaloneDoubleQuote_ReturnsErrorInResult()
        {
            var source = "value: \"";
            ToonTestHelpers.ParseWithErrors(source, "Unterminated");
        }

        [TestMethod]
        public void TryParse_StandaloneDoubleQuote_ReturnsTrueWithError()
        {
            var source = "value: \"";
            bool success = Toon.TryParse(source, out ToonParseResult? result);
            Assert.IsTrue(success, "TryParse should return true for recoverable errors");
            ToonTestHelpers.AssertHasError(result, "Unterminated");
        }
    }
}



