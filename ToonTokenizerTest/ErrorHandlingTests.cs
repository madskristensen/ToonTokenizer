using ToonTokenizer;

namespace ToonTokenizerTest
{
    [TestClass]
    public class ErrorHandlingTests
    {
        [TestMethod]
        public void Parse_MissingColon_ThrowsParseException()
        {
            var source = "name John";
            Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
        }

        [TestMethod]
        public void Parse_UnmatchedBracket_ThrowsParseException()
        {
            var source = "items[3: a,b,c";
            Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
        }

        [TestMethod]
        public void Parse_UnmatchedBrace_ThrowsParseException()
        {
            var source = "schema{id,name: 1,test";
            Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
        }

        [TestMethod]
        public void TryParse_InvalidSyntax_ReturnsFalseWithError()
        {
            var source = "invalid without colon";
            bool result = Toon.TryParse(source, out var errors);

            Assert.IsFalse(result);
            Assert.IsNotEmpty(errors);
            Assert.Contains("Expected", errors[0]);
        }

        [TestMethod]
        public void TryParse_ValidSyntax_ReturnsTrueWithNoErrors()
        {
            var source = "name: John";
            bool result = Toon.TryParse(source, out var errors);

            Assert.IsTrue(result);
            Assert.IsEmpty(errors);
        }

        [TestMethod]
        public void TryParse_NullSource_ReturnsFalseWithError()
        {
            bool result = Toon.TryParse(null!, out var errors);

            Assert.IsFalse(result);
            Assert.IsNotEmpty(errors);
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

            var ex = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("line", ex.Message);
        }

        [TestMethod]
        public void Parse_ErrorMessage_ContainsColumnNumber()
        {
            var source = "name John";
            var ex = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.IsTrue(ex.Message.Contains("column") || ex.Message.Contains("col"));
        }

        [TestMethod]
        public void Parse_TableArrayMissingComma_ThrowsParseException()
        {
            var source = @"data[2]{id,name}:
  1 Alice
  2,Bob";

            try
            {
                Toon.Parse(source);
                Assert.Fail("Expected ParseException to be thrown");
            }
            catch (ParseException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void Parse_InvalidSchemaFormat_ThrowsParseException()
        {
            var source = "data[2]{id name}: 1,test";
            Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
        }

        [TestMethod]
        public void Parse_EmptySource_ReturnsEmptyDocument()
        {
            var source = "";
            var document = Toon.Parse(source);

            Assert.IsNotNull(document);
            Assert.IsEmpty(document.Properties);
        }

        [TestMethod]
        public void Parse_WhitespaceOnly_ReturnsEmptyDocument()
        {
            var source = "   \n  \n  ";
            var document = Toon.Parse(source);

            Assert.IsNotNull(document);
            Assert.IsEmpty(document.Properties);
        }

        [TestMethod]
        public void Parse_CommentsOnly_ReturnsEmptyDocument()
        {
            var source = @"# Comment 1
// Comment 2
# Comment 3";
            var document = Toon.Parse(source);

            Assert.IsNotNull(document);
            Assert.IsEmpty(document.Properties);
        }

        [TestMethod]
        public void Parse_PropertyWithInlineComment_ParsesCorrectly()
        {
            var source = "name: John # This is John";
            var document = Toon.Parse(source);

            Assert.HasCount(1, document.Properties);
            Assert.AreEqual("name", document.Properties[0].Key);
        }

        [TestMethod]
        public void Parse_MultilineWithComments_ParsesCorrectly()
        {
            var source = @"# Header comment
name: John
# Middle comment
age: 30
// Another comment style";
            var document = Toon.Parse(source);

            Assert.HasCount(2, document.Properties);
        }

        [TestMethod]
        public void Parse_UnclosedQuotedString_HandlesGracefully()
        {
            var source = "name: \"John";

            // The lexer should handle this by consuming until end of line
            var tokens = Toon.Tokenize(source);
            Assert.IsNotEmpty(tokens);
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
            var document = Toon.Parse(source);

            Assert.HasCount(1, document.Properties);
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
            var document = Toon.Parse(source);

            Assert.HasCount(1, document.Properties);
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
            var document = Toon.Parse(source);

            Assert.HasCount(100, document.Properties);
        }

        [TestMethod]
        public void Parse_LargeArray_ParsesCorrectly()
        {
            var values = string.Join(",", Enumerable.Range(1, 100));
            var source = $"numbers[100]: {values}";
            var document = Toon.Parse(source);

            var array = (ToonTokenizer.Ast.ArrayNode)document.Properties[0].Value;
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
            var document = Toon.Parse(source);

            var table = (ToonTokenizer.Ast.TableArrayNode)document.Properties[0].Value;
            Assert.HasCount(50, table.Rows);
        }

        [TestMethod]
        public void Parse_UnicodeCharacters_ParsesCorrectly()
        {
            var source = "name: 日本語";
            var document = Toon.Parse(source);

            Assert.HasCount(1, document.Properties);
        }

        [TestMethod]
        public void Parse_SpecialCharactersInQuotedString_ParsesCorrectly()
        {
            var source = "text: \"Hello\\nWorld\\t!\"";
            var document = Toon.Parse(source);

            var value = (ToonTokenizer.Ast.StringValueNode)document.Properties[0].Value;
            Assert.Contains("\n", value.Value);
            Assert.Contains("\t", value.Value);
        }
    }
}
