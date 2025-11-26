using ToonTokenizer;

namespace ToonTokenizerTest.Integration
{
    [TestClass]
    public class HangTest
    {
        [TestMethod]
        public void Parse_UnterminatedStringAtEndOfLine_ShouldNotHang()
        {
            // This was causing the parser to hang
            var source = "season: spring_2025\"";

            var result = Toon.Parse(source);

            // Should complete (not hang) and have errors
            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.IsNotNull(result.Document, "Should return document");
            Assert.Contains("Unterminated", result.Errors[0].Message);
        }

        [TestMethod]
        public void Parse_UnterminatedStringFollowedByNewline_StopsAtNewline()
        {
            var source = "season: spring_2025\"\nother: value";

            var result = Toon.Parse(source);

            // Should parse and report error on first line
            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.Contains("Unterminated", result.Errors[0].Message);

            // Should be able to continue parsing after the error
            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void TryParse_UnterminatedStringAtEOF_ShouldNotHang()
        {
            var source = "value: \"no closing quote";

            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success, "Should complete parsing");
            Assert.IsTrue(result.HasErrors, "Should have errors");
        }

        [TestMethod]
        [Timeout(5000, CooperativeCancellation = true)] // 5 second timeout to catch hangs
        public void Parse_NestedObjectWithInvalidToken_ShouldNotHang()
        {
            // User reported: This was causing the parser to hang
            var source = @"context:
  task: Our favorite hikes together
  location: Boulder
  season: spring_2025""";

            var result = Toon.Parse(source);

            // Should complete (not hang) and have errors
            Assert.IsTrue(result.HasErrors, "Should have errors for unterminated string");
            Assert.IsNotNull(result.Document, "Should return document");
            Assert.HasCount(1, result.Document.Properties, "Should parse the context property");

            // Verify the error is about unterminated string
            Assert.Contains("Unterminated", result.Errors[0].Message);
        }

        [TestMethod]
        [Timeout(5000, CooperativeCancellation = true)]
        public void TryParse_NestedObjectWithInvalidToken_ShouldNotHang()
        {
            var source = @"context:
  task: Our favorite hikes together
  location: Boulder
  season: spring_2025""";

            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success, "Should complete parsing");
            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.IsNotNull(result.Document);
        }
    }
}
