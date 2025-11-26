using System;
using System.Collections.Generic;
using System.Globalization;

using ToonTokenizer.Ast;

namespace ToonTokenizer
{
    /// <summary>
    /// Options for controlling which tokens to skip.
    /// </summary>
    [Flags]
    internal enum SkipOptions
    {
        None = 0,
        Whitespace = 1,
        Newlines = 2,
        Comments = 4,
        IndentDedent = 8,
        Invalid = 16,
        /// <summary>
        /// Respect tab delimiters when inside an array with tab delimiter.
        /// </summary>
        RespectTabDelimiter = 32,
        /// <summary>
        /// Only skip non-tab whitespace (used when tab might be a delimiter marker).
        /// </summary>
        NonTabWhitespaceOnly = 64
    }

    /// <summary>
    /// Parser for the TOON language. Converts tokens into an Abstract Syntax Tree (AST).
    /// </summary>
    public class ToonParser
    {
        private readonly List<Token> _tokens;
        private int _position;
        private readonly Stack<Delimiter> _delimiterStack;
        private readonly List<ToonError> _errors;

        // Constants
        private const int AverageTokenLength = 10;

        public ToonParser(List<Token> tokens)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _position = 0;
            _delimiterStack = new Stack<Delimiter>();
            _delimiterStack.Push(Delimiter.Comma); // Document delimiter default
            _errors = [];
        }

        /// <summary>
        /// Gets the list of errors encountered during parsing.
        /// </summary>
        public List<ToonError> Errors => _errors;

        /// <summary>
        /// Records an error and optionally advances to recover.
        /// </summary>
        private void RecordError(string message, Token token, bool skipToNextProperty = false, string? errorCode = null)
        {
            _errors.Add(new ToonError(
                message,
                token.Position,
                token.Value?.Length ?? 1,
                token.Line,
                token.Column,
                errorCode
            ));

            if (skipToNextProperty)
            {
                RecoverToNextProperty();
            }
        }

        /// <summary>
        /// Records an error with explicit position information.
        /// </summary>
        private void RecordError(string message, int position, int length, int line, int column, string? errorCode = null)
        {
            _errors.Add(new ToonError(message, position, length, line, column, errorCode));
        }

        /// <summary>
        /// Recovers from an error by advancing to the next property definition.
        /// </summary>
        private void RecoverToNextProperty()
        {
            while (!IsAtEnd() && CurrentToken.Type != TokenType.EndOfFile)
            {
                if (CurrentToken.Type == TokenType.Newline)
                {
                    Advance();
                    SkipWhitespaceAndComments();
                    if (!IsAtEnd() && IsPropertyStart())
                    {
                        break;
                    }
                }
                else
                {
                    Advance();
                }
            }
        }

        /// <summary>
        /// Checks if the current token could be the start of a property.
        /// </summary>
        private bool IsPropertyStart() =>
            CurrentToken.Type == TokenType.Identifier || CurrentToken.Type == TokenType.String;

        /// <summary>
        /// Gets the token to use as the start position for the current parsing context.
        /// </summary>
        private Token GetStartToken() => _position > 0 ? _tokens[_position - 1] : CurrentToken;

        /// <summary>
        /// Gets the token to use as the end position for the current parsing context.
        /// </summary>
        private Token GetEndToken() => _position > 0 ? _tokens[_position - 1] : CurrentToken;

        /// <summary>
        /// Parses the tokens into a TOON document AST.
        /// </summary>
        public ToonDocument Parse()
        {
            var document = new ToonDocument();

            while (!IsAtEnd())
            {
                // Use consolidated whitespace/comment skipper so we handle blank lines
                // and indentation tokens consistently between top-level properties.
                SkipWhitespaceAndComments();



                if (IsAtEnd() || CurrentToken.Type == TokenType.EndOfFile)
                    break;

                // If the next token is a valid property key, parse a property
                if (CurrentToken.Type == TokenType.Identifier || CurrentToken.Type == TokenType.String)
                {
                    try
                    {
                        var property = ParseProperty(0);
                        if (property != null)
                        {
                            document.Properties.Add(property);
                        }
                    }
                    catch (ParseException ex)
                    {
                        // Record error and try to recover
                        RecordError(ex.Message, ex.Position, ex.Length, ex.Line, ex.Column);
                        RecoverToNextProperty();
                    }
                }
                else
                {
                    // Skip any non-property tokens to remain robust against stray tokens
                    Advance();
                }
            }

            if (document.Properties.Count > 0)
            {
                document.StartLine = document.Properties[0].StartLine;
                document.StartColumn = document.Properties[0].StartColumn;
                document.StartPosition = document.Properties[0].StartPosition;

                var lastProp = document.Properties[document.Properties.Count - 1];
                document.EndLine = lastProp.EndLine;
                document.EndColumn = lastProp.EndColumn;
                document.EndPosition = lastProp.EndPosition;
            }

            return document;
        }

        private PropertyNode? ParseProperty(int expectedIndent)
        {
            if (IsAtEnd())
                return null;

            var startToken = CurrentToken;

            // Parse property key
            string? key = ParsePropertyKey();
            if (key == null)
                return null;

            // Parse array notation (if present)
            var (arraySize, arrayDelimiter) = ParseArrayNotation();

            // Parse schema notation (if present)
            List<string>? schema = ParseSchemaNotation(arrayDelimiter);

            // Expect colon separator
            if (!ExpectColon())
            {
                // ExpectColon returns incomplete property on error
                return new PropertyNode
                {
                    Key = key,
                    Value = new NullValueNode(),
                    IndentLevel = expectedIndent,
                    StartLine = startToken.Line,
                    StartColumn = startToken.Column,
                    StartPosition = startToken.Position,
                    EndLine = CurrentToken.Line,
                    EndColumn = CurrentToken.Column,
                    EndPosition = CurrentToken.Position
                };
            }

            // Parse property value
            AstNode value = ParsePropertyValue(arraySize, arrayDelimiter, schema);

            // Create and return property node
            var property = new PropertyNode
            {
                Key = key,
                Value = value,
                IndentLevel = expectedIndent,
                StartLine = startToken.Line,
                StartColumn = startToken.Column,
                StartPosition = startToken.Position,
                EndLine = _position > 0 ? _tokens[_position - 1].Line : startToken.Line,
                EndColumn = _position > 0 ? _tokens[_position - 1].Column : startToken.Column,
                EndPosition = _position > 0 ? _tokens[_position - 1].Position : startToken.Position
            };

            SkipWhitespaceAndComments();

            return property;
        }

        /// <summary>
        /// Parses a property key (identifier or string).
        /// </summary>
        private string? ParsePropertyKey()
        {
            if (CurrentToken.Type != TokenType.Identifier && CurrentToken.Type != TokenType.String)
            {
                RecordError(ParserErrorMessages.ExpectedPropertyKey, CurrentToken, false, ErrorCode.ExpectedPropertyKey);
                return null;
            }

            string key = CurrentToken.Value;
            Advance();
            SkipWhitespace();
            return key;
        }

        /// <summary>
        /// Parses array notation [size] with optional delimiter marker.
        /// Returns (arraySize, delimiter) tuple.
        /// </summary>
        private (int? arraySize, Delimiter? delimiter) ParseArrayNotation()
        {
            if (CurrentToken.Type != TokenType.LeftBracket)
                return (null, null);

            Advance();
            SkipNonDelimiterWhitespace();

            int? arraySize = null;
            Delimiter? arrayDelimiter = null;

            if (CurrentToken.Type == TokenType.Number)
            {
                arraySize = (int)double.Parse(CurrentToken.Value, CultureInfo.InvariantCulture);
                Advance();
                SkipNonDelimiterWhitespace();
            }

            // Check for delimiter marker before closing bracket
            if (CurrentToken.Type == TokenType.Whitespace && _position + 1 < _tokens.Count &&
                _tokens[_position + 1].Type == TokenType.RightBracket)
            {
                // Tab delimiter: whitespace token containing only tab character
                if (CurrentToken.Value == "\t")
                {
                    arrayDelimiter = Delimiter.Tab;
                    Advance(); // consume the tab
                }
            }
            else if (CurrentToken.Type == TokenType.Pipe)
            {
                arrayDelimiter = Delimiter.Pipe;
                Advance();
            }
            // If no delimiter marker found, default is Comma (no need to set explicitly)

            if (CurrentToken.Type != TokenType.RightBracket)
            {
                RecordError(ParserErrorMessages.ExpectedRightBracket, CurrentToken, false, ErrorCode.ExpectedRightBracket);
                // Try to recover by finding the next colon
                while (!IsAtEnd() && CurrentToken.Type != TokenType.Colon && CurrentToken.Type != TokenType.Newline)
                {
                    if (CurrentToken.Type == TokenType.RightBracket)
                    {
                        Advance();
                        break;
                    }
                    Advance();
                }
            }
            else
            {
                Advance();
            }
            SkipWhitespace();

            return (arraySize, arrayDelimiter);
        }

        /// <summary>
        /// Parses schema notation {field1, field2, ...}.
        /// </summary>
        private List<string>? ParseSchemaNotation(Delimiter? arrayDelimiter)
        {
            if (CurrentToken.Type != TokenType.LeftBrace)
                return null;

            // Determine active delimiter: use array delimiter if specified, otherwise parent delimiter
            Delimiter activeDelimiter = arrayDelimiter ?? _delimiterStack.Peek();
            try
            {
                return ParseSchema(activeDelimiter);
            }
            catch (ParseException ex)
            {
                RecordError(ex.Message, ex.Position, ex.Length, ex.Line, ex.Column);
                // Try to recover by skipping to next }
                while (!IsAtEnd() && CurrentToken.Type != TokenType.RightBrace && CurrentToken.Type != TokenType.Colon)
                {
                    Advance();
                }
                if (CurrentToken.Type == TokenType.RightBrace)
                {
                    Advance();
                }
                return null;
            }
        }

        /// <summary>
        /// Expects a colon separator after property key/notation.
        /// Returns false on error (after recording error and creating recovery node).
        /// </summary>
        private bool ExpectColon()
        {
            if (CurrentToken.Type == TokenType.Colon)
            {
                Advance();
                SkipWhitespace();
                return true;
            }

            // Fallback scan: try to find colon, skipping blank lines and comments
            int lookahead = _position;
            while (lookahead < _tokens.Count)
            {
                var t = _tokens[lookahead];
                // Skip whitespace, newlines, and comments
                if (t.Type == TokenType.Whitespace || t.Type == TokenType.Newline ||
                    t.Type == TokenType.Comment || t.Type == TokenType.Indent ||
                    t.Type == TokenType.Dedent)
                {
                    lookahead++;
                    continue;
                }
                if (t.Type == TokenType.Colon)
                {
                    _position = lookahead; // jump to colon token
                    Advance();
                    SkipWhitespace();
                    return true;
                }
                // If we hit another property key or EOF before colon, error
                if (t.Type == TokenType.Identifier || t.Type == TokenType.String || t.Type == TokenType.EndOfFile)
                {
                    break;
                }
                lookahead++;
            }

            // Colon not found - record error and prepare for recovery
            RecordError(ParserErrorMessages.ExpectedColonAfterKey, CurrentToken, false, ErrorCode.ExpectedColon);
            // Skip to the next line to recover (avoid multiple errors for same line)
            while (!IsAtEnd() && CurrentToken.Type != TokenType.Newline && CurrentToken.Type != TokenType.EndOfFile)
            {
                Advance();
            }
            return false;
        }

        /// <summary>
        /// Parses the value portion of a property, dispatching to the appropriate parser.
        /// </summary>
        private AstNode ParsePropertyValue(int? arraySize, Delimiter? arrayDelimiter, List<string>? schema)
        {
            // Push active delimiter to stack if this is an array
            if (arraySize.HasValue)
            {
                Delimiter activeDelimiter = arrayDelimiter ?? Delimiter.Comma;
                _delimiterStack.Push(activeDelimiter);
            }

            try
            {
                if (CurrentToken.Type == TokenType.Newline)
                {
                    // Multi-line value (object or table array)
                    Advance();
                    // Skip blank lines and comments but preserve the indentation whitespace token
                    while (!IsAtEnd() && (CurrentToken.Type == TokenType.Newline || CurrentToken.Type == TokenType.Comment))
                    {
                        Advance();
                    }
                    int nextIndent = GetCurrentIndentation();

                    if (schema != null && arraySize.HasValue)
                    {
                        // Table array
                        return ParseTableArray(arraySize.Value, schema, nextIndent);
                    }
                    else if (arraySize.HasValue)
                    {
                        // Expanded list array
                        return ParseExpandedArray(arraySize.Value, nextIndent);
                    }
                    else
                    {
                        // Nested object
                        return ParseObject(nextIndent);
                    }
                }
                else if (arraySize.HasValue && schema == null)
                {
                    // Inline array
                    return ParseInlineArray(arraySize.Value);
                }
                else
                {
                    // Simple value
                    return ParseValue();
                }
            }
            catch (ParseException ex)
            {
                RecordError(ex.Message, ex.Position, ex.Length, ex.Line, ex.Column);
                // Use null as fallback value
                return new NullValueNode();
            }
            finally
            {
                // Pop delimiter stack if we pushed one
                if (arraySize.HasValue)
                {
                    _delimiterStack.Pop();
                }
            }
        }

        private List<string> ParseSchema(Delimiter delimiter)
        {
            var schema = new List<string>();

            Advance(); // Skip {

            // Skip whitespace but not delimiter tabs
            while (!IsAtEnd() && CurrentToken.Type == TokenType.Whitespace)
            {
                if (delimiter == Delimiter.Tab && CurrentToken.Value.Contains("\t"))
                    break;
                Advance();
            }

            bool hasError = false;
            while (CurrentToken.Type != TokenType.RightBrace && !IsAtEnd() && !hasError)
            {
                if (CurrentToken.Type == TokenType.Identifier || CurrentToken.Type == TokenType.String)
                {
                    schema.Add(CurrentToken.Value);
                    Advance();

                    // Skip whitespace but not delimiter tabs
                    while (!IsAtEnd() && CurrentToken.Type == TokenType.Whitespace)
                    {
                        if (delimiter == Delimiter.Tab && CurrentToken.Value.Contains("\t"))
                            break;
                        Advance();
                    }

                    // Check for delimiter between field names
                    if (IsCurrentTokenDelimiter(delimiter))
                    {
                        Advance();

                        // Skip whitespace but not delimiter tabs
                        while (!IsAtEnd() && CurrentToken.Type == TokenType.Whitespace)
                        {
                            if (delimiter == Delimiter.Tab && CurrentToken.Value.Contains("\t"))
                                break;
                            Advance();
                        }
                    }
                    else if (CurrentToken.Type != TokenType.RightBrace)
                    {
                        RecordError(ParserErrorMessages.ExpectedDelimiterOrBrace(CurrentToken.Line), CurrentToken);
                        hasError = true;
                        // Try to recover by skipping to }
                        while (!IsAtEnd() && CurrentToken.Type != TokenType.RightBrace && CurrentToken.Type != TokenType.Colon)
                        {
                            Advance();
                        }
                    }
                }
                else
                {
                    RecordError(ParserErrorMessages.ExpectedFieldNameInSchemaAtLine(CurrentToken.Line), CurrentToken);
                    hasError = true;
                    // Try to recover by skipping to } or :
                    while (!IsAtEnd() && CurrentToken.Type != TokenType.RightBrace && CurrentToken.Type != TokenType.Colon)
                    {
                        Advance();
                    }
                }
            }

            if (CurrentToken.Type != TokenType.RightBrace)
            {
                RecordError(ParserErrorMessages.ExpectedRightBraceAtLine(CurrentToken.Line), CurrentToken);
                // Skip to } or : to recover
                while (!IsAtEnd() && CurrentToken.Type != TokenType.RightBrace && CurrentToken.Type != TokenType.Colon)
                {
                    Advance();
                }
            }

            if (CurrentToken.Type == TokenType.RightBrace)
            {
                Advance();
            }

            SkipWhitespace();

            return schema;
        }

        private ObjectNode ParseObject(int indentLevel)
        {
            var obj = new ObjectNode();
            var startToken = GetStartToken();

            SkipWhitespaceAndComments();

            while (!IsAtEnd())
            {
                // Stop if indentation falls below the object's indent level
                int currentIndent = GetCurrentIndentation();
                if (currentIndent < indentLevel)
                    break;

                // If we encounter a deeper indent than expected, treat it as nested content
                // and skip to avoid hard failures on minor tokenizer differences.
                if (currentIndent > indentLevel)
                {
                    // Consume the token line and continue
                    Advance();
                    SkipWhitespaceAndComments();
                    continue;
                }

                var property = ParseProperty(indentLevel);
                if (property != null)
                {
                    obj.Properties.Add(property);
                }

                SkipWhitespaceAndComments();
            }

            SetNodePositions(obj, startToken, GetEndToken());

            if (obj.Properties.Count > 0)
            {
                var lastProp = obj.Properties[obj.Properties.Count - 1];
                obj.EndLine = lastProp.EndLine;
                obj.EndColumn = lastProp.EndColumn;
                obj.EndPosition = lastProp.EndPosition;
            }

            return obj;
        }

        private TableArrayNode ParseTableArray(int size, List<string> schema, int indentLevel)
        {
            var table = new TableArrayNode
            {
                DeclaredSize = size
            };

            // Populate the schema
            foreach (var field in schema)
            {
                table.Schema.Add(field);
            }

            var startToken = GetStartToken();

            SkipWhitespaceAndComments();

            // Allow empty table arrays (e.g., empty[0]{id,name}:\n or EOF or dedent)
            if (IsAtEnd() || CurrentToken.Type == TokenType.EndOfFile ||
                CurrentToken.Type == TokenType.Dedent ||
                (CurrentToken.Type == TokenType.Newline && (PeekNextTokenType() == TokenType.EndOfFile || PeekNextTokenType() == TokenType.Dedent)))
            {
                SetNodePositions(table, startToken, GetEndToken());
                return table;
            }

            int lastPosition = -1;
            while (!IsAtEnd() && GetCurrentIndentation() >= indentLevel)
            {
                // Safety check: prevent infinite loops
                if (_position == lastPosition)
                {
                    RecordError(ParserErrorMessages.InfiniteLoopDetected(nameof(ParseTableArray)), CurrentToken);
                    Advance(); // Force progress
                }
                lastPosition = _position;

                // Skip comments, whitespace, and newlines before a row
                while (!IsAtEnd() &&
                       (CurrentToken.Type == TokenType.Comment ||
                        CurrentToken.Type == TokenType.Whitespace ||
                        CurrentToken.Type == TokenType.Newline))
                {
                    Advance();
                }

                // Check indentation after skipping
                if (IsAtEnd() || CurrentToken.Type == TokenType.EndOfFile ||
                    CurrentToken.Type == TokenType.Dedent ||
                    GetCurrentIndentation() < indentLevel)
                    break;

                // Only parse a row if the next token is a value (not a comment/whitespace)
                if (CurrentToken.Type.IsValueToken())
                {
                    var row = ParseTableRowFlexible(schema.Count);
                    table.Rows.Add(row);
                }
                else
                {
                    // Skip anything else
                    Advance();
                }
            }

            SetNodePositions(table, startToken, GetEndToken());

            // Validate table array row count (§6.1 compliance)
            if (table.Rows.Count != table.DeclaredSize)
            {
                string hint = ErrorHints.GetTableSizeMismatchHint(table.DeclaredSize, table.Rows.Count);
                RecordError(
                    $"Table array size mismatch: declared {table.DeclaredSize} rows, but found {table.Rows.Count}. {hint}",
                    startToken.Position,
                    GetEndToken().Position - startToken.Position,
                    startToken.Line,
                    startToken.Column,
                    ErrorCode.TableSizeMismatch
                );
            }

            return table;
        }

        // More flexible table row parser: allows multi-word unquoted values (e.g., Blue Lake Trail)
        private List<AstNode> ParseTableRowFlexible(int expectedFields)
        {
            var row = new List<AstNode>();
            Delimiter activeDelimiter = _delimiterStack.Peek();

            // Special case: single-field table array (no delimiter separators, just one value per row)
            if (expectedFields == 1)
            {
                var cellTokens = new List<Token>();
                while (!IsAtEnd() &&
                       CurrentToken.Type != TokenType.Newline &&
                       CurrentToken.Type != TokenType.Dedent &&
                       CurrentToken.Type != TokenType.Comment)
                {
                    if (CurrentToken.Type.IsValueToken())
                    {
                        cellTokens.Add(CurrentToken);
                        Advance();
                    }
                    else if (CurrentToken.Type == TokenType.Whitespace)
                    {
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }

                row.Add(CreateValueNodeFromTokens(cellTokens));
                return row;
            }

            // Multi-field rows: split by active delimiter
            for (int i = 0; i < expectedFields; i++)
            {
                // Gather tokens for this cell until delimiter or end of row
                var cellTokens = new List<Token>();
                while (!IsAtEnd() &&
                       !IsCurrentTokenDelimiter(activeDelimiter) &&
                       CurrentToken.Type != TokenType.Newline &&
                       CurrentToken.Type != TokenType.Dedent &&
                       (row.Count < expectedFields - 1 || (row.Count == expectedFields - 1 && CurrentToken.Type != TokenType.Comment)))
                {
                    if (CurrentToken.Type.IsValueToken())
                    {
                        cellTokens.Add(CurrentToken);
                        Advance();
                    }
                    else if (CurrentToken.Type == TokenType.Whitespace)
                    {
                        // Skip whitespace unless it's a tab delimiter
                        if (activeDelimiter == Delimiter.Tab && CurrentToken.Value.Contains("\t"))
                            break;
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }

                // Compose cell value using helper method
                row.Add(CreateValueNodeFromTokens(cellTokens));

                // After each cell, expect delimiter (except last cell)
                if (i < expectedFields - 1)
                {
                    if (IsCurrentTokenDelimiter(activeDelimiter))
                    {
                        Advance();
                        SkipWhitespaceExceptTabDelimiter();
                    }
                    else
                    {
                        RecordError(ParserErrorMessages.ExpectedDelimiter(activeDelimiter, CurrentToken.Line), CurrentToken);
                        // Stop parsing - row will be incomplete and validation can catch issues
                        break;
                    }
                }
            }
            return row;
        }

        private ArrayNode ParseInlineArray(int size)
        {
            var array = new ArrayNode { DeclaredSize = size };
            var startToken = CurrentToken;
            Delimiter activeDelimiter = _delimiterStack.Peek();

            for (int i = 0; i < size; i++)
            {
                try
                {
                    var value = ParseValue();
                    array.Elements.Add(value);
                }
                catch (ParseException ex)
                {
                    RecordError(ex.Message, ex.Position, ex.Length, ex.Line, ex.Column);
                    // Add null for missing value
                    array.Elements.Add(new NullValueNode());
                }

                if (i < size - 1)
                {
                    if (IsCurrentTokenDelimiter(activeDelimiter))
                    {
                        Advance();
                        // Skip whitespace but not delimiter tabs
                        while (!IsAtEnd() && CurrentToken.Type == TokenType.Whitespace)
                        {
                            if (activeDelimiter == Delimiter.Tab && CurrentToken.Value.Contains("\t"))
                                break;
                            Advance();
                        }
                    }
                    else
                    {
                        RecordError(ParserErrorMessages.ExpectedDelimiterAtPosition(CurrentToken.Line, CurrentToken.Column), CurrentToken);
                        // Stop parsing - let size validation catch the mismatch
                        break;
                    }
                }
            }

            SetNodePositions(array, startToken, GetEndToken());

            if (array.Elements.Count > 0 && array.Elements[array.Elements.Count - 1] != null)
            {
                var lastElement = array.Elements[array.Elements.Count - 1];
                array.EndLine = lastElement.EndLine;
                array.EndColumn = lastElement.EndColumn;
                array.EndPosition = lastElement.EndPosition;
            }

            // Validate array size (§6.1 compliance)
            if (array.Elements.Count != array.DeclaredSize)
            {
                string hint = ErrorHints.GetArraySizeMismatchHint(array.DeclaredSize, array.Elements.Count);
                RecordError(
                    $"Array size mismatch: declared {array.DeclaredSize} elements, but found {array.Elements.Count}. {hint}",
                    startToken.Position,
                    GetEndToken().Position - startToken.Position,
                    startToken.Line,
                    startToken.Column,
                    ErrorCode.ArraySizeMismatch
                );
            }

            return array;
        }

        private AstNode ParseValue()
        {
            // Skip any non-delimiter whitespace before the value
            // Only be delimiter-aware if we're inside an array scope
            if (_delimiterStack.Count > 1) // More than just the document delimiter
            {
                Delimiter activeDelimiter = _delimiterStack.Peek();
                while (!IsAtEnd() && CurrentToken.Type == TokenType.Whitespace)
                {
                    // Don't skip tabs if they're the active delimiter
                    if (activeDelimiter == Delimiter.Tab && CurrentToken.Value.Contains("\t"))
                        break;
                    Advance();
                }
            }
            else
            {
                // Normal whitespace skipping when not in an array
                while (!IsAtEnd() && CurrentToken.Type == TokenType.Whitespace)
                {
                    Advance();
                }
            }

            // Skip Indent/Dedent tokens
            while (!IsAtEnd() && (CurrentToken.Type == TokenType.Indent || CurrentToken.Type == TokenType.Dedent))
            {
                Advance();
            }

            if (IsAtEnd() || CurrentToken.Type == TokenType.EndOfFile)
            {
                RecordError(ParserErrorMessages.UnexpectedEndOfInput, CurrentToken, false, ErrorCode.UnexpectedEndOfInput);
                return new NullValueNode();
            }

            // For simple property values (not in arrays), collect all tokens until newline/comment
            // This allows multi-word unquoted strings like "New York" or "alice@example.com"
            if (_delimiterStack.Count == 1) // Only document delimiter (not in an array)
            {
                var valueTokens = new List<Token>();
                while (!IsAtEnd() &&
                       CurrentToken.Type != TokenType.Newline &&
                       CurrentToken.Type != TokenType.Comment &&
                       CurrentToken.Type != TokenType.EndOfFile)
                {
                    if (CurrentToken.Type.IsValueToken())
                    {
                        valueTokens.Add(CurrentToken);
                        Advance();
                    }
                    else if (CurrentToken.Type == TokenType.Whitespace)
                    {
                        Advance(); // Skip whitespace between tokens
                    }
                    else
                    {
                        break;
                    }
                }

                return CreateValueNodeFromTokens(valueTokens);
            }

            // For array values, use single-token parsing (existing behavior)
            var token = CurrentToken;

            switch (token.Type)
            {
                case TokenType.String:
                case TokenType.Identifier:
                    Advance();
                    return new StringValueNode
                    {
                        Value = token.Value,
                        RawValue = token.Value
                    }.WithPositionFrom(token);

                case TokenType.Number:
                    Advance();
                    string v = token.Value;
                    bool isInteger = !v.Contains(".") && !v.Contains("e") && !v.Contains("E");
                    double numValue = double.Parse(token.Value, CultureInfo.InvariantCulture);
                    return new NumberValueNode
                    {
                        Value = numValue,
                        IsInteger = isInteger,
                        RawValue = token.Value
                    }.WithPositionFrom(token);

                case TokenType.True:
                case TokenType.False:
                    Advance();
                    return new BooleanValueNode
                    {
                        Value = token.Type == TokenType.True,
                        RawValue = token.Value
                    }.WithPositionFrom(token);

                case TokenType.Null:
                    Advance();
                    return new NullValueNode
                    {
                        RawValue = token.Value
                    }.WithPositionFrom(token);

                default:
                    RecordError(string.Format(ParserErrorMessages.UnexpectedToken, token.Type), token, false, ErrorCode.UnexpectedToken);
                    Advance(); // Skip the unexpected token
                    return new NullValueNode();
            }
        }

        /// <summary>
        /// Skips tokens based on the specified options.
        /// </summary>
        private void Skip(SkipOptions options)
        {
            bool skipWhitespace = (options & SkipOptions.Whitespace) != 0;
            bool skipNewlines = (options & SkipOptions.Newlines) != 0;
            bool skipComments = (options & SkipOptions.Comments) != 0;
            bool skipIndentDedent = (options & SkipOptions.IndentDedent) != 0;
            bool skipInvalid = (options & SkipOptions.Invalid) != 0;
            bool respectTabDelimiter = (options & SkipOptions.RespectTabDelimiter) != 0;
            bool nonTabOnly = (options & SkipOptions.NonTabWhitespaceOnly) != 0;

            while (!IsAtEnd())
            {
                bool shouldSkip = CurrentToken.Type switch
                {
                    TokenType.Whitespace => ShouldSkipWhitespace(skipWhitespace, respectTabDelimiter, nonTabOnly),
                    TokenType.Newline => skipNewlines,
                    TokenType.Comment => skipComments,
                    TokenType.Indent or TokenType.Dedent => skipIndentDedent,
                    TokenType.Invalid => skipInvalid,
                    _ => false
                };

                if (!shouldSkip)
                    break;

                Advance();
            }
        }

        private bool ShouldSkipWhitespace(bool skipWhitespace, bool respectTabDelimiter, bool nonTabOnly)
        {
            if (!skipWhitespace)
                return false;

            // If we're respecting tab delimiters and inside an array with tab delimiter
            if (respectTabDelimiter && _delimiterStack.Count > 1)
            {
                Delimiter activeDelimiter = _delimiterStack.Peek();
                if (activeDelimiter == Delimiter.Tab && CurrentToken.Value == "\t")
                    return false;
            }

            // If non-tab only mode, don't skip tabs
            if (nonTabOnly && CurrentToken.Value.Contains("\t"))
                return false;

            return true;
        }

        // Convenience methods for common skip patterns
        private void SkipWhitespace() =>
            Skip(SkipOptions.Whitespace | SkipOptions.IndentDedent);

        private void SkipWhitespaceAndComments() =>
            Skip(SkipOptions.Whitespace | SkipOptions.Newlines | SkipOptions.Comments |
                 SkipOptions.IndentDedent | SkipOptions.Invalid);

        private void SkipNonDelimiterWhitespace() =>
            Skip(SkipOptions.Whitespace | SkipOptions.IndentDedent | SkipOptions.NonTabWhitespaceOnly);

        private void SkipWhitespaceExceptTabDelimiter() =>
            Skip(SkipOptions.Whitespace | SkipOptions.IndentDedent | SkipOptions.RespectTabDelimiter);

        private int GetCurrentIndentation()
        {
            if (IsAtEnd())
                return 0;

            int indent = 0;
            int pos = _position;

            // Look back to find the start of the line
            while (pos > 0 && _tokens[pos - 1].Type != TokenType.Newline)
            {
                pos--;
            }

            // Count spaces at the start of the line. Lexer may consume leading spaces
            // and not emit a Whitespace token, so fall back to using the Column value
            // of the first token on the line.
            if (pos < _tokens.Count && _tokens[pos].Type == TokenType.Whitespace)
            {
                indent = _tokens[pos].Value.Length;
            }
            else if (pos < _tokens.Count)
            {
                // Column numbers are 1-based; indent is column-1
                indent = Math.Max(0, _tokens[pos].Column - 1);
            }

            return indent;
        }

        // Helper to peek at the next token type
        private TokenType PeekNextTokenType()
        {
            int next = _position + 1;
            if (next < _tokens.Count)
                return _tokens[next].Type;
            return TokenType.EndOfFile;
        }

        /// <summary>
        /// Checks if the current token is the active delimiter.
        /// </summary>
        private bool IsCurrentTokenDelimiter(Delimiter delimiter)
        {
            if (IsAtEnd())
                return false;

            return delimiter switch
            {
                Delimiter.Comma => CurrentToken.Type == TokenType.Comma,
                Delimiter.Tab => CurrentToken.Type == TokenType.Whitespace && CurrentToken.Value == "\t",
                Delimiter.Pipe => CurrentToken.Type == TokenType.Pipe,
                _ => false,
            };
        }

        /// <summary>
        /// Sets position information on a node from start and end tokens.
        /// </summary>
        private void SetNodePositions<T>(T node, Token startToken, Token endToken) where T : AstNode
        {
            node.StartLine = startToken.Line;
            node.StartColumn = startToken.Column;
            node.StartPosition = startToken.Position;
            node.EndLine = endToken.Line;
            node.EndColumn = endToken.Column;
            node.EndPosition = endToken.Position;
        }

        /// <summary>
        /// Parses an expanded array (arrays of arrays or mixed arrays) with list items.
        /// </summary>
        private ArrayNode ParseExpandedArray(int size, int indentLevel)
        {
            var array = new ArrayNode { DeclaredSize = size };
            var startToken = GetStartToken();

            SkipWhitespaceAndComments();

            // Allow empty expanded arrays
            if (IsAtEnd() || CurrentToken.Type == TokenType.EndOfFile ||
                CurrentToken.Type == TokenType.Dedent ||
                (CurrentToken.Type == TokenType.Newline && (PeekNextTokenType() == TokenType.EndOfFile || PeekNextTokenType() == TokenType.Dedent)))
            {
                SetNodePositions(array, startToken, GetEndToken());
                return array;
            }

            int lastPosition = -1;
            while (!IsAtEnd() && GetCurrentIndentation() >= indentLevel)
            {
                // Safety check: prevent infinite loops
                if (_position == lastPosition)
                {
                    RecordError(ParserErrorMessages.InfiniteLoopDetected(nameof(ParseExpandedArray)), CurrentToken);
                    Advance(); // Force progress
                }
                lastPosition = _position;

                // Skip comments and whitespace before an item
                while (!IsAtEnd() &&
                       (CurrentToken.Type == TokenType.Comment ||
                        CurrentToken.Type == TokenType.Whitespace))
                {
                    Advance();
                }

                // Check for blank lines
                if (CurrentToken.Type == TokenType.Newline && GetCurrentIndentation() == indentLevel)
                {
                    int peek = _position + 1;
                    while (peek < _tokens.Count &&
                           (_tokens[peek].Type == TokenType.Newline ||
                            _tokens[peek].Type == TokenType.Whitespace ||
                            _tokens[peek].Type == TokenType.Comment))
                    {
                        peek++;
                    }
                    if (peek >= _tokens.Count ||
                        _tokens[peek].Type == TokenType.EndOfFile ||
                        _tokens[peek].Type == TokenType.Dedent ||
                        _tokens[peek].Type == TokenType.Identifier ||
                        _tokens[peek].Type == TokenType.String)
                    {
                        Advance();
                        break;
                    }
                    else
                    {
                        Advance();
                        continue;
                    }
                }

                if (IsAtEnd() || CurrentToken.Type == TokenType.EndOfFile ||
                    CurrentToken.Type == TokenType.Dedent ||
                    GetCurrentIndentation() < indentLevel)
                    break;

                // Parse list item: expect "- " prefix
                if (CurrentToken.Type == TokenType.Identifier && CurrentToken.Value == "-")
                {
                    Advance(); // consume the "-"
                    SkipWhitespace();

                    // Check for nested array header: "- [N]:"
                    if (CurrentToken.Type == TokenType.LeftBracket)
                    {
                        // Parse inline nested array with its own header
                        Advance(); // consume "["
                        SkipWhitespace();

                        int nestedSize = 0;
                        Delimiter? nestedDelimiter = null;

                        if (CurrentToken.Type == TokenType.Number)
                        {
                            nestedSize = (int)double.Parse(CurrentToken.Value, CultureInfo.InvariantCulture);
                            Advance();
                            SkipNonDelimiterWhitespace();
                        }

                        // Check for delimiter marker
                        if (CurrentToken.Type == TokenType.Whitespace && _position + 1 < _tokens.Count &&
                            _tokens[_position + 1].Type == TokenType.RightBracket)
                        {
                            if (CurrentToken.Value == "\t")
                            {
                                nestedDelimiter = Delimiter.Tab;
                                Advance();
                            }
                        }
                        else if (CurrentToken.Type == TokenType.Pipe)
                        {
                            nestedDelimiter = Delimiter.Pipe;
                            Advance();
                        }

                        if (CurrentToken.Type != TokenType.RightBracket)
                        {
                            RecordError(ParserErrorMessages.ExpectedRightBracketAtLine(CurrentToken.Line), CurrentToken);
                            // Skip to next colon or newline
                            while (!IsAtEnd() && CurrentToken.Type != TokenType.Colon && CurrentToken.Type != TokenType.Newline)
                            {
                                if (CurrentToken.Type == TokenType.RightBracket)
                                {
                                    Advance();
                                    break;
                                }
                                Advance();
                            }
                        }
                        else
                        {
                            Advance();
                        }
                        SkipWhitespace();

                        if (CurrentToken.Type != TokenType.Colon)
                        {
                            RecordError(ParserErrorMessages.ExpectedColonAfterArrayHeader(CurrentToken.Line), CurrentToken);
                            // Skip the malformed array item
                            while (!IsAtEnd() && CurrentToken.Type != TokenType.Newline)
                            {
                                Advance();
                            }
                            continue;
                        }
                        Advance();
                        SkipWhitespace();

                        // Push the nested array's delimiter to the stack
                        Delimiter activeDelimiter = nestedDelimiter ?? Delimiter.Comma;
                        _delimiterStack.Push(activeDelimiter);

                        try
                        {
                            // Parse the nested inline array
                            var nestedArray = new ArrayNode { DeclaredSize = nestedSize };
                            for (int i = 0; i < nestedSize; i++)
                            {
                                var value = ParseValue();
                                nestedArray.Elements.Add(value);

                                if (i < nestedSize - 1)
                                {
                                    if (IsCurrentTokenDelimiter(activeDelimiter))
                                    {
                                        Advance();
                                        // Skip whitespace but not delimiter tabs
                                        while (!IsAtEnd() && CurrentToken.Type == TokenType.Whitespace)
                                        {
                                            if (activeDelimiter == Delimiter.Tab && CurrentToken.Value.Contains("\t"))
                                                break;
                                            Advance();
                                        }
                                    }
                                    else
                                    {
                                        RecordError(ParserErrorMessages.ExpectedDelimiterAtPosition(CurrentToken.Line, CurrentToken.Column), CurrentToken);
                                        // Stop parsing - let size validation catch the mismatch
                                        break;
                                    }
                                }
                            }

                            array.Elements.Add(nestedArray);
                        }
                        finally
                        {
                            _delimiterStack.Pop();
                        }
                    }
                    else
                    {
                        // Parse a primitive or object value
                        var value = ParseValue();
                        array.Elements.Add(value);
                    }

                    // Consume end of list item line
                    SkipWhitespaceAndComments();
                }
                else
                {
                    // Skip anything else
                    Advance();
                }
            }

            SetNodePositions(array, startToken, GetEndToken());

            // Validate array size (§6.1 compliance)
            if (array.Elements.Count != array.DeclaredSize)
            {
                string hint = ErrorHints.GetArraySizeMismatchHint(array.DeclaredSize, array.Elements.Count);
                RecordError(
                    $"Array size mismatch: declared {array.DeclaredSize} elements, but found {array.Elements.Count}. {hint}",
                    startToken.Position,
                    GetEndToken().Position - startToken.Position,
                    startToken.Line,
                    startToken.Column,
                    ErrorCode.ArraySizeMismatch
                );
            }

            return array;
        }

        private Token CurrentToken => _position < _tokens.Count ? _tokens[_position] : _tokens[_tokens.Count - 1];

        private bool IsAtEnd() => _position >= _tokens.Count || CurrentToken.Type == TokenType.EndOfFile;

        private void Advance() => _position++;

        /// <summary>
        /// Creates an AST value node from a list of tokens.
        /// Handles single tokens, multiple tokens (joined as string), and empty token lists.
        /// </summary>
        private AstNode CreateValueNodeFromTokens(List<Token> tokens)
        {
            if (tokens.Count == 0)
            {
                return new NullValueNode();
            }

            if (tokens.Count == 1)
            {
                return CreateValueNodeFromSingleToken(tokens[0]);
            }

            // Multi-token: join as string (space-separated) without LINQ
            // Pre-size StringBuilder based on token count
            var sb = new System.Text.StringBuilder(tokens.Count * AverageTokenLength);
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(tokens[i].Value);
            }
            var joined = sb.ToString();
            var first = tokens[0];
            var last = tokens[tokens.Count - 1];

            return new StringValueNode
            {
                Value = joined,
                RawValue = joined
            }.WithPositionFromRange(first, last);
        }

        /// <summary>
        /// Creates an AST value node from a single token.
        /// </summary>
        private AstNode CreateValueNodeFromSingleToken(Token token)
        {
            return token.Type switch
            {
                TokenType.String or TokenType.Identifier => new StringValueNode
                {
                    Value = token.Value,
                    RawValue = token.Value
                }.WithPositionFrom(token),
                TokenType.Number => new NumberValueNode
                {
                    Value = double.Parse(token.Value, CultureInfo.InvariantCulture),
                    IsInteger = !token.Value.Contains(".") && !token.Value.Contains("e") && !token.Value.Contains("E"),
                    RawValue = token.Value
                }.WithPositionFrom(token),
                TokenType.True or TokenType.False => new BooleanValueNode
                {
                    Value = token.Type == TokenType.True,
                    RawValue = token.Value
                }.WithPositionFrom(token),
                TokenType.Null => new NullValueNode
                {
                    RawValue = token.Value
                }.WithPositionFrom(token),
                _ => new StringValueNode
                {
                    Value = token.Value,
                    RawValue = token.Value
                }.WithPositionFrom(token),
            };
        }
    }

    public class ParseException : Exception
    {
        /// <summary>
        /// The starting position (0-based index) of the error in the source string.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// The length of the span that contains the error.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The line number (1-based) where the error occurs.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// The column number (1-based) where the error occurs.
        /// </summary>
        public int Column { get; }

        public ParseException(string message) : base(message)
        {
            Position = 0;
            Length = 0;
            Line = 0;
            Column = 0;
        }

        public ParseException(string message, int position, int length, int line, int column) : base($"{message} at line {line}, column {column}")
        {
            Position = position;
            Length = length;
            Line = line;
            Column = column;
        }

        public ParseException(string message, Token token) : base($"{message} at line {token.Line}, column {token.Column}")
        {
            Position = token.Position;
            Length = token.Value?.Length ?? 1;
            Line = token.Line;
            Column = token.Column;
        }
    }
}
