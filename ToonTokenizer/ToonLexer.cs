using System;
using System.Collections.Generic;
using System.Text;

namespace ToonTokenizer
{
    /// <summary>
    /// Lexical analyzer for the TOON language. Converts source text into tokens.
    /// </summary>
    public class ToonLexer
    {
        private readonly string _source;
        private int _position;
        private int _line;
        private int _column;

        private readonly Stack<int> _indentStack;
        private readonly List<ToonError> _errors;
        private readonly StringBuilder _stringBuilder;
        private readonly ToonParserOptions _options;
        private int _tokenCount;

        public ToonLexer(string source) : this(source, ToonParserOptions.Default)
        {
        }

        public ToonLexer(string source, ToonParserOptions? options)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _options = options ?? ToonParserOptions.Default;
            _position = 0;
            _line = 1;
            _column = 1;
            _indentStack = new Stack<int>();
            _indentStack.Push(0);
            _errors = [];
            _stringBuilder = new StringBuilder();
            _tokenCount = 0;
        }

        /// <summary>
        /// Gets the list of errors encountered during tokenization.
        /// </summary>
        public List<ToonError> Errors => _errors;

        /// <summary>
        /// Tokenizes the entire source and returns all tokens.
        /// </summary>
        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            Token token;

            while ((token = NextToken()).Type != TokenType.EndOfFile)
            {
                tokens.Add(token);
                
                // Check token count limit (excluding EOF)
                _tokenCount++;
                if (_tokenCount > _options.MaxTokenCount)
                {
                    _errors.Add(new ToonError(
                        $"Token count ({_tokenCount:N0}) exceeds maximum allowed ({_options.MaxTokenCount:N0}). " +
                        $"This limit protects against algorithmic complexity attacks. " +
                        $"To tokenize larger inputs, increase ToonParserOptions.MaxTokenCount.",
                        token.Position,
                        token.Length,
                        token.Line,
                        token.Column,
                        ErrorCode.UnexpectedToken));
                    // Stop tokenization to prevent excessive memory usage
                    break;
                }
            }

            tokens.Add(token); // Add EOF token
            return tokens;
        }

        /// <summary>
        /// Gets the next token from the source.
        /// </summary>
        public Token NextToken()
        {
            // Handle dedents at end of file
            if (_position >= _source.Length)
            {
                if (_indentStack.Count > 1)
                {
                    _indentStack.Pop();
                    return CreateToken(TokenType.Dedent, string.Empty);
                }
                return CreateToken(TokenType.EndOfFile, string.Empty);
            }

            char current = _source[_position];

            // Skip inline whitespace (not newlines)
            if (current == ' ' || current == '\t')
            {
                return ConsumeWhitespace();
            }

            // Handle newlines and indentation
            if (current == '\n' || current == '\r')
            {
                return ConsumeNewline();
            }

            // Comments (# or //)
            if (current == '#' || (current == '/' && Peek() == '/'))
            {
                return ConsumeComment();
            }

            // Structural tokens
            switch (current)
            {
                case ':':
                    return ConsumeSingleChar(TokenType.Colon);
                case ',':
                    return ConsumeSingleChar(TokenType.Comma);
                case '|':
                    return ConsumeSingleChar(TokenType.Pipe);
                case '[':
                    return ConsumeSingleChar(TokenType.LeftBracket);
                case ']':
                    return ConsumeSingleChar(TokenType.RightBracket);
                case '{':
                    return ConsumeSingleChar(TokenType.LeftBrace);
                case '}':
                    return ConsumeSingleChar(TokenType.RightBrace);
            }

            // String literals (quoted)
            if (current == '"' || current == '\'')
            {
                return ConsumeQuotedString(current);
            }


            // Numbers (only if '-' is followed by digit, or digit)
            if (char.IsDigit(current) || (current == '-' && char.IsDigit(Peek())))
            {
                return ConsumeNumber();
            }

            // Keywords and identifiers
            if (char.IsLetter(current) || current == '_')
            {
                return ConsumeIdentifierOrKeyword();
            }

            // Unquoted string (for TOON compact syntax)
            if (IsUnquotedStringStart(current))
            {
                return ConsumeUnquotedString();
            }

            // Invalid character
            return ConsumeSingleChar(TokenType.Invalid);
        }

        private Token ConsumeWhitespace()
        {
            int start = _position;
            int startColumn = _column;

            while (_position < _source.Length && (_source[_position] == ' ' || _source[_position] == '\t'))
            {
                _position++;
                _column++;
            }

            string value = _source.Substring(start, _position - start);
            return new Token(TokenType.Whitespace, value, _line, startColumn, start, value.Length);
        }

        private Token ConsumeNewline()
        {
            int start = _position;
            int startColumn = _column;
            int startLine = _line;

            if (_source[_position] == '\r' && Peek() == '\n')
            {
                _position += 2;
            }
            else
            {
                _position++;
            }

            _line++;
            _column = 1;

            // Check indentation on the next line
            if (_position < _source.Length)
            {
                ProcessIndentation();
            }

            return new Token(TokenType.Newline, "\n", startLine, startColumn, start, _position - start);
        }

        private void ProcessIndentation()
        {
            int indentCount = 0;
            while (_position < _source.Length && _source[_position] == ' ')
            {
                indentCount++;
                _position++;
                _column++;
            }

            int currentIndent = _indentStack.Peek();

            if (indentCount > currentIndent)
            {
                _indentStack.Push(indentCount);
            }
            else if (indentCount < currentIndent)
            {
                while (_indentStack.Count > 1 && _indentStack.Peek() > indentCount)
                {
                    _indentStack.Pop();
                }
            }
        }

        private Token ConsumeComment()
        {
            int start = _position;
            int startColumn = _column;

            if (_source[_position] == '#')
            {
                _position++;
                _column++;
            }
            else if (_source[_position] == '/' && Peek() == '/')
            {
                _position += 2;
                _column += 2;
            }

            while (_position < _source.Length && _source[_position] != '\n' && _source[_position] != '\r')
            {
                _position++;
                _column++;
            }

            string value = _source.Substring(start, _position - start);
            return new Token(TokenType.Comment, value, _line, startColumn, start, value.Length);
        }

        private Token ConsumeSingleChar(TokenType type)
        {
            char value = _source[_position];
            int start = _position;
            int startColumn = _column;

            _position++;
            _column++;

            return new Token(type, value.ToString(), _line, startColumn, start, 1);
        }

        private Token ConsumeQuotedString(char quote)
        {
            int start = _position;
            int startColumn = _column;
            _stringBuilder.Clear(); // Reuse field-level StringBuilder

            _position++; // Skip opening quote
            _column++;

            // Track string length for security validation
            int stringLength = 0;

            while (_position < _source.Length && _source[_position] != quote)
            {
                // Stop at newlines for unterminated strings (better error recovery)
                if (_source[_position] == '\n' || _source[_position] == '\r')
                {
                    break;
                }

                if (_source[_position] == '\\' && _position + 1 < _source.Length)
                {
                    _position++;
                    _column++;
                    char escaped = _source[_position];
                    switch (escaped)
                    {
                        case 'n': _stringBuilder.Append('\n'); break;
                        case 'r': _stringBuilder.Append('\r'); break;
                        case 't': _stringBuilder.Append('\t'); break;
                        case '\\': _stringBuilder.Append('\\'); break;
                        case '"': _stringBuilder.Append('"'); break;
                        case '\'':
                            // Single quotes can be escaped within single-quoted strings
                            if (quote == '\'')
                                _stringBuilder.Append('\'');
                            else
                            {
                                // Invalid escape - record error and include literal characters
                                _errors.Add(new ToonError(
                                    $"Invalid escape sequence: \\{escaped} at line {_line}, column {_column}. " +
                                    $"Valid escape sequences are: \\n (newline), \\r (carriage return), \\t (tab), \\\\ (backslash), \\\" (double quote)" +
                                    (quote == '\'' ? ", \\' (single quote)" : "") + ". " +
                                    $"Fix: Use \\\\ if you meant a literal backslash, or remove the backslash for a regular character",
                                    _position - 1,
                                    2,
                                    _line,
                                    _column - 1,
                                    ErrorCode.InvalidEscapeSequence));
                                _stringBuilder.Append('\\');
                                _stringBuilder.Append(escaped);
                            }
                            break;
                        default:
                            // Invalid escape - record error and include literal characters
                            _errors.Add(new ToonError(
                                $"Invalid escape sequence: \\{escaped} at line {_line}, column {_column}. " +
                                $"Valid escape sequences are: \\n (newline), \\r (carriage return), \\t (tab), \\\\ (backslash), \\\" (double quote)" +
                                (quote == '\'' ? ", \\' (single quote)" : "") + ". " +
                                $"Fix: Use \\\\ if you meant a literal backslash, or remove the backslash for a regular character",
                                _position - 1,
                                2,
                                _line,
                                _column - 1,
                                ErrorCode.InvalidEscapeSequence));
                            _stringBuilder.Append('\\');
                            _stringBuilder.Append(escaped);
                            break;
                    }
                }
                else
                {
                    _stringBuilder.Append(_source[_position]);
                }

                _position++;
                _column++;
                stringLength++;

                // Check string length limit
                if (stringLength > _options.MaxStringLength)
                {
                    _errors.Add(new ToonError(
                        $"String length ({stringLength:N0} characters) exceeds maximum allowed ({_options.MaxStringLength:N0}). " +
                        $"This limit protects against memory exhaustion. " +
                        $"To parse longer strings, increase ToonParserOptions.MaxStringLength.",
                        start,
                        _position - start,
                        _line,
                        startColumn,
                        ErrorCode.UnexpectedToken));
                    // Continue parsing but truncate the string
                    break;
                }
            }

            // Check for unterminated string (spec §7.1: decoders MUST reject unterminated strings)
            // For resilient parsing, record error and continue to next line
            if (_position >= _source.Length || _source[_position] != quote)
            {
                // String was not properly closed (hit EOF or newline)
                int endPos = _position;
                string reason = _position >= _source.Length ? "end of file" : "newline";
                string quoteType = quote == '"' ? "double" : "single";

                // Record error but continue parsing
                _errors.Add(new ToonError(
                    $"Unterminated {quoteType}-quoted string at line {_line}, column {startColumn}. " +
                    $"String reached {reason} without closing {quote} character. " +
                    $"Fix: Add closing {quote} before the end of the line",
                    start,
                    endPos - start,
                    _line,
                    startColumn,
                    ErrorCode.UnterminatedString));

                // Return what we have so far as an invalid token
                return new Token(TokenType.Invalid, quote + _stringBuilder.ToString(), _line, startColumn, start, endPos - start);
            }

            if (_position < _source.Length)
            {
                _position++; // Skip closing quote
                _column++;
            }

            return new Token(TokenType.String, _stringBuilder.ToString(), _line, startColumn, start, _position - start);
        }

        private Token ConsumeNumber()
        {
            int start = _position;
            int startColumn = _column;

            if (_source[_position] == '-')
            {
                _position++;
                _column++;
            }

            int digitStart = _position;
            while (_position < _source.Length && char.IsDigit(_source[_position]))
            {
                _position++;
                _column++;
            }

            bool hasDot = false;
            if (_position < _source.Length && _source[_position] == '.')
            {
                hasDot = true;
                _position++;
                _column++;

                while (_position < _source.Length && char.IsDigit(_source[_position]))
                {
                    _position++;
                    _column++;
                }
            }

            bool hasExponent = false;
            if (_position < _source.Length && (_source[_position] == 'e' || _source[_position] == 'E'))
            {
                hasExponent = true;
                _position++;
                _column++;

                if (_position < _source.Length && (_source[_position] == '+' || _source[_position] == '-'))
                {
                    _position++;
                    _column++;
                }

                while (_position < _source.Length && char.IsDigit(_source[_position]))
                {
                    _position++;
                    _column++;
                }
            }

            string value = _source.Substring(start, _position - start);

            // TOON spec: Check for forbidden leading zeros
            // Forbidden: "05", "0001", "-01" (integers with leading zeros, excluding "0", "-0")
            // Allowed: "0", "-0", "0.1", "-0.1", "1e-6"
            // Extract just the integer part before decimal or exponent
            int integerPartEnd = digitStart;
            while (integerPartEnd < _source.Length && char.IsDigit(_source[integerPartEnd]))
            {
                integerPartEnd++;
            }
            string integerPart = _source.Substring(digitStart, integerPartEnd - digitStart);

            // Check if it's an integer (no dot, no exponent) with forbidden leading zeros
            bool isInteger = !hasDot && !hasExponent;
            bool hasForbiddenLeadingZero = isInteger && integerPart.Length > 1 && integerPart[0] == '0';

            if (hasForbiddenLeadingZero)
            {
                // Treat as string per TOON spec
                return new Token(TokenType.String, value, _line, startColumn, start, value.Length);
            }

            return new Token(TokenType.Number, value, _line, startColumn, start, value.Length);
        }

        private Token ConsumeIdentifierOrKeyword()
        {
            int start = _position;
            int startColumn = _column;

            // Allow hyphens, dots, and @ within value tokens (kebab-case, dotted paths, and emails)
            while (_position < _source.Length && (char.IsLetterOrDigit(_source[_position]) || _source[_position] == '_' || _source[_position] == '-' || _source[_position] == '.' || _source[_position] == '@'))
            {
                _position++;
                _column++;
                
                // Check identifier/string length
                int currentLength = _position - start;
                if (currentLength > _options.MaxStringLength)
                {
                    _errors.Add(new ToonError(
                        $"Identifier length ({currentLength:N0} characters) exceeds maximum allowed ({_options.MaxStringLength:N0}). " +
                        $"To parse longer identifiers, increase ToonParserOptions.MaxStringLength.",
                        start,
                        currentLength,
                        _line,
                        startColumn,
                        ErrorCode.UnexpectedToken));
                    break;
                }
            }

            string value = _source.Substring(start, _position - start);
            TokenType type;

            switch (value)
            {
                case "true": type = TokenType.True; return new Token(type, value, _line, startColumn, start, value.Length);
                case "false": type = TokenType.False; return new Token(type, value, _line, startColumn, start, value.Length);
                case "null": type = TokenType.Null; return new Token(type, value, _line, startColumn, start, value.Length);
            }

            // Look ahead (without consuming) to classify as Identifier (property key) or String (value)
            int look = _position;
            // Skip any spaces between word and possible structural char
            while (look < _source.Length && (_source[look] == ' ' || _source[look] == '\t')) look++;
            char next = look < _source.Length ? _source[look] : '\0';

            // Property key patterns: directly followed by ':' OR '[' OR '{'
            if (next == ':' || next == '[' || next == '{')
            {
                type = TokenType.Identifier;
            }
            else
            {
                type = TokenType.String;
            }

            return new Token(type, value, _line, startColumn, start, value.Length);
        }

        private Token ConsumeUnquotedString()
        {
            int start = _position;
            int startColumn = _column;

            while (_position < _source.Length && IsUnquotedStringChar(_source[_position]))
            {
                _position++;
                _column++;
                
                // Check unquoted string length
                int currentLength = _position - start;
                if (currentLength > _options.MaxStringLength)
                {
                    _errors.Add(new ToonError(
                        $"Unquoted string length ({currentLength:N0} characters) exceeds maximum allowed ({_options.MaxStringLength:N0}). " +
                        $"To parse longer strings, increase ToonParserOptions.MaxStringLength.",
                        start,
                        currentLength,
                        _line,
                        startColumn,
                        ErrorCode.UnexpectedToken));
                    break;
                }
            }

            string value = _source.Substring(start, _position - start);
            return new Token(TokenType.String, value, _line, startColumn, start, value.Length);
        }

        private bool IsUnquotedStringStart(char c)
        {
            // Allow '-' unless it's the first character and followed by a digit (number)
            if (c == '-')
            {
                // If '-' is at the start and followed by digit, it's a number
                return !char.IsDigit(Peek());
            }
            // Allow @ as a valid starting character (for emails, handles, etc.)
            // Per TOON spec §7.2: unquoted strings cannot contain: :, ", \, [, ], {, }, or comment markers (#, /)
            return !char.IsWhiteSpace(c) && c != ':' && c != ',' && c != '[' && c != ']' &&
                   c != '{' && c != '}' && c != '#' && c != '/' && c != '"' && c != '\'' && c != '\\';
        }

        private static bool IsUnquotedStringChar(char c)
        {
            // Allow hyphens and @ in the middle of unquoted strings
            // Per TOON spec §7.2: unquoted strings cannot contain: :, ", \, [, ], {, }, or comment markers (#, /)
            return !char.IsWhiteSpace(c) && c != ',' && c != ':' && c != '[' && c != ']' &&
                   c != '{' && c != '}' && c != '#' && c != '/' && c != '"' && c != '\\';
        }

        private char Peek(int offset = 1)
        {
            int pos = _position + offset;
            return pos < _source.Length ? _source[pos] : '\0';
        }

        private Token CreateToken(TokenType type, string value)
        {
            return new Token(type, value, _line, _column, _position, value.Length);
        }
    }
}
