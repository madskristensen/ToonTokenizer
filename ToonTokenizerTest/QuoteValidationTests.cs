using ToonTokenizer;

namespace ToonTokenizerTest
{
    /// <summary>
    /// Tests for quote validation per TOON spec §7.1 and §7.2.
    /// Spec §7.1: Decoders MUST reject unterminated strings and invalid escape sequences.
    /// </summary>
    [TestClass]
    public class QuoteValidationTests
    {
        [TestMethod]
        public void Parse_UnterminatedDoubleQuotedString_ThrowsParseException()
        {
            // Spec §7.1: "Decoders MUST reject ... unterminated strings"
            var source = "name: \"John";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Unterminated string", exception.Message);
        }

        [TestMethod]
        public void Parse_UnterminatedSingleQuotedString_ThrowsParseException()
        {
            // Spec §7.1: "Decoders MUST reject ... unterminated strings"
            var source = "name: 'John";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Unterminated string", exception.Message);
        }

        [TestMethod]
        public void Parse_EmptyQuotedString_ParsesCorrectly()
        {
            // Empty quoted strings are valid
            var source = "name: \"\"";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(1, result.Document!.Properties);
            var value = (ToonTokenizer.Ast.StringValueNode)result.Document!.Properties[0].Value;
            Assert.AreEqual("", value.Value);
        }

        [TestMethod]
        public void Parse_ProperlyQuotedStringWithEscapedQuote_ParsesCorrectly()
        {
            // Spec §7.1: \" is a valid escape sequence
            var source = "quote: \"He said \\\"Hello\\\"\"";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var value = (ToonTokenizer.Ast.StringValueNode)result.Document!.Properties[0].Value;
            Assert.AreEqual("He said \"Hello\"", value.Value);
        }

        [TestMethod]
        public void Parse_StringWithEscapedBackslash_ParsesCorrectly()
        {
            // Spec §7.1: \\ is a valid escape sequence
            var source = "path: \"C:\\\\Users\\\\Name\"";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var value = (ToonTokenizer.Ast.StringValueNode)result.Document!.Properties[0].Value;
            Assert.AreEqual("C:\\Users\\Name", value.Value);
        }

        [TestMethod]
        public void Parse_StringWithValidEscapes_ParsesCorrectly()
        {
            // Spec §7.1: Only \\, \", \n, \r, \t are valid escapes
            var source = "text: \"Line1\\nLine2\\tTabbed\\r\\nCRLF\\\\Backslash\\\"Quote\"";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var value = (ToonTokenizer.Ast.StringValueNode)result.Document!.Properties[0].Value;
            Assert.Contains("\n", value.Value);
            Assert.Contains("\t", value.Value);
            Assert.Contains("\r", value.Value);
            Assert.Contains("\\", value.Value);
            Assert.Contains("\"", value.Value);
        }

        [TestMethod]
        public void Parse_StringWithInvalidEscape_ThrowsParseException()
        {
            // Spec §7.1: "Decoders MUST reject any other escape sequence"
            var source = "text: \"Hello\\xWorld\"";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Invalid escape sequence", exception.Message);
            Assert.Contains("\\x", exception.Message);
        }

        [TestMethod]
        public void Parse_StringWithInvalidEscapeU_ThrowsParseException()
        {
            // Spec §7.1: \u is not a valid escape sequence
            var source = "text: \"Unicode\\u0041\"";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Invalid escape sequence", exception.Message);
        }

        [TestMethod]
        public void Parse_StringWithInvalidEscapeF_ThrowsParseException()
        {
            // Spec §7.1: \f is not a valid escape sequence
            var source = "text: \"Form\\fFeed\"";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Invalid escape sequence", exception.Message);
        }

        [TestMethod]
        public void Parse_StringWithInvalidEscapeB_ThrowsParseException()
        {
            // Spec §7.1: \b is not a valid escape sequence
            var source = "text: \"Back\\bspace\"";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Invalid escape sequence", exception.Message);
        }

        [TestMethod]
        public void Parse_UnquotedStringCannotContainQuote()
        {
            // Per spec §7.2: unquoted strings cannot contain double quotes
            // If a bare quote appears, the lexer will try to parse it as a quoted string
            // Since it's unterminated, it will throw
            var source = "value: hello\"world";
            
            // The bare quote in the middle triggers unterminated string error
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Unterminated string", exception.Message);
        }

        [TestMethod]
        public void Parse_SingleQuotedStringWithEscapedSingleQuote_ParsesCorrectly()
        {
            // Single quotes can be escaped within single-quoted strings
            var source = "text: 'It\\'s working'";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var value = (ToonTokenizer.Ast.StringValueNode)result.Document!.Properties[0].Value;
            Assert.AreEqual("It's working", value.Value);
        }

        [TestMethod]
        public void Parse_DoubleQuotedStringWithSingleQuote_ParsesCorrectly()
        {
            // Single quotes don't need escaping in double-quoted strings
            var source = "text: \"It's working\"";
            var result = Toon.Parse(source);
            
            Assert.IsTrue(result.IsSuccess);
            var value = (ToonTokenizer.Ast.StringValueNode)result.Document!.Properties[0].Value;
            Assert.AreEqual("It's working", value.Value);
        }

        [TestMethod]
        public void Parse_MultilineUnterminatedString_ThrowsParseException()
        {
            // Unterminated string that spans multiple lines (hits EOF)
            var source = @"name: ""John
age: 30";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Unterminated string", exception.Message);
        }

        [TestMethod]
        public void Parse_UnterminatedStringInArrayValue_ThrowsParseException()
        {
            // Unterminated string in an array context
            var source = "items[2]: \"value1,value2";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Unterminated string", exception.Message);
        }

        [TestMethod]
        public void Parse_UnterminatedStringInTableArrayCell_ThrowsParseException()
        {
            // Unterminated string in table array cell
            var source = @"data[1]{id,name}:
  1,""Alice";
            
            var exception = Assert.ThrowsExactly<ParseException>(() => Toon.Parse(source));
            Assert.Contains("Unterminated string", exception.Message);
        }

        [TestMethod]
        public void Parse_DoubleQuoteInSingleQuotedString_ParsesCorrectly()
        {
            // Double quotes don't need escaping in single-quoted strings
            // The \" sequence in single quotes is treated as literal backslash + quote
            var source = "text: 'He said \"Hello\"'";
            
            // This should parse successfully - double quotes are just regular chars in single-quoted strings
            var result = Toon.Parse(source);
            Assert.IsTrue(result.IsSuccess);
            var value = (ToonTokenizer.Ast.StringValueNode)result.Document!.Properties[0].Value;
            Assert.AreEqual("He said \"Hello\"", value.Value);
        }
    }
}
