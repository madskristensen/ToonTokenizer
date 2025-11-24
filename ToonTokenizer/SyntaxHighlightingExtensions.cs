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
        public static Token GetTokenAt(this List<Token> tokens, int line, int column)
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

        /// <summary>
        /// Gets a classification for syntax highlighting purposes.
        /// </summary>
        public static string GetClassification(this Token token)
        {
            switch (token.Type)
            {
                case TokenType.String:
                    return "string";
                case TokenType.Number:
                    return "number";
                case TokenType.True:
                case TokenType.False:
                    return "keyword.boolean";
                case TokenType.Null:
                    return "keyword.null";
                case TokenType.Identifier:
                    return "identifier";
                case TokenType.Colon:
                case TokenType.Comma:
                case TokenType.LeftBracket:
                case TokenType.RightBracket:
                case TokenType.LeftBrace:
                case TokenType.RightBrace:
                    return "punctuation";
                case TokenType.Comment:
                    return "comment";
                case TokenType.Whitespace:
                case TokenType.Newline:
                case TokenType.Indent:
                case TokenType.Dedent:
                    return "whitespace";
                default:
                    return "other";
            }
        }
    }
}
