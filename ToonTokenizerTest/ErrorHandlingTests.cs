using ToonTokenizer;

namespace ToonTokenizerTest
{
    [TestClass]
    public class ErrorHandlingTests
    {
        [TestMethod]
        public void Parse_MissingColon_ReturnsErrorInResult()
        {
            var source = "name John";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.IsNotEmpty(result.Errors, "Errors collection should not be empty");
            Assert.Contains("':'", result.Errors[0].Message, "Error should mention missing colon");
            Assert.IsNotNull(result.Document, "Document should still be returned");
        }

        [TestMethod]
        public void Parse_UnmatchedBracket_ReturnsErrorInResult()
        {
            var source = "items[3: a,b,c";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.IsNotEmpty(result.Errors);
            Assert.Contains("']'", result.Errors[0].Message, "Error should mention missing bracket");
            Assert.IsNotNull(result.Document, "Document should still be returned");
        }

        [TestMethod]
        public void Parse_UnmatchedBrace_ReturnsErrorInResult()
        {
            var source = "schema{id,name: 1,test";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.IsNotEmpty(result.Errors);
            Assert.IsNotNull(result.Document, "Document should still be returned");
        }

        [TestMethod]
        public void TryParse_InvalidSyntax_ReturnsTrueWithErrors()
        {
            var source = "invalid without colon";
            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success, "TryParse should return true for completed parse");
            Assert.IsTrue(result.HasErrors, "Result should have errors");
            Assert.IsNotEmpty(result.Errors);
            Assert.Contains("Expected", result.Errors[0].Message);
            Assert.IsNotNull(result.Document, "Document should be returned even with errors");
        }

        [TestMethod]
        public void TryParse_ValidSyntax_ReturnsTrueWithNoErrors()
        {
            var source = "name: John";
            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success);
            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void TryParse_NullSource_ReturnsFalseWithError()
        {
            bool success = Toon.TryParse(null!, out var result);

            Assert.IsFalse(success);
            Assert.IsNotEmpty(result.Errors);
        }

        [TestMethod]
        public void Parse_NullSource_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Toon.Parse(null!));
        }

        [TestMethod]
        public void Tokenize_NullSource_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Toon.Tokenize(null!));
        }

        [TestMethod]
        public void Parse_ErrorMessage_ContainsLineNumber()
        {
            var source = @"name: John
invalid without colon";

            var result = Toon.Parse(source);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotEmpty(result.Errors);
            Assert.AreEqual(2, result.Errors[0].Line, "Error should be on line 2");
        }

        [TestMethod]
        public void Parse_ErrorMessage_ContainsColumnNumber()
        {
            var source = "name John";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors);
            Assert.IsGreaterThan(0, result.Errors[0].Column, "Error should have column number");
        }

        [TestMethod]
        public void Parse_TableArrayMissingComma_ReturnsErrorInResult()
        {
            var source = @"data[2]{id,name}:
  1 Alice
  2,Bob";

            var result = Toon.Parse(source);
            Assert.IsTrue(result.HasErrors, "Should have delimiter error");
            Assert.IsNotNull(result.Document, "Document should still be returned");
        }

        [TestMethod]
        public void Parse_InvalidSchemaFormat_ReturnsErrorInResult()
        {
            var source = "data[2]{id name}: 1,test";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have schema format error");
            Assert.IsNotNull(result.Document, "Document should still be returned");
        }

        [TestMethod]
        public void Parse_EmptySource_ReturnsEmptyDocument()
        {
            var source = "";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsEmpty(result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_WhitespaceOnly_ReturnsEmptyDocument()
        {
            var source = "   \n  \n  ";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsEmpty(result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_CommentsOnly_ReturnsEmptyDocument()
        {
            var source = @"# Comment 1
// Comment 2
# Comment 3";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsEmpty(result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_PropertyWithInlineComment_ParsesCorrectly()
        {
            var source = "name: John # This is John";
            var result = Toon.Parse(source);

            Assert.HasCount(1, result.Document!.Properties);
            Assert.AreEqual("name", result.Document!.Properties[0].Key);
        }

        [TestMethod]
        public void Parse_MultilineWithComments_ParsesCorrectly()
        {
            var source = @"# Header comment
name: John
# Middle comment
age: 30
// Another comment style";
            var result = Toon.Parse(source);

            Assert.HasCount(2, result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_UnclosedQuotedString_ReturnsError()
        {
            // Spec §7.1: Decoders MUST reject unterminated strings
            // With resilient parsing, lexer records error and continues
            var source = "name: \"John";

            var lexer = new ToonLexer(source);
            var tokens = lexer.Tokenize();

            Assert.IsNotEmpty(lexer.Errors, "Lexer should have errors");
            Assert.Contains("Unterminated string", lexer.Errors[0].Message);
        }

        [TestMethod]
        public void Lexer_InvalidCharacter_CreatesInvalidToken()
        {
            var lexer = new ToonLexer("@#$");
            var tokens = lexer.Tokenize();

            // Should still produce tokens, even if marked as invalid or treated as strings
            Assert.IsNotEmpty(tokens);
        }

        [TestMethod]
        public void Parse_VeryLongLine_ParsesCorrectly()
        {
            var longValue = new string('a', 10000);
            var source = $"key: {longValue}";
            var result = Toon.Parse(source);

            Assert.HasCount(1, result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_DeeplyNestedStructure_ParsesCorrectly()
        {
            var depth = 20;
            var indent = "";
            var lines = new List<string>();

            for (int i = 0; i < depth; i++)
            {
                lines.Add($"{indent}level{i}:");
                indent += "  ";
            }
            lines.Add($"{indent}value: deep");

            var source = string.Join("\n", lines);
            var result = Toon.Parse(source);

            Assert.HasCount(1, result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_ManyTopLevelProperties_ParsesCorrectly()
        {
            var lines = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                lines.Add($"prop{i}: value{i}");
            }

            var source = string.Join("\n", lines);
            var result = Toon.Parse(source);

            Assert.HasCount(100, result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_LargeArray_ParsesCorrectly()
        {
            var values = string.Join(",", Enumerable.Range(1, 100));
            var source = $"numbers[100]: {values}";
            var result = Toon.Parse(source);

            var array = (ToonTokenizer.Ast.ArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(100, array.Elements);
        }

        [TestMethod]
        public void Parse_LargeTableArray_ParsesCorrectly()
        {
            var lines = new List<string> { "data[50]{id,value}:" };
            for (int i = 1; i <= 50; i++)
            {
                lines.Add($"  {i},val{i}");
            }

            var source = string.Join("\n", lines);
            var result = Toon.Parse(source);

            var table = (ToonTokenizer.Ast.TableArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(50, table.Rows);
        }

        [TestMethod]
        public void Parse_UnicodeCharacters_ParsesCorrectly()
        {
            var source = "name: 日本語";
            var result = Toon.Parse(source);

            Assert.HasCount(1, result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_SpecialCharactersInQuotedString_ParsesCorrectly()
        {
            var source = "text: \"Hello\\nWorld\\t!\"";
            var result = Toon.Parse(source);

            var value = (ToonTokenizer.Ast.StringValueNode)result.Document!.Properties[0].Value;
            Assert.Contains("\n", value.Value);
            Assert.Contains("\t", value.Value);
        }

        [TestMethod]
        public void TryParse_ErrorIncludesSpanInformation()
        {
            var source = "name John";  // Missing colon
            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success, "TryParse should return true for completed parse");
            Assert.IsTrue(result.HasErrors);
            Assert.HasCount(1, result.Errors);

            var error = result.Errors[0];
            Assert.IsNotNull(error.Message);
            Assert.IsGreaterThanOrEqualTo(0, error.Position, "Position should be non-negative");
            Assert.IsGreaterThan(0, error.Length, "Length should be positive");
            Assert.IsGreaterThan(0, error.Line, "Line should be positive (1-based)");
            Assert.IsGreaterThan(0, error.Column, "Column should be positive (1-based)");
        }

        [TestMethod]
        public void TryParse_ErrorToString_IncludesAllInformation()
        {
            var source = "items[3: incomplete";  // Missing closing bracket
            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success, "TryParse should return true for completed parse");
            Assert.IsTrue(result.HasErrors);
            Assert.HasCount(2, result.Errors);

            var errorString = result.Errors[0].ToString();
            Assert.Contains("line", errorString.ToLower());
            Assert.Contains("column", errorString.ToLower());
            Assert.Contains("position", errorString.ToLower());
            Assert.Contains("length", errorString.ToLower());
        }

        [TestMethod]
        public void Parse_ErrorIncludesSpanInformation()
        {
            var source = "name John";  // Missing colon

            var result = Toon.Parse(source);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotEmpty(result.Errors);

            var error = result.Errors[0];
            Assert.IsGreaterThanOrEqualTo(0, error.Position, "Position should be non-negative");
            Assert.IsGreaterThan(0, error.Length, "Length should be positive");
            Assert.IsGreaterThan(0, error.Line, "Line should be positive (1-based)");
            Assert.IsGreaterThan(0, error.Column, "Column should be positive (1-based)");
        }

        [TestMethod]
        public void TryParse_MultilineError_CorrectLineNumber()
        {
            var source = @"name: John
age: 30
invalid line";  // Missing colon on line 3

            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success, "TryParse should return true even with parse errors");
            Assert.IsTrue(result.HasErrors);
            Assert.HasCount(1, result.Errors);
            Assert.AreEqual(3, result.Errors[0].Line, "Error should be on line 3");
        }

        [TestMethod]
        public void ToonError_EndPosition_CalculatesCorrectly()
        {
            var error = new ToonError("Test error", 10, 5, 1, 11);
            Assert.AreEqual(15, error.EndPosition, "EndPosition should be Position + Length");
        }

        [TestMethod]
        public void Parse_IncludesTokensInResult()
        {
            var source = "name: John";
            var result = Toon.Parse(source);

            Assert.IsNotNull(result.Tokens, "Tokens should not be null");
            Assert.IsNotEmpty(result.Tokens, "Tokens should not be empty");

            // Verify we have the expected tokens: Identifier, Colon, Identifier, EOF
            Assert.IsGreaterThanOrEqualTo(3, result.Tokens.Count, "Should have at least 3 tokens");
            Assert.AreEqual(TokenType.Identifier, result.Tokens[0].Type);
            Assert.AreEqual("name", result.Tokens[0].Value);
        }

        [TestMethod]
        public void Parse_WithErrors_IncludesTokensInResult()
        {
            var source = "name John";  // Missing colon
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.Tokens, "Tokens should not be null even with errors");
            Assert.IsNotEmpty(result.Tokens, "Tokens should not be empty even with errors");
        }

        [TestMethod]
        public void TryParse_IncludesTokensInResult()
        {
            var source = "age: 30";
            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success);
            Assert.IsNotNull(result.Tokens, "Tokens should not be null");
            Assert.IsNotEmpty(result.Tokens, "Tokens should not be empty");
        }

        [TestMethod]
        public void Parse_TokensMatchSourceStructure()
        {
            var source = @"name: John
age: 30";
            var result = Toon.Parse(source);

            Assert.IsNotNull(result.Tokens);

            // Should have tokens for both properties
            var identifiers = result.Tokens.Where(t => t.Type == TokenType.Identifier).ToList();
            Assert.IsGreaterThanOrEqualTo(2, identifiers.Count, "Should have at least 2 identifiers (property keys)");

            var colons = result.Tokens.Where(t => t.Type == TokenType.Colon).ToList();
            Assert.HasCount(2, colons, "Should have 2 colons");

            var numbers = result.Tokens.Where(t => t.Type == TokenType.Number).ToList();
            Assert.HasCount(1, numbers, "Should have 1 number token (30)");
        }
    }
}
