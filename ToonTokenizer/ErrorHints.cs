namespace ToonTokenizer
{
    /// <summary>
    /// Provides helpful hints and suggestions for common TOON syntax mistakes.
    /// </summary>
    internal static class ErrorHints
    {
        /// <summary>
        /// Gets a helpful hint for array size mismatches based on the difference.
        /// </summary>
        public static string GetArraySizeMismatchHint(int declared, int actual)
        {
            if (actual == 0)
            {
                return "No elements found. Check if the array is on the correct line and properly indented.";
            }
            if (actual < declared)
            {
                int missing = declared - actual;
                return $"Missing {missing} element{(missing > 1 ? "s" : "")}. Check for incomplete array or missing delimiter between elements.";
            }
            else
            {
                int extra = actual - declared;
                return $"Found {extra} extra element{(extra > 1 ? "s" : "")}. Either increase the declared size []{declared}→[{actual}], or remove extra elements.";
            }
        }

        /// <summary>
        /// Gets a helpful hint for table size mismatches.
        /// </summary>
        public static string GetTableSizeMismatchHint(int declared, int actual)
        {
            if (actual == 0)
            {
                return "No rows found. Check if rows are properly indented under the table array declaration.";
            }
            if (actual < declared)
            {
                int missing = declared - actual;
                return $"Missing {missing} row{(missing > 1 ? "s" : "")}. Check for incomplete table data or missing rows.";
            }
            else
            {
                int extra = actual - declared;
                return $"Found {extra} extra row{(extra > 1 ? "s" : "")}. Either increase the declared size []{declared}→[{actual}], or remove extra rows.";
            }
        }

        /// <summary>
        /// Gets a hint for when a colon is missing after a property key.
        /// </summary>
        public static string GetMissingColonHint(string key)
        {
            // Check for common typos
            if (key.EndsWith(";"))
            {
                return $"Detected semicolon after '{key}'. In TOON, use colon ':' not semicolon ';' as separator.";
            }
            if (key.EndsWith("="))
            {
                return $"Detected equals sign after '{key}'. In TOON, use colon ':' not equals '=' as separator.";
            }
            if (key.Contains(" "))
            {
                return $"Property key '{key}' contains spaces. If this is a multi-word key, quote it. Then add ':' separator.";
            }
            return $"Add ':' after property key. Correct format: {key}: value";
        }

        /// <summary>
        /// Gets a hint for unterminated strings based on content.
        /// </summary>
        public static string GetUnterminatedStringHint(string content, char quote)
        {
            if (content.Contains("\n") || content.Contains("\r"))
            {
                return "Multi-line strings must be closed on the same line. For multi-line content, use separate quoted strings or escape sequences (\\n).";
            }
            if (content.Length > 100)
            {
                return $"Very long string ({content.Length} chars). Check if closing {quote} was accidentally omitted far from the opening quote.";
            }
            string quoteStr = quote.ToString();
            if (content.Contains(quoteStr) && !content.Contains($"\\{quote}"))
            {
                return $"String contains unescaped {quote} character. Use \\{quote} to include literal quotes in strings.";
            }
            return $"Add closing {quote} character before the end of the line.";
        }

        /// <summary>
        /// Gets a hint for missing delimiters in arrays.
        /// </summary>
        public static string GetMissingDelimiterHint(Delimiter delimiter, string nearToken)
        {
            string delimiterName = delimiter switch
            {
                Delimiter.Comma => "comma (,)",
                Delimiter.Tab => "tab character",
                Delimiter.Pipe => "pipe (|)",
                _ => "delimiter"
            };

            if (nearToken != null && nearToken.Contains(" ") && delimiter == Delimiter.Comma)
            {
                return $"Detected space-separated values near '{nearToken}'. This array uses {delimiterName} as delimiter. Add {delimiterName} between elements or quote multi-word values.";
            }

            return $"This array uses {delimiterName} as delimiter. Ensure all elements are separated by {delimiterName}.";
        }

        /// <summary>
        /// Gets a hint for unexpected tokens.
        /// </summary>
        public static string GetUnexpectedTokenHint(TokenType tokenType)
        {
            return tokenType switch
            {
                TokenType.LeftBracket => "Found '[' in unexpected position. Array size notation [n] must come immediately after property key.",
                TokenType.LeftBrace => "Found '{' in unexpected position. Schema notation {fields} must come after array size [n].",
                TokenType.Colon => "Found extra ':'. Each property should have exactly one colon separator.",
                TokenType.Comma => "Found ',' outside of array context. Commas are only used as array delimiters.",
                TokenType.Pipe => "Found '|' outside of array context. Pipes are only used as array delimiters when declared with [|].",
                TokenType.RightBracket => "Found ']' without matching '['. Check for extra or misplaced brackets.",
                TokenType.RightBrace => "Found '}' without matching '{'. Check for extra or misplaced braces.",
                _ => $"Unexpected {tokenType}. Check TOON syntax rules for this context."
            };
        }

        /// <summary>
        /// Gets a hint for indentation errors.
        /// </summary>
        public static string GetIndentationHint(int expected, int actual)
        {
            if (actual > expected)
            {
                return $"Over-indented by {actual - expected} spaces. Child properties should be indented by consistent amounts (e.g., 2 or 4 spaces per level).";
            }
            if (actual < expected)
            {
                return $"Under-indented by {expected - actual} spaces. Check if this property belongs to the correct parent object.";
            }
            return "Indentation must be consistent. TOON uses significant whitespace to define structure.";
        }
    }
}
