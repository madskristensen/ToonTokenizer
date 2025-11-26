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
    /// <remarks>
    /// Tokens are the atomic lexical units produced by the lexer before parsing.
    /// Value tokens (String, Number, True, False, Null, Identifier) represent data,
    /// structural tokens (Colon, Comma, Brackets, Braces) define structure,
    /// and formatting tokens (Newline, Indent, Dedent, Whitespace) preserve layout.
    /// </remarks>
    public enum TokenType
    {
        /// <summary>A string literal (quoted or unquoted).</summary>
        String,
        /// <summary>A numeric literal (integer or floating-point).</summary>
        Number,
        /// <summary>The boolean keyword 'true'.</summary>
        True,
        /// <summary>The boolean keyword 'false'.</summary>
        False,
        /// <summary>The null keyword 'null'.</summary>
        Null,
        
        /// <summary>An unquoted identifier (property keys, unquoted strings).</summary>
        Identifier,
        
        /// <summary>Colon separator (:) - separates keys from values.</summary>
        Colon,
        /// <summary>Comma delimiter (,) - default array/table element separator.</summary>
        Comma,
        /// <summary>Pipe delimiter (|) - alternative array/table element separator.</summary>
        Pipe,
        /// <summary>Left square bracket ([) - starts array size or inline array.</summary>
        LeftBracket,
        /// <summary>Right square bracket (]) - ends array size or inline array.</summary>
        RightBracket,
        /// <summary>Left curly brace ({) - starts table array schema.</summary>
        LeftBrace,
        /// <summary>Right curly brace (}) - ends table array schema.</summary>
        RightBrace,
        
        /// <summary>Newline character (LF, U+000A).</summary>
        Newline,
        /// <summary>Indentation increase (virtual token).</summary>
        Indent,
        /// <summary>Indentation decrease (virtual token).</summary>
        Dedent,
        /// <summary>Horizontal whitespace (spaces, but not newlines or tabs in delimiter context).</summary>
        Whitespace,
        
        /// <summary>Comment (# or // style) - ignored during parsing.</summary>
        Comment,
        
        /// <summary>End of file marker (virtual token).</summary>
        EndOfFile,
        /// <summary>Invalid or unrecognized token.</summary>
        Invalid
    }
}
