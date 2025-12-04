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
        /// Parses TOON source text with default security limits and returns a parse result containing the AST and any errors.
        /// The parser is resilient and will continue parsing after errors, returning a partial AST.
        /// </summary>
        /// <param name="source">The TOON source text to parse.</param>
        /// <returns>A ToonParseResult containing the parsed document (possibly partial) and any errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="ArgumentException">Thrown when source exceeds maximum input size.</exception>
        /// <remarks>
        /// <para>This method uses <see cref="ToonParserOptions.Default"/> security limits to protect against
        /// denial-of-service attacks. For untrusted input, these defaults are recommended.</para>
        /// 
        /// <para>The parser uses resilient error recovery, meaning it continues parsing after encountering errors.
        /// This allows language services and IDEs to provide IntelliSense and error highlighting for
        /// partially valid documents. Check result.HasErrors to determine if parsing was completely successful.</para>
        /// 
        /// <para><b>Security:</b> Default limits include 10 MB max input, 100 nesting depth, 1M array size,
        /// 1M tokens, and 64 KB string length. Use <see cref="Parse(string, ToonParserOptions)"/> for custom limits.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = Toon.Parse("name: John\nage: 30");
        /// if (result.IsSuccess) {
        ///     // Access result.Document
        /// } else {
        ///     // Check result.Errors for details
        /// }
        /// </code>
        /// </example>
        public static ToonParseResult Parse(string source)
        {
            return Parse(source, ToonParserOptions.Default);
        }

        /// <summary>
        /// Parses TOON source text with custom security limits and returns a parse result containing the AST and any errors.
        /// The parser is resilient and will continue parsing after errors, returning a partial AST.
        /// </summary>
        /// <param name="source">The TOON source text to parse.</param>
        /// <param name="options">Parser options including security limits. Use null for default limits.</param>
        /// <returns>A ToonParseResult containing the parsed document (possibly partial) and any errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="ArgumentException">Thrown when source exceeds maximum input size.</exception>
        /// <remarks>
        /// <para>This overload allows customization of security limits for specific use cases:</para>
        /// <list type="bullet">
        /// <item><b>Trusted input:</b> Use <see cref="ToonParserOptions.Unlimited"/> (with caution)</item>
        /// <item><b>High-throughput APIs:</b> Reduce limits to prevent resource exhaustion</item>
        /// <item><b>Large files:</b> Increase <see cref="ToonParserOptions.MaxInputSize"/></item>
        /// </list>
        /// 
        /// <para><b>Security Warning:</b> Using <see cref="ToonParserOptions.Unlimited"/> with untrusted input
        /// can lead to denial-of-service vulnerabilities. Always validate input source trustworthiness first.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Parse with custom limits
        /// var options = new ToonParserOptions {
        ///     MaxInputSize = 1024 * 1024,  // 1 MB
        ///     MaxNestingDepth = 50
        /// };
        /// var result = Toon.Parse(source, options);
        /// </code>
        /// </example>
        public static ToonParseResult Parse(string source, ToonParserOptions? options)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                return ToonParseResult.Failure(new ToonError("Source text cannot be null or empty", 0, 0, 0, 0));
            }

            // Use default options if none provided
            options ??= ToonParserOptions.Default;

            // Validate input size before processing
            if (source.Length > options.MaxInputSize)
            {
                throw new ArgumentException(
                    $"Input size ({source.Length:N0} bytes) exceeds maximum allowed size ({options.MaxInputSize:N0} bytes). " +
                    $"Consider increasing ToonParserOptions.MaxInputSize or validating input source. " +
                    $"Large inputs may indicate malicious content or data structure issues.",
                    nameof(source));
            }

            try
            {
                ToonLexer lexer = new(source, options);
                var tokens = lexer.Tokenize();

                ToonParser parser = new(tokens, options);
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
        /// Tokenizes TOON source text with default security limits and returns all tokens.
        /// </summary>
        /// <param name="source">The TOON source text to tokenize.</param>
        /// <returns>A list of tokens.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="ArgumentException">Thrown when source exceeds maximum input size.</exception>
        /// <remarks>
        /// Use this method when you only need the token stream for syntax highlighting or
        /// lexical analysis without building the full AST. For most cases, use Parse() instead,
        /// which includes tokens in the result along with the AST.
        /// 
        /// This method applies default security limits (10 MB input, 1M tokens, 64 KB strings).
        /// </remarks>
        /// <example>
        /// <code>
        /// var tokens = Toon.Tokenize("name: John");
        /// foreach (var token in tokens) {
        ///     Console.WriteLine($"{token.Type}: {token.Value}");
        /// }
        /// </code>
        /// </example>
        public static List<Token> Tokenize(string source)
        {
            return Tokenize(source, ToonParserOptions.Default);
        }

        /// <summary>
        /// Tokenizes TOON source text with custom security limits and returns all tokens.
        /// </summary>
        /// <param name="source">The TOON source text to tokenize.</param>
        /// <param name="options">Parser options including security limits. Use null for default limits.</param>
        /// <returns>A list of tokens.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="ArgumentException">Thrown when source exceeds maximum input size.</exception>
        public static List<Token> Tokenize(string source, ToonParserOptions? options)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Use default options if none provided
            options ??= ToonParserOptions.Default;

            // Validate input size before processing
            if (source.Length > options.MaxInputSize)
            {
                throw new ArgumentException(
                    $"Input size ({source.Length:N0} bytes) exceeds maximum allowed size ({options.MaxInputSize:N0} bytes).",
                nameof(source));
            }

            ToonLexer lexer = new(source, options);
            return lexer.Tokenize();
        }

        /// <summary>
        /// Validates TOON source text with default security limits and returns the parse result with errors.
        /// Returns true if parsing completed (even with errors), false only on catastrophic failures.
        /// This allows language services to get partial results and all errors.
        /// </summary>
        /// <param name="source">The TOON source text to validate.</param>
        /// <param name="result">Output parameter for the parse result containing document and errors.</param>
        /// <returns>True if parsing completed, false only on catastrophic exceptions.</returns>
        /// <remarks>
        /// Unlike traditional TryParse patterns, this method returns true even when parse errors occur.
        /// It only returns false for catastrophic failures (null source, unexpected exceptions).
        /// Always check result.IsSuccess or result.HasErrors to determine parse validity.
        /// 
        /// This method uses default security limits to protect against DoS attacks.
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Toon.TryParse(source, out var result)) {
        ///     if (result.IsSuccess) {
        ///         // Valid TOON - no errors
        ///     } else {
        ///         // Parse completed with errors - check result.Errors
        ///     }
        /// } else {
        ///     // Catastrophic failure
        /// }
        /// </code>
        /// </example>
        public static bool TryParse(string source, out ToonParseResult result)
        {
            return TryParse(source, ToonParserOptions.Default, out result);
        }

        /// <summary>
        /// Validates TOON source text with custom security limits and returns the parse result with errors.
        /// Returns true if parsing completed (even with errors), false only on catastrophic failures.
        /// </summary>
        /// <param name="source">The TOON source text to validate.</param>
        /// <param name="options">Parser options including security limits. Use null for default limits.</param>
        /// <param name="result">Output parameter for the parse result containing document and errors.</param>
        /// <returns>True if parsing completed, false only on catastrophic exceptions.</returns>
        public static bool TryParse(string source, ToonParserOptions? options, out ToonParseResult result)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                result = ToonParseResult.Failure(new ToonError("Source text cannot be null or empty", 0, 0, 0, 0));
                return false;
            }

            try
            {
                result = Parse(source, options);
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

