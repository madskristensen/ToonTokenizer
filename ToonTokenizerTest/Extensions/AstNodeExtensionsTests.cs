using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Extensions
{
    /// <summary>
    /// Tests for AstNodeExtensions methods that provide fluent position setting for AST nodes.
    /// </summary>
    [TestClass]
    public class AstNodeExtensionsTests
    {
        #region WithPositionFrom Tests

        [TestMethod]
        public void WithPositionFrom_StringValueNode_SetsPositionCorrectly()
        {
            var token = new Token(TokenType.String, "test", 2, 5, 10, 4);
            var node = new StringValueNode { Value = "test" };

            var result = node.WithPositionFrom(token);

            Assert.AreSame(node, result); // Method chaining
            Assert.AreEqual(2, node.StartLine);
            Assert.AreEqual(5, node.StartColumn);
            Assert.AreEqual(10, node.StartPosition);
            Assert.AreEqual(2, node.EndLine);
            Assert.AreEqual(9, node.EndColumn); // 5 + 4
            Assert.AreEqual(14, node.EndPosition); // 10 + 4
        }

        [TestMethod]
        public void WithPositionFrom_NumberValueNode_SetsPositionCorrectly()
        {
            var token = new Token(TokenType.Number, "42", 3, 10, 20, 2);
            var node = new NumberValueNode { Value = 42, IsInteger = true };

            var result = node.WithPositionFrom(token);

            Assert.AreSame(node, result);
            Assert.AreEqual(3, node.StartLine);
            Assert.AreEqual(10, node.StartColumn);
            Assert.AreEqual(20, node.StartPosition);
            Assert.AreEqual(3, node.EndLine);
            Assert.AreEqual(12, node.EndColumn); // 10 + 2
            Assert.AreEqual(22, node.EndPosition); // 20 + 2
        }

        [TestMethod]
        public void WithPositionFrom_BooleanValueNode_SetsPositionCorrectly()
        {
            var token = new Token(TokenType.True, "true", 4, 15, 30, 4);
            var node = new BooleanValueNode { Value = true };

            var result = node.WithPositionFrom(token);

            Assert.AreSame(node, result);
            Assert.AreEqual(4, node.StartLine);
            Assert.AreEqual(15, node.StartColumn);
            Assert.AreEqual(30, node.StartPosition);
            Assert.AreEqual(4, node.EndLine);
            Assert.AreEqual(19, node.EndColumn); // 15 + 4
            Assert.AreEqual(34, node.EndPosition); // 30 + 4
        }

        [TestMethod]
        public void WithPositionFrom_NullValueNode_SetsPositionCorrectly()
        {
            var token = new Token(TokenType.Null, "null", 5, 20, 40, 4);
            var node = new NullValueNode();

            var result = node.WithPositionFrom(token);

            Assert.AreSame(node, result);
            Assert.AreEqual(5, node.StartLine);
            Assert.AreEqual(20, node.StartColumn);
            Assert.AreEqual(40, node.StartPosition);
            Assert.AreEqual(5, node.EndLine);
            Assert.AreEqual(24, node.EndColumn); // 20 + 4
            Assert.AreEqual(44, node.EndPosition); // 40 + 4
        }

        [TestMethod]
        public void WithPositionFrom_SupportsMethodChaining()
        {
            var token = new Token(TokenType.String, "value", 1, 1, 0, 5);
            var node = new StringValueNode { Value = "test" }
                .WithPositionFrom(token);

            Assert.IsNotNull(node);
            Assert.AreEqual("test", node.Value);
            Assert.AreEqual(1, node.StartLine);
        }

        [TestMethod]
        public void WithPositionFrom_PositionZero_HandlesCorrectly()
        {
            var token = new Token(TokenType.String, "first", 1, 1, 0, 5);
            var node = new StringValueNode { Value = "first" };

            node.WithPositionFrom(token);

            Assert.AreEqual(0, node.StartPosition);
            Assert.AreEqual(5, node.EndPosition); // 0 + 5
            Assert.AreEqual(1, node.StartLine);
        }

        #endregion

        #region WithPositionFromRange Tests

        [TestMethod]
        public void WithPositionFromRange_SetsStartAndEndCorrectly()
        {
            var startToken = new Token(TokenType.String, "start", 2, 5, 10, 5);
            var endToken = new Token(TokenType.String, "end", 2, 45, 50, 3);
            var node = new StringValueNode { Value = "test" };

            var result = node.WithPositionFromRange(startToken, endToken);

            Assert.AreSame(node, result);
            Assert.AreEqual(2, node.StartLine);
            Assert.AreEqual(5, node.StartColumn);
            Assert.AreEqual(10, node.StartPosition);
            Assert.AreEqual(2, node.EndLine);
            Assert.AreEqual(48, node.EndColumn); // 45 + 3
            Assert.AreEqual(53, node.EndPosition); // 50 + 3
        }

        [TestMethod]
        public void WithPositionFromRange_MultiLine_HandlesCorrectly()
        {
            var startToken = new Token(TokenType.String, "start", 2, 5, 10, 5);
            var endToken = new Token(TokenType.String, "end", 5, 10, 100, 3);
            var node = new ArrayNode();

            node.WithPositionFromRange(startToken, endToken);

            Assert.AreEqual(2, node.StartLine);
            Assert.AreEqual(5, node.StartColumn);
            Assert.AreEqual(5, node.EndLine);
            Assert.AreEqual(13, node.EndColumn); // 10 + 3
        }

        [TestMethod]
        public void WithPositionFromRange_SupportsMethodChaining()
        {
            var startToken = new Token(TokenType.LeftBracket, "[", 1, 1, 0, 1);
            var endToken = new Token(TokenType.RightBracket, "]", 1, 11, 10, 1);
            var node = new ArrayNode { DeclaredSize = 3 }
                .WithPositionFromRange(startToken, endToken);

            Assert.IsNotNull(node);
            Assert.AreEqual(3, node.DeclaredSize);
            Assert.AreEqual(1, node.StartLine);
            Assert.AreEqual(1, node.EndLine);
        }

        #endregion

        #region WithPosition Tests

        [TestMethod]
        public void WithPosition_ExplicitValues_SetsAllPropertiesCorrectly()
        {
            var node = new StringValueNode { Value = "test" };

            var result = node.WithPosition(1, 5, 10, 3, 15, 50);

            Assert.AreSame(node, result);
            Assert.AreEqual(1, node.StartLine);
            Assert.AreEqual(5, node.StartColumn);
            Assert.AreEqual(10, node.StartPosition);
            Assert.AreEqual(3, node.EndLine);
            Assert.AreEqual(15, node.EndColumn);
            Assert.AreEqual(50, node.EndPosition);
        }

        [TestMethod]
        public void WithPosition_SameLine_SetsCorrectly()
        {
            var node = new NumberValueNode { Value = 123 };

            node.WithPosition(5, 10, 100, 5, 15, 105);

            Assert.AreEqual(5, node.StartLine);
            Assert.AreEqual(5, node.EndLine);
            Assert.AreEqual(10, node.StartColumn);
            Assert.AreEqual(15, node.EndColumn);
        }

        [TestMethod]
        public void WithPosition_LargeValues_HandlesCorrectly()
        {
            var node = new StringValueNode { Value = "test" };

            node.WithPosition(1000, 500, 50000, 1500, 750, 75000);

            Assert.AreEqual(1000, node.StartLine);
            Assert.AreEqual(500, node.StartColumn);
            Assert.AreEqual(50000, node.StartPosition);
            Assert.AreEqual(1500, node.EndLine);
            Assert.AreEqual(750, node.EndColumn);
            Assert.AreEqual(75000, node.EndPosition);
        }

        [TestMethod]
        public void WithPosition_SupportsMethodChaining()
        {
            var node = new BooleanValueNode { Value = true }
                .WithPosition(1, 1, 0, 1, 5, 4);

            Assert.IsNotNull(node);
            Assert.IsTrue(node.Value);
            Assert.AreEqual(1, node.StartLine);
            Assert.AreEqual(1, node.EndLine);
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void AllThreeMethods_CanBeUsedInterchangeably()
        {
            var token = new Token(TokenType.String, "test", 2, 5, 10, 4);
            var node1 = new StringValueNode { Value = "v1" }.WithPositionFrom(token);
            var node2 = new StringValueNode { Value = "v2" }.WithPositionFromRange(token, token);
            var node3 = new StringValueNode { Value = "v3" }.WithPosition(2, 5, 10, 2, 5, 10);

            // All three should produce the same position
            Assert.AreEqual(node1.StartLine, node2.StartLine);
            Assert.AreEqual(node2.StartLine, node3.StartLine);
            Assert.AreEqual(node1.StartColumn, node2.StartColumn);
            Assert.AreEqual(node2.StartColumn, node3.StartColumn);
        }

        [TestMethod]
        public void MethodChaining_MultipleOperations_WorksCorrectly()
        {
            var token = new Token(TokenType.Number, "42", 3, 10, 20, 2);
            var node = new NumberValueNode { Value = 42, IsInteger = true, RawValue = "42" }
                .WithPositionFrom(token);

            // Verify the node state is complete
            Assert.AreEqual(42, node.Value);
            Assert.IsTrue(node.IsInteger);
            Assert.AreEqual("42", node.RawValue);
            Assert.AreEqual(3, node.StartLine);
            Assert.AreEqual(10, node.StartColumn);
            Assert.AreEqual(20, node.StartPosition);
        }

        #endregion
    }
}
