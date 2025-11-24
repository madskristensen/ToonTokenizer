namespace ToonTokenizer
{
    /// <summary>
    /// The delimiter used in TOON arrays and tables.
    /// </summary>
    public enum Delimiter
    {
        /// <summary>Comma delimiter (default)</summary>
        Comma,
        /// <summary>Tab delimiter (HTAB, U+0009)</summary>
        Tab,
        /// <summary>Pipe delimiter (|)</summary>
        Pipe
    }

    /// <summary>
    /// Defines all token types in the TOON language.
    /// </summary>
    public enum TokenType
    {
        // Literals
        String,
        Number,
        True,
        False,
        Null,
        
        // Identifiers and Keywords
        Identifier,
        
        // Structural tokens
        Colon,              // :
        Comma,              // ,
        Pipe,               // |
        LeftBracket,        // [
        RightBracket,       // ]
        LeftBrace,          // {
        RightBrace,         // }
        
        // Whitespace and formatting
        Newline,
        Indent,
        Dedent,
        Whitespace,
        
        // Comments
        Comment,
        
        // Special
        EndOfFile,
        Invalid
    }
}
