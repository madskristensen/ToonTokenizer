using System;

namespace ToonTokenizer
{
    /// <summary>
    /// Represents a parsing or validation error with span information.
    /// </summary>
    public class ToonError
    {
        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The starting position (0-based index) of the error in the source string.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// The length of the span that contains the error.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The line number (1-based) where the error occurs.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// The column number (1-based) where the error occurs.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Creates a new ToonError.
        /// </summary>
        public ToonError(string message, int position, int length, int line, int column)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Position = position;
            Length = length;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Returns a string representation of the error with location information.
        /// </summary>
        public override string ToString()
        {
            return $"{Message} (line {Line}, column {Column}, position {Position}, length {Length})";
        }

        /// <summary>
        /// Gets the end position of the error span.
        /// </summary>
        public int EndPosition => Position + Length;
    }
}
