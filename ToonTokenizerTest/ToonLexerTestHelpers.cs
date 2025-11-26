using ToonTokenizer;

namespace ToonTokenizerTest
{
    /// <summary>
    /// Helper methods for lexer-focused tests.
    /// </summary>
    public static class ToonLexerTestHelpers
    {
        /// <summary>
        /// Tokenizes source and returns non-whitespace, non-EOF tokens.
        /// </summary>
        public static List<Token> TokenizeNow(string source)
        {
            var lexer = new ToonLexer(source);
            return [.. lexer.Tokenize().Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.EndOfFile)];
        }

        /// <summary>
        /// Asserts a single token of given type and returns it.
        /// </summary>
        public static Token AssertSingleToken(string source, TokenType expectedType)
        {
            List<Token> tokens = TokenizeNow(source);
            Assert.HasCount(1, tokens, $"Expected 1 token but got {tokens.Count}");
            Assert.AreEqual(expectedType, tokens[0].Type);
            return tokens[0];
        }

        /// <summary>
        /// Asserts token sequence types in order and returns filtered tokens.
        /// </summary>
        public static List<Token> AssertTokenTypes(string source, params TokenType[] expectedTypes)
        {
            List<Token> tokens = TokenizeNow(source);
            Assert.HasCount(expectedTypes.Length, tokens,
                $"Token count mismatch: expected {expectedTypes.Length}, got {tokens.Count}");
            for (int i = 0; i < expectedTypes.Length; i++)
            {
                Assert.AreEqual(expectedTypes[i], tokens[i].Type, $"Token {i} type mismatch");
            }
            return tokens;
        }
    }
}
