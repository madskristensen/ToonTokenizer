using System;
using System.Collections.Generic;
using ToonTokenizer.Ast;

namespace ToonTokenizer
{
    /// <summary>
    /// Main entry point for parsing TOON documents.
    /// </summary>
    public class Toon
    {
        /// <summary>
        /// Parses TOON source text and returns an AST.
        /// </summary>
        /// <param name="source">The TOON source text to parse.</param>
        /// <returns>A ToonDocument representing the parsed AST.</returns>
        public static ToonDocument Parse(string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var lexer = new ToonLexer(source);
            var tokens = lexer.Tokenize();
            // Debug helper: when parsing specific failing cases, emit tokens to stderr
            try
            {
                if (source != null && source.Contains("items[3]{value}"))
                {
                    foreach (var t in tokens)
                    {
                        System.Console.Error.WriteLine($"TOK: {t.Type} '{t.Value}' (line {t.Line}, col {t.Column})");
                    }
                }
            }
            catch { }
            
            var parser = new ToonParser(tokens);
            return parser.Parse();
        }

        /// <summary>
        /// Tokenizes TOON source text and returns all tokens.
        /// </summary>
        /// <param name="source">The TOON source text to tokenize.</param>
        /// <returns>A list of tokens.</returns>
        public static List<Token> Tokenize(string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var lexer = new ToonLexer(source);
            return lexer.Tokenize();
        }

        /// <summary>
        /// Validates TOON source text and returns any errors.
        /// </summary>
        /// <param name="source">The TOON source text to validate.</param>
        /// <param name="errors">Output parameter for validation errors.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public static bool TryParse(string source, out List<string> errors)
        {
            errors = new List<string>();

            if (source == null)
            {
                errors.Add("Source text cannot be null");
                return false;
            }

            try
            {
                var document = Parse(source);
                return true;
            }
            catch (ParseException ex)
            {
                errors.Add(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                errors.Add($"Unexpected error: {ex.Message}");
                return false;
            }
        }
    }
}
