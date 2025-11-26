namespace ToonTokenizer
{
    /// <summary>
    /// Extension methods for TokenType categorization.
    /// </summary>
    public static class TokenTypeExtensions
    {
        /// <summary>
        /// Checks if the token type represents a value token (string, number, boolean, null, identifier).
        /// </summary>
        public static bool IsValueToken(this TokenType type) =>
            type == TokenType.String ||
            type == TokenType.Identifier ||
            type == TokenType.Number ||
            type == TokenType.True ||
            type == TokenType.False ||
            type == TokenType.Null;

        /// <summary>
        /// Checks if the token type represents a structural token (colons, brackets, braces, commas, pipes).
        /// </summary>
        public static bool IsStructuralToken(this TokenType type) =>
            type == TokenType.Colon ||
            type == TokenType.Comma ||
            type == TokenType.Pipe ||
            type == TokenType.LeftBracket ||
            type == TokenType.RightBracket ||
            type == TokenType.LeftBrace ||
            type == TokenType.RightBrace;

        /// <summary>
        /// Checks if the token type represents whitespace or formatting (whitespace, newline, indent, dedent).
        /// </summary>
        public static bool IsWhitespaceToken(this TokenType type) =>
            type == TokenType.Whitespace ||
            type == TokenType.Newline ||
            type == TokenType.Indent ||
            type == TokenType.Dedent;

        /// <summary>
        /// Checks if the token type represents a keyword (true, false, null).
        /// </summary>
        public static bool IsKeywordToken(this TokenType type) =>
            type == TokenType.True ||
            type == TokenType.False ||
            type == TokenType.Null;
    }
}
