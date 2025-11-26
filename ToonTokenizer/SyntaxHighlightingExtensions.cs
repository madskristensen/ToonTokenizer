using System.Collections.Generic;
using System.Linq;

namespace ToonTokenizer
{
    /// <summary>
    /// Provides utility methods for syntax highlighting support.
    /// </summary>
    public static class SyntaxHighlightingExtensions
    {
        /// <summary>
        /// Gets all tokens in a specific line.
        /// </summary>
        public static List<Token> GetTokensOnLine(this List<Token> tokens, int line)
        {
            return [.. tokens.Where(t => t.Line == line)];
        }

        /// <summary>
        /// Gets all tokens in a range of lines.
        /// </summary>
        public static List<Token> GetTokensInRange(this List<Token> tokens, int startLine, int endLine)
        {
            return [.. tokens.Where(t => t.Line >= startLine && t.Line <= endLine)];
        }

        /// <summary>
        /// Gets the token at a specific position (line and column).
        /// </summary>
        /// <param name="tokens">The list of tokens to search.</param>
        /// <param name="line">The line number (1-based).</param>
        /// <param name="column">The column number (1-based).</param>
        /// <returns>The token at that position, or null if no token found.</returns>
        /// <example>
        /// <code>
        /// var result = Toon.Parse(source);
        /// var token = result.Tokens.GetTokenAt(line: 5, column: 10);
        /// if (token != null) {
        ///     Console.WriteLine($"Token at 5:10 is {token.Type}: {token.Value}");
        /// }
        /// </code>
        /// </example>
        public static Token? GetTokenAt(this List<Token> tokens, int line, int column)
        {
            return tokens.FirstOrDefault(t =>
                t.Line == line &&
                column >= t.Column &&
                column < t.Column + t.Length);
        }

        /// <summary>
        /// Gets all tokens of a specific type.
        /// </summary>
        public static List<Token> GetTokensByType(this List<Token> tokens, TokenType type)
        {
            return [.. tokens.Where(t => t.Type == type)];
        }

        /// <summary>
        /// Determines if a token represents a keyword.
        /// </summary>
        public static bool IsKeyword(this Token token)
        {
            return token.Type == TokenType.True ||
                   token.Type == TokenType.False ||
                   token.Type == TokenType.Null;
        }

        /// <summary>
        /// Determines if a token represents a structural element.
        /// </summary>
        public static bool IsStructural(this Token token)
        {
            return token.Type == TokenType.Colon ||
                   token.Type == TokenType.Comma ||
                   token.Type == TokenType.LeftBracket ||
                   token.Type == TokenType.RightBracket ||
                   token.Type == TokenType.LeftBrace ||
                   token.Type == TokenType.RightBrace;
        }

        /// <summary>
        /// Determines if a token represents a value (literal).
        /// </summary>
        public static bool IsValue(this Token token)
        {
            return token.Type == TokenType.String ||
                   token.Type == TokenType.Number ||
                   token.Type == TokenType.True ||
                   token.Type == TokenType.False ||
                   token.Type == TokenType.Null;
        }
    }
}
