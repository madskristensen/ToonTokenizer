using System;

namespace ToonTokenizer
{
    /// <summary>
    /// Represents a parsing or validation error with span information.
    /// </summary>
    /// <remarks>
    /// Creates a new ToonError.
    /// </remarks>
    public class ToonError(string message, int position, int length, int line, int column, string? code = null)
    {
        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; set; } = message ?? throw new ArgumentNullException(nameof(message));

        /// <summary>
        /// The error code for programmatic error handling (e.g., "TOON1001").
        /// See <see cref="ErrorCode"/> for standard error codes.
        /// </summary>
        public string? Code { get; set; } = code;

        /// <summary>
        /// The starting position (0-based index) of the error in the source string.
        /// </summary>
        public int Position { get; set; } = position;

        /// <summary>
        /// The length of the span that contains the error.
        /// </summary>
        public int Length { get; set; } = length;

        /// <summary>
        /// The line number (1-based) where the error occurs.
        /// </summary>
        public int Line { get; set; } = line;

        /// <summary>
        /// The column number (1-based) where the error occurs.
        /// </summary>
        public int Column { get; set; } = column;

        /// <summary>
        /// Returns a string representation of the error with location information.
        /// </summary>
        public override string ToString()
        {
            string codePrefix = !string.IsNullOrEmpty(Code) ? $"[{Code}] " : "";
            return $"{codePrefix}{Message} (line {Line}, column {Column}, position {Position}, length {Length})";
        }

        /// <summary>
        /// Gets the end position of the error span.
        /// </summary>
        public int EndPosition => Position + Length;
    }
}
