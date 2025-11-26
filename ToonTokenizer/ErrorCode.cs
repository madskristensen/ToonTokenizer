namespace ToonTokenizer
{
    /// <summary>
    /// Error codes for TOON parsing and lexical errors.
    /// These codes enable programmatic error handling and filtering.
    /// </summary>
    public static class ErrorCode
    {
        // Lexer errors (1xxx)
        /// <summary>
        /// String literal was not terminated before end of line or file.
        /// TOON spec §7.1 requires strings to be properly closed.
        /// </summary>
        public const string UnterminatedString = "TOON1001";

        /// <summary>
        /// Invalid escape sequence in string literal.
        /// Valid escapes: \n, \r, \t, \\, \", \' (in single-quoted strings only).
        /// </summary>
        public const string InvalidEscapeSequence = "TOON1002";

        /// <summary>
        /// Invalid character encountered that cannot be tokenized.
        /// </summary>
        public const string InvalidCharacter = "TOON1003";

        // Parser structural errors (2xxx)
        /// <summary>
        /// Expected a property key (identifier or string) but found something else.
        /// </summary>
        public const string ExpectedPropertyKey = "TOON2001";

        /// <summary>
        /// Expected a colon ':' after property key or array notation.
        /// </summary>
        public const string ExpectedColon = "TOON2002";

        /// <summary>
        /// Expected a right bracket ']' to close array size notation.
        /// </summary>
        public const string ExpectedRightBracket = "TOON2003";

        /// <summary>
        /// Expected a right brace '}' to close schema definition.
        /// </summary>
        public const string ExpectedRightBrace = "TOON2004";

        /// <summary>
        /// Expected a field name in schema definition.
        /// </summary>
        public const string ExpectedFieldName = "TOON2005";

        /// <summary>
        /// Expected a delimiter (comma, pipe, or tab) between elements.
        /// </summary>
        public const string ExpectedDelimiter = "TOON2006";

        /// <summary>
        /// Unexpected token type encountered during parsing.
        /// </summary>
        public const string UnexpectedToken = "TOON2007";

        /// <summary>
        /// Reached end of input unexpectedly while parsing.
        /// </summary>
        public const string UnexpectedEndOfInput = "TOON2008";

        // Array validation errors (3xxx)
        /// <summary>
        /// Array element count does not match declared size.
        /// TOON spec §6.1 requires array sizes to match declarations.
        /// </summary>
        public const string ArraySizeMismatch = "TOON3001";

        /// <summary>
        /// Table array row count does not match declared size.
        /// TOON spec §6.1 requires table row counts to match declarations.
        /// </summary>
        public const string TableSizeMismatch = "TOON3002";

        /// <summary>
        /// Table row has wrong number of fields (doesn't match schema).
        /// </summary>
        public const string TableRowFieldMismatch = "TOON3003";

        // Delimiter errors (4xxx)
        /// <summary>
        /// Mixed delimiters detected in array or schema.
        /// All elements in an array must use the same delimiter.
        /// </summary>
        public const string MixedDelimiters = "TOON4001";

        /// <summary>
        /// Expected delimiter but found delimiter marker.
        /// Common when delimiter marker is at wrong position.
        /// </summary>
        public const string DelimiterMarkerMisplaced = "TOON4002";

        // Indentation errors (5xxx)
        /// <summary>
        /// Unexpected indentation level detected.
        /// TOON uses significant whitespace for structure.
        /// </summary>
        public const string UnexpectedIndentation = "TOON5001";

        /// <summary>
        /// Inconsistent indentation detected (mixing spaces and tabs).
        /// </summary>
        public const string InconsistentIndentation = "TOON5002";

        // Internal errors (9xxx)
        /// <summary>
        /// Infinite loop detected in parser (safety check).
        /// This indicates a bug in the parser implementation.
        /// </summary>
        public const string InfiniteLoopDetected = "TOON9001";
    }
}
