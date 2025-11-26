using ToonTokenizer;

namespace ToonTokenizerTest.Lexer
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void Tokenize_EmptyString_ReturnsOnlyEofToken()
        {
            var lexer = new ToonLexer("");
            var tokens = lexer.Tokenize();

            Assert.HasCount(1, tokens);
            Assert.AreEqual(TokenType.EndOfFile, tokens[0].Type);
        }

        [TestMethod]
        public void Tokenize_SimpleIdentifier_ReturnsIdentifierToken()
        {
            var lexer = new ToonLexer("name");
            var tokens = lexer.Tokenize();

            Assert.HasCount(2, tokens); // Identifier or String + EOF
            Assert.IsTrue(tokens[0].Type == TokenType.Identifier || tokens[0].Type == TokenType.String);
            Assert.AreEqual("name", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_Colon_ReturnsColonToken()
        {
            var lexer = new ToonLexer(":");
            var tokens = lexer.Tokenize();

            Assert.HasCount(2, tokens);
            Assert.AreEqual(TokenType.Colon, tokens[0].Type);
            Assert.AreEqual(":", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_KeyValuePair_ReturnsCorrectTokens()
        {
            var lexer = new ToonLexer("name: John");
            var tokens = lexer.Tokenize();

            var nonWhitespace = tokens.Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.EndOfFile).ToList();

            Assert.HasCount(3, nonWhitespace);
            Assert.AreEqual(TokenType.Identifier, nonWhitespace[0].Type);
            Assert.AreEqual("name", nonWhitespace[0].Value);
            Assert.AreEqual(TokenType.Colon, nonWhitespace[1].Type);
            Assert.AreEqual(TokenType.String, nonWhitespace[2].Type);
            Assert.AreEqual("John", nonWhitespace[2].Value);
        }

        [TestMethod]
        public void Tokenize_QuotedString_ReturnsStringToken()
        {
            var lexer = new ToonLexer("\"Hello World\"");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.String, tokens[0].Type);
            Assert.AreEqual("Hello World", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_SingleQuotedString_ReturnsStringToken()
        {
            var lexer = new ToonLexer("'Hello World'");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.String, tokens[0].Type);
            Assert.AreEqual("Hello World", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_StringWithEscapes_HandlesEscapeSequences()
        {
            var lexer = new ToonLexer("\"Line1\\nLine2\\tTabbed\"");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.String, tokens[0].Type);
            Assert.AreEqual("Line1\nLine2\tTabbed", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_Integer_ReturnsNumberToken()
        {
            var lexer = new ToonLexer("42");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.Number, tokens[0].Type);
            Assert.AreEqual("42", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_NegativeInteger_ReturnsNumberToken()
        {
            var lexer = new ToonLexer("-42");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.Number, tokens[0].Type);
            Assert.AreEqual("-42", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_Float_ReturnsNumberToken()
        {
            var lexer = new ToonLexer("3.14");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.Number, tokens[0].Type);
            Assert.AreEqual("3.14", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_ScientificNotation_ReturnsNumberToken()
        {
            var lexer = new ToonLexer("1.5e-10");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.Number, tokens[0].Type);
            Assert.AreEqual("1.5e-10", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_TrueKeyword_ReturnsTrueToken()
        {
            var lexer = new ToonLexer("true");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.True, tokens[0].Type);
            Assert.AreEqual("true", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_FalseKeyword_ReturnsFalseToken()
        {
            var lexer = new ToonLexer("false");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.False, tokens[0].Type);
            Assert.AreEqual("false", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_NullKeyword_ReturnsNullToken()
        {
            var lexer = new ToonLexer("null");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.Null, tokens[0].Type);
            Assert.AreEqual("null", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_LeftBracket_ReturnsLeftBracketToken()
        {
            var lexer = new ToonLexer("[");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.LeftBracket, tokens[0].Type);
        }

        [TestMethod]
        public void Tokenize_RightBracket_ReturnsRightBracketToken()
        {
            var lexer = new ToonLexer("]");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.RightBracket, tokens[0].Type);
        }

        [TestMethod]
        public void Tokenize_LeftBrace_ReturnsLeftBraceToken()
        {
            var lexer = new ToonLexer("{");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.LeftBrace, tokens[0].Type);
        }

        [TestMethod]
        public void Tokenize_RightBrace_ReturnsRightBraceToken()
        {
            var lexer = new ToonLexer("}");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.RightBrace, tokens[0].Type);
        }

        [TestMethod]
        public void Tokenize_Comma_ReturnsCommaToken()
        {
            var lexer = new ToonLexer(",");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.Comma, tokens[0].Type);
        }

        [TestMethod]
        public void Tokenize_HashComment_ReturnsCommentToken()
        {
            var lexer = new ToonLexer("# This is a comment");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.Comment, tokens[0].Type);
            Assert.StartsWith("#", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_SlashSlashComment_ReturnsCommentToken()
        {
            var lexer = new ToonLexer("// This is a comment");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(TokenType.Comment, tokens[0].Type);
            Assert.StartsWith("//", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_NewLine_ReturnsNewlineToken()
        {
            var lexer = new ToonLexer("line1\nline2");
            var tokens = lexer.Tokenize();

            var newlineToken = tokens.FirstOrDefault(t => t.Type == TokenType.Newline);
            Assert.IsNotNull(newlineToken);
        }

        [TestMethod]
        public void Tokenize_ArrayNotation_ReturnsCorrectTokens()
        {
            var lexer = new ToonLexer("items[3]");
            var tokens = lexer.Tokenize();

            var nonWhitespace = tokens.Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.EndOfFile).ToList();

            Assert.HasCount(4, nonWhitespace);
            Assert.AreEqual(TokenType.Identifier, nonWhitespace[0].Type);
            Assert.AreEqual("items", nonWhitespace[0].Value);
            Assert.AreEqual(TokenType.LeftBracket, nonWhitespace[1].Type);
            Assert.AreEqual(TokenType.Number, nonWhitespace[2].Type);
            Assert.AreEqual("3", nonWhitespace[2].Value);
            Assert.AreEqual(TokenType.RightBracket, nonWhitespace[3].Type);
        }

        [TestMethod]
        public void Tokenize_SchemaNotation_ReturnsCorrectTokens()
        {
            var lexer = new ToonLexer("{id,name,age}");
            var tokens = lexer.Tokenize();

            var nonWhitespace = tokens.Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.EndOfFile).ToList();

            Assert.HasCount(7, nonWhitespace);
            Assert.AreEqual(TokenType.LeftBrace, nonWhitespace[0].Type);
            Assert.IsTrue(nonWhitespace[1].Type == TokenType.Identifier || nonWhitespace[1].Type == TokenType.String);
            Assert.AreEqual("id", nonWhitespace[1].Value);
            Assert.AreEqual(TokenType.Comma, nonWhitespace[2].Type);
            Assert.IsTrue(nonWhitespace[3].Type == TokenType.Identifier || nonWhitespace[3].Type == TokenType.String);
            Assert.AreEqual("name", nonWhitespace[3].Value);
            Assert.AreEqual(TokenType.Comma, nonWhitespace[4].Type);
            Assert.IsTrue(nonWhitespace[5].Type == TokenType.Identifier || nonWhitespace[5].Type == TokenType.String);
            Assert.AreEqual("age", nonWhitespace[5].Value);
            Assert.AreEqual(TokenType.RightBrace, nonWhitespace[6].Type);
        }

        [TestMethod]
        public void Tokenize_UnquotedString_ReturnsStringToken()
        {
            var lexer = new ToonLexer("name: John_Doe");
            var tokens = lexer.Tokenize();

            var lastValue = tokens.Where(t => t.Type == TokenType.String || t.Type == TokenType.Identifier).Last();
            Assert.IsTrue(lastValue.Value.Contains("John") || lastValue.Value.Contains("Doe"));
        }

        [TestMethod]
        public void Tokenize_CommaSeparatedValues_ReturnsCorrectTokens()
        {
            var lexer = new ToonLexer("red,green,blue");
            var tokens = lexer.Tokenize();

            var nonWhitespace = tokens.Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.EndOfFile).ToList();

            Assert.HasCount(5, nonWhitespace);
            Assert.AreEqual(TokenType.String, nonWhitespace[0].Type);
            Assert.AreEqual("red", nonWhitespace[0].Value);
            Assert.AreEqual(TokenType.Comma, nonWhitespace[1].Type);
            Assert.AreEqual(TokenType.String, nonWhitespace[2].Type);
            Assert.AreEqual("green", nonWhitespace[2].Value);
            Assert.AreEqual(TokenType.Comma, nonWhitespace[3].Type);
            Assert.AreEqual(TokenType.String, nonWhitespace[4].Type);
            Assert.AreEqual("blue", nonWhitespace[4].Value);
        }

        [TestMethod]
        public void Token_HasCorrectLineAndColumnInformation()
        {
            var lexer = new ToonLexer("name: John");
            var tokens = lexer.Tokenize();

            Assert.AreEqual(1, tokens[0].Line);
            Assert.AreEqual(1, tokens[0].Column);
        }

        [TestMethod]
        public void Token_TracksPositionAcrossLines()
        {
            var source = "line1\nline2";
            var lexer = new ToonLexer(source);
            var tokens = lexer.Tokenize();

            var line2Token = tokens.FirstOrDefault(t => t.Line == 2);
            Assert.IsNotNull(line2Token);
            Assert.AreEqual(2, line2Token.Line);
        }
    }
}
