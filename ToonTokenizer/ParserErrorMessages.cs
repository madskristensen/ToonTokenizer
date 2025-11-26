namespace ToonTokenizer
{
    /// <summary>
    /// Centralized error messages for the TOON parser.
    /// </summary>
    internal static class ParserErrorMessages
    {
        public const string ExpectedRightBracket = "Expected ']'";
        public const string ExpectedRightBrace = "Expected '}'";
        public const string ExpectedColonAfterKey = "Expected ':' after property key";
        public const string ExpectedPropertyKey = "Expected property key";
        public const string ExpectedFieldNameInSchema = "Expected field name in schema";
        public const string UnexpectedEndOfInput = "Unexpected end of input while parsing value";
        public const string UnexpectedToken = "Unexpected token {0} while parsing value";
        
        // Parameterized messages
        public static string ExpectedDelimiterOrBrace(int line) => 
            $"Expected delimiter or '}}' at line {line}";
        
        public static string ExpectedDelimiter(Delimiter delimiter, int line) => 
            $"Expected {delimiter} delimiter between values at line {line}";
        
        public static string ExpectedDelimiterAtPosition(int line, int column) => 
            $"Expected delimiter at line {line}, column {column}";
        
        public static string ExpectedRightBracketAtLine(int line) => 
            $"Expected ']' at line {line}";
        
        public static string ExpectedColonAfterArrayHeader(int line) => 
            $"Expected ':' after array header at line {line}";
        
        public static string ExpectedRightBraceAtLine(int line) => 
            $"Expected '}}' at line {line}";
        
        public static string ExpectedFieldNameInSchemaAtLine(int line) => 
            $"Expected field name in schema at line {line}";
        
        public static string InfiniteLoopDetected(string methodName) => 
            $"Parser stuck in infinite loop in {methodName}";
    }
}
