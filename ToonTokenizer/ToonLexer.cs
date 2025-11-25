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
        private int _lineStartPosition;
        private readonly Stack<int> _indentStack;

        public ToonLexer(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _position = 0;
            _line = 1;
            _column = 1;
            _lineStartPosition = 0;
            _indentStack = new Stack<int>();
            _indentStack.Push(0);
        }

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
            _lineStartPosition = _position;
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
            var sb = new StringBuilder();

            _position++; // Skip opening quote
            _column++;

            while (_position < _source.Length && _source[_position] != quote)
            {
                if (_source[_position] == '\\' && _position + 1 < _source.Length)
                {
                    _position++;
                    _column++;
                    char escaped = _source[_position];
                    switch (escaped)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '\\': sb.Append('\\'); break;
                        case '"': sb.Append('"'); break;
                        case '\'': 
                            // Single quotes can be escaped within single-quoted strings
                            if (quote == '\'')
                                sb.Append('\'');
                            else
                                throw new ParseException($"Invalid escape sequence: \\{escaped} at line {_line}, column {_column}");
                            break;
                        default:
                            throw new ParseException($"Invalid escape sequence: \\{escaped} at line {_line}, column {_column}");
                    }
                }
                else
                {
                    sb.Append(_source[_position]);
                }

                _position++;
                _column++;
            }

            // Check for unterminated string (spec §7.1: decoders MUST reject unterminated strings)
            if (_position >= _source.Length)
            {
                throw new ParseException($"Unterminated string starting at line {_line}, column {startColumn}");
            }

            if (_position < _source.Length)
            {
                _position++; // Skip closing quote
                _column++;
            }

            return new Token(TokenType.String, sb.ToString(), _line, startColumn, start, _position - start);
        }

        private Token ConsumeNumber()
        {
            int start = _position;
            int startColumn = _column;
            bool isNegative = false;

            if (_source[_position] == '-')
            {
                isNegative = true;
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
            bool hasForbiddenLeadingZero = isInteger && integerPart.Length > 1 && integerPart.StartsWith("0");
            
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

        private bool IsUnquotedStringChar(char c)
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
