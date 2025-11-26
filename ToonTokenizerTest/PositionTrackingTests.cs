using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest
{
    [TestClass]
    public class PositionTrackingTests
    {
        [TestMethod]
        public void Token_HasCorrectLineNumber()
        {
            var source = "name: John";
            var tokens = Toon.Tokenize(source);

            foreach (var token in tokens.Where(t => t.Type != TokenType.EndOfFile))
            {
                Assert.AreEqual(1, token.Line);
            }
        }

        [TestMethod]
        public void Token_HasCorrectColumnNumber()
        {
            var source = "name: John";
            var tokens = Toon.Tokenize(source);

            var nameToken = tokens.First(t => t.Type == TokenType.Identifier);
            Assert.AreEqual(1, nameToken.Column);

            var colonToken = tokens.First(t => t.Type == TokenType.Colon);
            Assert.IsGreaterThan(nameToken.Column, colonToken.Column);
        }

        [TestMethod]
        public void Token_HasCorrectPosition()
        {
            var source = "name: John";
            var tokens = Toon.Tokenize(source);

            var nameToken = tokens.First(t => t.Type == TokenType.Identifier);
            Assert.AreEqual(0, nameToken.Position); // First character

            var colonToken = tokens.First(t => t.Type == TokenType.Colon);
            Assert.AreEqual(4, colonToken.Position); // After "name"
        }

        [TestMethod]
        public void Token_HasCorrectLength()
        {
            var source = "name: John";
            var tokens = Toon.Tokenize(source);

            var nameToken = tokens.First(t => t.Type == TokenType.Identifier);
            Assert.AreEqual(4, nameToken.Length); // "name" is 4 characters

            var colonToken = tokens.First(t => t.Type == TokenType.Colon);
            Assert.AreEqual(1, colonToken.Length); // ":" is 1 character
        }

        [TestMethod]
        public void Token_TracksPositionAcrossLines()
        {
            var source = @"line1: value1
line2: value2";
            var tokens = Toon.Tokenize(source);

            var line1Tokens = tokens.Where(t => t.Line == 1 && t.Type != TokenType.Newline).ToList();
            var line2Tokens = tokens.Where(t => t.Line == 2).ToList();

            Assert.IsNotEmpty(line1Tokens);
            Assert.IsNotEmpty(line2Tokens);

            // Line 2 tokens should have column numbers starting from 1
            var firstLine2Token = line2Tokens.First(t => t.Type != TokenType.Whitespace);
            Assert.AreEqual(1, firstLine2Token.Column);
        }

        [TestMethod]
        public void Token_MultilineTracksCorrectly()
        {
            var source = @"prop1: val1
prop2: val2
prop3: val3";
            var tokens = Toon.Tokenize(source);

            var prop1 = tokens.First(t => t.Value == "prop1");
            var prop2 = tokens.First(t => t.Value == "prop2");
            var prop3 = tokens.First(t => t.Value == "prop3");

            Assert.AreEqual(1, prop1.Line);
            Assert.AreEqual(2, prop2.Line);
            Assert.AreEqual(3, prop3.Line);
        }

        [TestMethod]
        public void AstNode_HasCorrectStartPosition()
        {
            var source = "name: John";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsGreaterThan(0, property.StartLine);
            Assert.IsGreaterThan(0, property.StartColumn);
            Assert.IsGreaterThanOrEqualTo(0, property.StartPosition);
        }

        [TestMethod]
        public void AstNode_HasCorrectEndPosition()
        {
            var source = "name: John";
            var result = Toon.Parse(source);

            var property = result.Document!.Properties[0];
            Assert.IsGreaterThan(0, property.EndLine);
            Assert.IsGreaterThan(0, property.EndColumn);
            Assert.IsGreaterThanOrEqualTo(property.StartPosition, property.EndPosition);
        }

        [TestMethod]
        public void AstNode_NestedObjectPositions_AreCorrect()
        {
            var source = @"user:
  name: John";
            var result = Toon.Parse(source);

            var userProp = result.Document!.Properties[0];
            var userObj = (ObjectNode)userProp.Value;
            var nameProp = userObj.Properties[0];

            Assert.AreEqual(1, userProp.StartLine);
            Assert.AreEqual(2, nameProp.StartLine);
        }

        [TestMethod]
        public void ValueNode_HasCorrectPosition()
        {
            var source = "count: 42";
            var result = Toon.Parse(source);

            var value = (NumberValueNode)result.Document!.Properties[0].Value;
            Assert.IsGreaterThan(0, value.StartLine);
            Assert.IsGreaterThan(0, value.StartColumn);
            Assert.IsGreaterThanOrEqualTo(0, value.StartPosition);
        }

        [TestMethod]
        public void ArrayNode_HasCorrectPosition()
        {
            var source = "items[3]: a,b,c";
            var result = Toon.Parse(source);

            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.IsGreaterThan(0, array.StartLine);
            Assert.IsGreaterThanOrEqualTo(0, array.StartPosition);
        }

        [TestMethod]
        public void TableArrayNode_HasCorrectPosition()
        {
            var source = @"data[2]{id,name}:
  1,Alice
  2,Bob";
            var result = Toon.Parse(source);

            var table = (TableArrayNode)result.Document!.Properties[0].Value;
            Assert.AreEqual(1, table.StartLine);
            Assert.IsGreaterThanOrEqualTo(table.StartLine, table.EndLine);
        }

        [TestMethod]
        public void GetTokenAt_ReturnsCorrectToken()
        {
            var source = "name: John";
            var tokens = Toon.Tokenize(source);

            // Token at position of "name" (line 1, column 1-4)
            var token = tokens.GetTokenAt(1, 2);
            Assert.IsNotNull(token);
            Assert.AreEqual(TokenType.Identifier, token.Type);
        }

        [TestMethod]
        public void GetTokensOnLine_ReturnsCorrectTokens()
        {
            var source = @"line1: value1
line2: value2";
            var tokens = Toon.Tokenize(source);

            var line1Tokens = tokens.GetTokensOnLine(1);
            var line2Tokens = tokens.GetTokensOnLine(2);

            Assert.IsNotEmpty(line1Tokens);
            Assert.IsNotEmpty(line2Tokens);
            Assert.IsTrue(line1Tokens.All(t => t.Line == 1));
            Assert.IsTrue(line2Tokens.All(t => t.Line == 2));
        }

        [TestMethod]
        public void GetTokensInRange_ReturnsCorrectTokens()
        {
            var source = @"line1: value1
line2: value2
line3: value3";
            var tokens = Toon.Tokenize(source);

            var rangeTokens = tokens.GetTokensInRange(1, 2);

            Assert.IsTrue(rangeTokens.All(t => t.Line >= 1 && t.Line <= 2));
            Assert.IsFalse(rangeTokens.Any(t => t.Line == 3));
        }

        [TestMethod]
        public void GetTokensByType_ReturnsCorrectTokens()
        {
            var source = "name: John, age: 30";
            var tokens = Toon.Tokenize(source);

            var identifiers = tokens.GetTokensByType(TokenType.Identifier);
            var colons = tokens.GetTokensByType(TokenType.Colon);

            Assert.IsGreaterThanOrEqualTo(2, identifiers.Count); // "name" and "age"
            Assert.HasCount(2, colons);
        }

        [TestMethod]
        public void Token_IsKeyword_ReturnsCorrectValue()
        {
            var lexer = new ToonLexer("true false null");
            var tokens = lexer.Tokenize();

            var trueToken = tokens.First(t => t.Type == TokenType.True);
            var falseToken = tokens.First(t => t.Type == TokenType.False);
            var nullToken = tokens.First(t => t.Type == TokenType.Null);

            Assert.IsTrue(trueToken.IsKeyword());
            Assert.IsTrue(falseToken.IsKeyword());
            Assert.IsTrue(nullToken.IsKeyword());
        }

        [TestMethod]
        public void Token_IsStructural_ReturnsCorrectValue()
        {
            var lexer = new ToonLexer(":[]{},");
            var tokens = lexer.Tokenize();

            var structuralTokens = tokens.Where(t => t.Type != TokenType.EndOfFile).ToList();

            foreach (var token in structuralTokens)
            {
                Assert.IsTrue(token.IsStructural());
            }
        }

        [TestMethod]
        public void Token_IsValue_ReturnsCorrectValue()
        {
            var source = "count: 42, active: true, name: \"John\", empty: null";
            var tokens = Toon.Tokenize(source);

            var valueTokens = tokens.Where(t => t.IsValue()).ToList();

            Assert.IsGreaterThanOrEqualTo(4, valueTokens.Count); // 42, true, "John", null
        }

        [TestMethod]
        public void Document_PositionSpans_CoverAllContent()
        {
            var source = @"name: John
age: 30";
            var result = Toon.Parse(source);

            // Document should span from first to last property
            Assert.AreEqual(1, result.Document!.StartLine);
            Assert.AreEqual(2, result.Document!.EndLine);
        }

        [TestMethod]
        public void ComplexStructure_AllNodesHavePositions()
        {
            var source = @"root:
  array[2]: a,b
  table[1]{x,y}:
    1,2";
            var result = Toon.Parse(source);

            // Recursively check all nodes have positions
            CheckNodePositions(result.Document!);
        }

        private static void CheckNodePositions(AstNode node)
        {
            Assert.IsGreaterThan(0, node.StartLine, "Node StartLine should be positive");
            Assert.IsGreaterThan(0, node.StartColumn, "Node StartColumn should be positive");
            Assert.IsGreaterThanOrEqualTo(0, node.StartPosition, "Node StartPosition should be non-negative");

            if (node is ToonDocument doc)
            {
                foreach (var prop in doc.Properties)
                    CheckNodePositions(prop);
            }
            else if (node is PropertyNode prop)
            {
                CheckNodePositions(prop.Value);
            }
            else if (node is ObjectNode obj)
            {
                foreach (var p in obj.Properties)
                    CheckNodePositions(p);
            }
            else if (node is ArrayNode arr)
            {
                foreach (var elem in arr.Elements)
                    CheckNodePositions(elem);
            }
            else if (node is TableArrayNode table)
            {
                foreach (var row in table.Rows)
                    foreach (var val in row)
                        CheckNodePositions(val);
            }
        }

        [TestMethod]
        public void Token_Equality_WorksCorrectly()
        {
            var token1 = new Token(TokenType.String, "test", 1, 1, 0, 4);
            var token2 = new Token(TokenType.String, "test", 1, 1, 0, 4);
            var token3 = new Token(TokenType.String, "other", 1, 1, 0, 5);

            Assert.AreEqual(token1, token2);
            Assert.AreNotEqual(token1, token3);
        }

        [TestMethod]
        public void Token_ToString_ReturnsUsefulInfo()
        {
            var token = new Token(TokenType.String, "test", 1, 5, 4, 4);
            var str = token.ToString();

            Assert.Contains("String", str);
            Assert.Contains("test", str);
            Assert.Contains("1", str);
            Assert.Contains("5", str);
        }
    }
}
