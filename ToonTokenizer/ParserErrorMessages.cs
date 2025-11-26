namespace ToonTokenizer
{
    /// <summary>
    /// Centralized error messages for the TOON parser.
    /// Provides clear, actionable error messages to help users fix TOON syntax issues.
    /// </summary>
    internal static class ParserErrorMessages
    {
        // Basic structural errors with guidance
        public const string ExpectedRightBracket = "Expected ']' to close array size declaration. Example: items[3]: value1,value2,value3";
        public const string ExpectedRightBrace = "Expected '}' to close schema definition. Example: users{id,name}: ...";
        public const string ExpectedColonAfterKey = "Expected ':' after property key. Every property must have a colon separator: key: value";
        public const string ExpectedPropertyKey = "Expected property key (identifier or quoted string) at start of line";
        public const string ExpectedFieldNameInSchema = "Expected field name in schema definition. Schema format: {field1,field2,field3}";
        public const string UnexpectedEndOfInput = "Unexpected end of input while parsing value. Check for incomplete array or missing value after ':'";
        public const string UnexpectedToken = "Unexpected token {0} while parsing value. Expected: string, number, true, false, null, or nested structure";
        
        // Parameterized messages with context
        public static string ExpectedDelimiterOrBrace(int line) => 
            $"Expected delimiter or '}}' at line {line}. Schema fields must be separated by the array's delimiter (comma, tab, or pipe)";
        
        public static string ExpectedDelimiter(Delimiter delimiter, int line) => 
            $"Expected {GetDelimiterDescription(delimiter)} at line {line}. Ensure all elements in this array use the same delimiter";
        
        public static string ExpectedDelimiterAtPosition(int line, int column) => 
            $"Expected delimiter at line {line}, column {column}. Array elements must be separated by the declared delimiter";
        
        public static string ExpectedRightBracketAtLine(int line) => 
            $"Expected ']' at line {line}. Array size declaration must be closed before the colon. Example: items[5]: ...";
        
        public static string ExpectedColonAfterArrayHeader(int line) => 
            $"Expected ':' after array header at line {line}. Format: arrayName[size]: elements OR arrayName[size]{{schema}}: rows";
        
        public static string ExpectedRightBraceAtLine(int line) => 
            $"Expected '}}' at line {line}. Schema definition must be closed. Example: users[2]{{id,name}}: ...";
        
        public static string ExpectedFieldNameInSchemaAtLine(int line) => 
            $"Expected field name in schema at line {line}. Schema fields must be identifiers or quoted strings separated by delimiters";
        
        public static string InfiniteLoopDetected(string methodName) => 
            $"Parser stuck in infinite loop in {methodName}. This is a parser bug - please report with your TOON input";
        
        // Helper method for delimiter descriptions
        private static string GetDelimiterDescription(Delimiter delimiter) => delimiter switch
        {
            Delimiter.Comma => "comma (,) delimiter",
            Delimiter.Tab => "tab (\\t) delimiter",
            Delimiter.Pipe => "pipe (|) delimiter",
            _ => "delimiter"
        };
    }
}
