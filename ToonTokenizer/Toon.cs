using System;
using System.Collections.Generic;

namespace ToonTokenizer
{
    /// <summary>
    /// Main entry point for parsing TOON documents.
    /// </summary>
    public class Toon
    {
        /// <summary>
        /// Parses TOON source text and returns a parse result containing the AST and any errors.
        /// The parser is resilient and will continue parsing after errors, returning a partial AST.
        /// </summary>
        /// <param name="source">The TOON source text to parse.</param>
        /// <returns>A ToonParseResult containing the parsed document (possibly partial) and any errors.</returns>
        public static ToonParseResult Parse(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                return ToonParseResult.Failure(new ToonError("Source text cannot be null or empty", 0, 0, 0, 0));
            }

            try
            {
                ToonLexer lexer = new(source);
                var tokens = lexer.Tokenize();

                ToonParser parser = new(tokens);
                var document = parser.Parse();

                // Combine lexer and parser errors
                var allErrors = new List<ToonError>(lexer.Errors);
                allErrors.AddRange(parser.Errors);

                // If there are errors, return partial result
                if (allErrors.Count > 0)
                {
                    return ToonParseResult.Partial(document, allErrors, tokens);
                }

                // If document is empty (no properties), return failure
                // This handles empty strings, whitespace-only, and comments-only documents
                if (document.Properties.Count == 0)
                {
                    return ToonParseResult.Failure(new ToonError("Source text cannot be null or empty", 0, 0, 0, 0), tokens);
                }

                return ToonParseResult.Success(document, tokens);
            }
            catch (ParseException)
            {
                // Re-throw ParseException (from lexer for invalid escapes) to caller
                throw;
            }
            catch (Exception ex)
            {
                // For unexpected exceptions, return empty document with error
                return ToonParseResult.Failure(new ToonError($"Unexpected error: {ex.Message}", 0, 0, 0, 0));
            }
        }

        /// <summary>
        /// Encodes a JSON string to TOON format.
        /// Supports JSONC (JSON with comments) - both single-line (//) and multi-line (/* */) comments are ignored.
        /// Also supports trailing commas in objects and arrays.
        /// </summary>
        /// <param name="json">The JSON or JSONC string to convert.</param>
        /// <returns>A TOON formatted string.</returns>
        public static string Encode(string json)
        {
            return Encode(json, new ToonEncoderOptions());
        }

        /// <summary>
        /// Encodes a JSON string to TOON format with the specified options.
        /// Supports JSONC (JSON with comments) - both single-line (//) and multi-line (/* */) comments are ignored.
        /// Also supports trailing commas in objects and arrays.
        /// </summary>
        /// <param name="json">The JSON or JSONC string to convert.</param>
        /// <param name="options">Encoding options.</param>
        /// <returns>A TOON formatted string.</returns>
        public static string Encode(string json, ToonEncoderOptions options)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            ToonEncoder encoder = new(options);
            return encoder.EncodeFromJson(json);
        }

        /// <summary>
        /// Tokenizes TOON source text and returns all tokens.
        /// </summary>
        /// <param name="source">The TOON source text to tokenize.</param>
        /// <returns>A list of tokens.</returns>
        public static List<Token> Tokenize(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ToonLexer lexer = new(source);
            return lexer.Tokenize();
        }

        /// <summary>
        /// Validates TOON source text and returns the parse result with errors.
        /// Returns true if parsing completed (even with errors), false only on catastrophic failures.
        /// This allows language services to get partial results and all errors.
        /// </summary>
        /// <param name="source">The TOON source text to validate.</param>
        /// <param name="result">Output parameter for the parse result containing document and errors.</param>
        /// <returns>True if parsing completed, false only on catastrophic exceptions.</returns>
        public static bool TryParse(string source, out ToonParseResult result)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                result = ToonParseResult.Failure(new ToonError("Source text cannot be null or empty", 0, 0, 0, 0));
                return false;
            }

            try
            {
                result = Parse(source);
                // Ensure result is never null
                if (result == null)
                {
                    result = ToonParseResult.Failure(new ToonError("Unknown error: Parse returned null", 0, 0, 0, 0));
                    return false;
                }
                // Return true even if there are parse errors - we still have a partial document
                // TryParse only returns false for catastrophic failures (unexpected exceptions)
                return true;
            }
            catch (Exception ex)
            {
                result = ToonParseResult.Failure(new ToonError($"Catastrophic error: {ex.Message}", 0, 0, 0, 0));
                return false;
            }
        }
    }
}

