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
            List<Token> tokens = lexer.Tokenize();

            Assert.HasCount(1, tokens);
            Assert.AreEqual(TokenType.EndOfFile, tokens[0].Type);
        }

        [TestMethod]
        public void Tokenize_SimpleIdentifier_ReturnsIdentifierToken()
        {
            List<Token> tokens = ToonLexerTestHelpers.TokenizeNow("name");
            Assert.HasCount(1, tokens);
            Assert.IsTrue(tokens[0].Type == TokenType.Identifier || tokens[0].Type == TokenType.String);
            Assert.AreEqual("name", tokens[0].Value);
        }

        [TestMethod]
        public void Tokenize_Colon_ReturnsColonToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken(":", TokenType.Colon);
            Assert.AreEqual(":", token.Value);
        }

        [TestMethod]
        public void Tokenize_KeyValuePair_ReturnsCorrectTokens()
        {
            List<Token> nonWhitespace = ToonLexerTestHelpers.TokenizeNow("name: John");
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
            Token token = ToonLexerTestHelpers.AssertSingleToken("\"Hello World\"", TokenType.String);
            Assert.AreEqual("Hello World", token.Value);
        }

        [TestMethod]
        public void Tokenize_SingleQuotedString_ReturnsStringToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("'Hello World'", TokenType.String);
            Assert.AreEqual("Hello World", token.Value);
        }

        [TestMethod]
        public void Tokenize_StringWithEscapes_HandlesEscapeSequences()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("\"Line1\\nLine2\\tTabbed\"", TokenType.String);
            Assert.AreEqual("Line1\nLine2\tTabbed", token.Value);
        }

        [TestMethod]
        public void Tokenize_Integer_ReturnsNumberToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("42", TokenType.Number);
            Assert.AreEqual("42", token.Value);
        }

        [TestMethod]
        public void Tokenize_NegativeInteger_ReturnsNumberToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("-42", TokenType.Number);
            Assert.AreEqual("-42", token.Value);
        }

        [TestMethod]
        public void Tokenize_Float_ReturnsNumberToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("3.14", TokenType.Number);
            Assert.AreEqual("3.14", token.Value);
        }

        [TestMethod]
        public void Tokenize_ScientificNotation_ReturnsNumberToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("1.5e-10", TokenType.Number);
            Assert.AreEqual("1.5e-10", token.Value);
        }

        [TestMethod]
        public void Tokenize_TrueKeyword_ReturnsTrueToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("true", TokenType.True);
            Assert.AreEqual("true", token.Value);
        }

        [TestMethod]
        public void Tokenize_FalseKeyword_ReturnsFalseToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("false", TokenType.False);
            Assert.AreEqual("false", token.Value);
        }

        [TestMethod]
        public void Tokenize_NullKeyword_ReturnsNullToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("null", TokenType.Null);
            Assert.AreEqual("null", token.Value);
        }

        [TestMethod]
        public void Tokenize_LeftBracket_ReturnsLeftBracketToken()
        {
            ToonLexerTestHelpers.AssertSingleToken("[", TokenType.LeftBracket);
        }

        [TestMethod]
        public void Tokenize_RightBracket_ReturnsRightBracketToken()
        {
            ToonLexerTestHelpers.AssertSingleToken("]", TokenType.RightBracket);
        }

        [TestMethod]
        public void Tokenize_Comma_ReturnsCommaToken()
        {
            ToonLexerTestHelpers.AssertSingleToken(",", TokenType.Comma);
        }

        [TestMethod]
        public void Tokenize_HashComment_ReturnsCommentToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("# This is a comment", TokenType.Comment);
            Assert.StartsWith("#", token.Value);
        }

        [TestMethod]
        public void Tokenize_SlashSlashComment_ReturnsCommentToken()
        {
            Token token = ToonLexerTestHelpers.AssertSingleToken("// This is a comment", TokenType.Comment);
            Assert.StartsWith("//", token.Value);
        }

        [TestMethod]
        public void Tokenize_NewLine_ReturnsNewlineToken()
        {
            List<Token> tokens = new ToonLexer("line1\nline2").Tokenize();
            Token? newlineToken = tokens.FirstOrDefault(t => t.Type == TokenType.Newline);
            Assert.IsNotNull(newlineToken);
        }

        [TestMethod]
        public void Tokenize_ArrayNotation_ReturnsCorrectTokens()
        {
            List<Token> nonWhitespace = ToonLexerTestHelpers.AssertTokenTypes("items[3]",
                TokenType.Identifier, TokenType.LeftBracket, TokenType.Number, TokenType.RightBracket);

            Assert.AreEqual("items", nonWhitespace[0].Value);
            Assert.AreEqual("3", nonWhitespace[2].Value);
        }

        [TestMethod]
        public void Tokenize_SchemaNotation_ReturnsCorrectTokens()
        {
            List<Token> tokens = ToonLexerTestHelpers.TokenizeNow("{id,name,age}");
            Assert.HasCount(7, tokens);
            Assert.AreEqual(TokenType.LeftBrace, tokens[0].Type);
            Assert.IsTrue(tokens[1].Type == TokenType.Identifier || tokens[1].Type == TokenType.String);
            Assert.AreEqual("id", tokens[1].Value);
            Assert.AreEqual(TokenType.Comma, tokens[2].Type);
            Assert.IsTrue(tokens[3].Type == TokenType.Identifier || tokens[3].Type == TokenType.String);
            Assert.AreEqual("name", tokens[3].Value);
            Assert.AreEqual(TokenType.Comma, tokens[4].Type);
            Assert.IsTrue(tokens[5].Type == TokenType.Identifier || tokens[5].Type == TokenType.String);
            Assert.AreEqual("age", tokens[5].Value);
            Assert.AreEqual(TokenType.RightBrace, tokens[6].Type);
        }

        [TestMethod]
        public void Tokenize_UnquotedString_ReturnsStringToken()
        {
            List<Token> tokens = ToonLexerTestHelpers.TokenizeNow("name: John_Doe");
            Token lastValue = tokens.Where(t => t.Type == TokenType.String || t.Type == TokenType.Identifier).Last();
            Assert.IsTrue(lastValue.Value.Contains("John") || lastValue.Value.Contains("Doe"));
        }

        [TestMethod]
        public void Tokenize_CommaSeparatedValues_ReturnsCorrectTokens()
        {
            List<Token> nonWhitespace = ToonLexerTestHelpers.AssertTokenTypes("red,green,blue",
                TokenType.String, TokenType.Comma, TokenType.String, TokenType.Comma, TokenType.String);

            Assert.AreEqual("red", nonWhitespace[0].Value);
            Assert.AreEqual("green", nonWhitespace[2].Value);
            Assert.AreEqual("blue", nonWhitespace[4].Value);
        }

        [TestMethod]
        public void Token_HasCorrectLineAndColumnInformation()
        {
            List<Token> tokens = new ToonLexer("name: John").Tokenize();
            Assert.AreEqual(1, tokens[0].Line);
            Assert.AreEqual(1, tokens[0].Column);
        }

        [TestMethod]
        public void Token_TracksPositionAcrossLines()
        {
            var source = "line1\nline2";
            List<Token> tokens = new ToonLexer(source).Tokenize();

            Token? line2Token = tokens.FirstOrDefault(t => t.Line == 2);
            Assert.IsNotNull(line2Token);
            Assert.AreEqual(2, line2Token.Line);
        }
    }
}

