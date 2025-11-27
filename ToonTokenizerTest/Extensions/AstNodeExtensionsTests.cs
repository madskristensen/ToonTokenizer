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

        #endregion
    }
}
