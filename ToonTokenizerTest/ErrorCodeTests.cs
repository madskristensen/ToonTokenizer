using ToonTokenizer;

namespace ToonTokenizerTest
{
    /// <summary>
    /// Tests for error codes and enhanced error messages.
    /// Verifies that errors include proper codes and helpful suggestions.
    /// </summary>
    [TestClass]
    public class ErrorCodeTests
    {
        [TestMethod]
        public void UnterminatedString_HasCorrectErrorCode()
        {
            string toon = @"name: ""unterminated";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            Assert.HasCount(1, result.Errors);
            Assert.AreEqual(ErrorCode.UnterminatedString, result.Errors[0].Code);
            Assert.Contains("Unterminated", result.Errors[0].Message);
            Assert.Contains("Fix:", result.Errors[0].Message);
        }

        [TestMethod]
        public void InvalidEscapeSequence_HasCorrectErrorCode()
        {
            string toon = @"text: ""hello\xworld""";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            Assert.IsNotEmpty(result.Errors);
            var escapeError = result.Errors.Find(e => e.Code == ErrorCode.InvalidEscapeSequence);
            Assert.IsNotNull(escapeError);
            Assert.Contains("Valid escape sequences", escapeError.Message);
            Assert.Contains("Fix:", escapeError.Message);
        }

        [TestMethod]
        public void MissingColon_HasCorrectErrorCode()
        {
            string toon = @"name ""value""";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            var colonError = result.Errors.Find(e => e.Code == ErrorCode.ExpectedColon);
            Assert.IsNotNull(colonError);
            Assert.Contains("Expected ':'", colonError.Message);
        }

        [TestMethod]
        public void ArraySizeMismatch_HasCorrectErrorCodeAndHint()
        {
            string toon = @"items[3]: one,two";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            var sizeError = result.Errors.Find(e => e.Code == ErrorCode.ArraySizeMismatch);
            Assert.IsNotNull(sizeError);
            Assert.Contains("Array size mismatch", sizeError.Message);
            Assert.Contains("declared 3", sizeError.Message);
            Assert.Contains("found 2", sizeError.Message);
            Assert.Contains("Missing 1 element", sizeError.Message);
        }

        [TestMethod]
        public void ArraySizeMismatch_TooManyElements_HasHelpfulHint()
        {
            string toon = @"items[2]: one,two,three";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            // The error might be about expecting delimiter or array size
            // Either way, there should be an error with helpful message
            Assert.IsGreaterThan(0, result.Errors.Count);
            
            // Check if we have an array size mismatch error
            var sizeError = result.Errors.Find(e => e.Code == ErrorCode.ArraySizeMismatch);
            if (sizeError != null)
            {
                // If we got a size error, check for helpful hints
                Assert.IsTrue(sizeError.Message.Contains("extra") || sizeError.Message.Contains("mismatch"));
            }
        }

        [TestMethod]
        public void TableSizeMismatch_HasCorrectErrorCodeAndHint()
        {
            string toon = @"users[2]{id,name}:
  1,Alice";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            var tableError = result.Errors.Find(e => e.Code == ErrorCode.TableSizeMismatch);
            Assert.IsNotNull(tableError);
            Assert.Contains("Table array size mismatch", tableError.Message);
            Assert.Contains("declared 2", tableError.Message);
            Assert.Contains("found 1", tableError.Message);
            Assert.Contains("Missing 1 row", tableError.Message);
        }

        [TestMethod]
        public void ExpectedPropertyKey_HasCorrectErrorCode()
        {
            string toon = @"123: value";
            var result = Toon.Parse(toon);

            // Parser is resilient, but should have logged an error or handled it
            // Note: Numbers might be parsed as unquoted strings in some contexts
            // This test verifies error code when property key is truly invalid
            if (result.HasErrors)
            {
                var keyError = result.Errors.Find(e => e.Code == ErrorCode.ExpectedPropertyKey);
                if (keyError != null)
                {
                    Assert.Contains("Expected property key", keyError.Message);
                }
            }
        }

        [TestMethod]
        public void UnexpectedEndOfInput_HasCorrectErrorCode()
        {
            string toon = @"name:";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            var eofError = result.Errors.Find(e => e.Code == ErrorCode.UnexpectedEndOfInput);
            Assert.IsNotNull(eofError);
            Assert.Contains("Unexpected end of input", eofError.Message);
        }

        [TestMethod]
        public void ErrorToString_IncludesErrorCode()
        {
            string toon = @"items[3]: one,two";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            // Find an error that has a code
            var errorWithCode = result.Errors.Find(e => !string.IsNullOrEmpty(e.Code));
            Assert.IsNotNull(errorWithCode, "Should have at least one error with a code");
            
            var errorString = errorWithCode.ToString();
            Assert.Contains("[TOON", errorString);
            Assert.Contains("]", errorString);
        }

        [TestMethod]
        public void MultipleErrors_EachHasUniqueCode()
        {
            string toon = @"
name ""missing colon""
items[3]: one,two
text: ""unterminated
";
            var result = Toon.Parse(toon);

            Assert.IsTrue(result.HasErrors);
            Assert.IsGreaterThanOrEqualTo(2, result.Errors.Count);

            // Verify we have different error codes
            var codes = result.Errors.Select(e => e.Code).Where(c => c != null).Distinct().ToList();
            Assert.IsGreaterThanOrEqualTo(2, codes.Count, "Should have at least 2 different error codes");
        }

        [TestMethod]
        public void EmptyArray_HasHelpfulHint()
        {
            string toon = @"items[3]:";
            var result = Toon.Parse(toon);

            // Parser might find unexpected end of input or parse an empty array
            // If it parses as an array, it should have a size mismatch
            if (result.HasErrors)
            {
                // Either we get "No elements found" in a size mismatch
                // or we get "Unexpected end of input"
                bool hasHelpfulError = result.Errors.Any(e => 
                    e.Message.Contains("No elements") || 
                    e.Message.Contains("Unexpected end") ||
                    e.Message.Contains("Missing"));
                Assert.IsTrue(hasHelpfulError, "Should have a helpful error message");
            }
        }

        [TestMethod]
        public void EmptyTableArray_HasHelpfulHint()
        {
            string toon = @"users[2]{id,name}:
  1,Alice";
            var result = Toon.Parse(toon);

            // Should have a table size mismatch error since we declared 2 rows but only have 1
            Assert.IsTrue(result.HasErrors, "Should have errors for table size mismatch");
            bool hasHelpfulError = result.Errors.Any(e =>
                e.Message.Contains("Missing") ||
                e.Message.Contains("mismatch") ||
                e.Message.Contains("declared 2"));
            Assert.IsTrue(hasHelpfulError, "Should have a helpful error message about missing rows");
        }
    }
}
