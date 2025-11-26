using System.Collections.Generic;

using ToonTokenizer.Ast;

namespace ToonTokenizer
{
    /// <summary>
    /// Represents the result of parsing a TOON document.
    /// </summary>
    public class ToonParseResult
    {
        /// <summary>
        /// The parsed TOON document. Always non-null after parsing, even if errors occurred.
        /// Contains a partial AST when errors are present.
        /// </summary>
        /// <seealso cref="HasErrors"/>
        /// <seealso cref="IsSuccess"/>
        public ToonDocument Document { get; set; }

        /// <summary>
        /// List of errors encountered during parsing. Empty list means successful parse.
        /// </summary>
        public List<ToonError> Errors { get; set; }

        /// <summary>
        /// List of tokens generated during lexical analysis.
        /// Allows consumers to access the token stream for syntax highlighting, IDE features, etc.
        /// </summary>
        public List<Token> Tokens { get; set; }

        /// <summary>
        /// Indicates whether the parsing was successful (no errors).
        /// </summary>
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>
        /// Indicates whether parsing encountered any errors.
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Creates a new ToonParseResult with an empty document.
        /// </summary>
        public ToonParseResult()
        {
            Document = new ToonDocument();
            Errors = [];
            Tokens = [];
        }

        /// <summary>
        /// Creates a parse result with a document and optional errors.
        /// </summary>
        public ToonParseResult(ToonDocument document, List<ToonError> errors)
        {
            Document = document ?? new ToonDocument();
            Errors = errors ?? [];
            Tokens = [];
        }

        /// <summary>
        /// Creates a successful parse result with a document and no errors.
        /// </summary>
        public static ToonParseResult Success(ToonDocument document, List<Token>? tokens = null)
        {
            return new ToonParseResult
            {
                Document = document ?? new ToonDocument(),
                Errors = [],
                Tokens = tokens ?? []
            };
        }

        /// <summary>
        /// Creates a parse result with both a partial document and errors.
        /// Used for resilient parsing where errors don't stop the parse.
        /// </summary>
        public static ToonParseResult Partial(ToonDocument document, List<ToonError> errors, List<Token>? tokens = null)
        {
            return new ToonParseResult
            {
                Document = document ?? new ToonDocument(),
                Errors = errors ?? [],
                Tokens = tokens ?? []
            };
        }

        /// <summary>
        /// Creates a failed parse result with errors and an empty document.
        /// Used only for catastrophic failures.
        /// </summary>
        public static ToonParseResult Failure(List<ToonError> errors, List<Token>? tokens = null)
        {
            return new ToonParseResult
            {
                Document = new ToonDocument(),
                Errors = errors ?? [],
                Tokens = tokens ?? []
            };
        }

        /// <summary>
        /// Creates a failed parse result with a single error and an empty document.
        /// Used only for catastrophic failures.
        /// </summary>
        public static ToonParseResult Failure(ToonError error, List<Token>? tokens = null)
        {
            return new ToonParseResult
            {
                Document = new ToonDocument(),
                Errors = [error],
                Tokens = tokens ?? []
            };
        }
    }
}
