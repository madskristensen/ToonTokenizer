using System;

namespace ToonTokenizer
{
    /// <summary>
    /// Represents a single token in the TOON language with position tracking.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The type of this token.
        /// </summary>
        public TokenType Type { get; }
        
        /// <summary>
        /// The raw text value of this token.
        /// </summary>
        public string Value { get; }
        
        /// <summary>
        /// The line number where this token starts (1-based).
        /// </summary>
        public int Line { get; }
        
        /// <summary>
        /// The column number where this token starts (1-based).
        /// </summary>
        public int Column { get; }
        
        /// <summary>
        /// The absolute character position in the source text (0-based).
        /// </summary>
        public int Position { get; }
        
        /// <summary>
        /// The length of this token in characters.
        /// </summary>
        public int Length { get; }

        public Token(TokenType type, string value, int line, int column, int position, int length)
        {
            Type = type;
            Value = value ?? string.Empty;
            Line = line;
            Column = column;
            Position = position;
            Length = length;
        }

        public override string ToString()
        {
            return $"{Type}({Value}) at {Line}:{Column}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Token other)
            {
                return Type == other.Type &&
                       Value == other.Value &&
                       Line == other.Line &&
                       Column == other.Column &&
                       Position == other.Position;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Type.GetHashCode();
                hash = hash * 31 + (Value?.GetHashCode() ?? 0);
                hash = hash * 31 + Line.GetHashCode();
                hash = hash * 31 + Column.GetHashCode();
                hash = hash * 31 + Position.GetHashCode();
                return hash;
            }
        }
    }
}
