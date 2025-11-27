using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToonTokenizer;

namespace ToonTokenizerTest.Extensions
{
    /// <summary>
    /// Tests for TokenTypeExtensions methods that categorize TokenType enum values.
    /// </summary>
    [TestClass]
    public class TokenTypeExtensionsTests
    {
        #region IsValueToken Tests

        [TestMethod]
        public void IsValueToken_StringType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.String.IsValueToken());
        }

        [TestMethod]
        public void IsValueToken_IdentifierType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Identifier.IsValueToken());
        }

        [TestMethod]
        public void IsValueToken_NumberType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Number.IsValueToken());
        }

        [TestMethod]
        public void IsValueToken_TrueType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.True.IsValueToken());
        }

        [TestMethod]
        public void IsValueToken_FalseType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.False.IsValueToken());
        }

        [TestMethod]
        public void IsValueToken_NullType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Null.IsValueToken());
        }

        [TestMethod]
        public void IsValueToken_NonValueTypes_ReturnFalse()
        {
            Assert.IsFalse(TokenType.Whitespace.IsValueToken());
            Assert.IsFalse(TokenType.Newline.IsValueToken());
            Assert.IsFalse(TokenType.Comment.IsValueToken());
            Assert.IsFalse(TokenType.Colon.IsValueToken());
            Assert.IsFalse(TokenType.Comma.IsValueToken());
            Assert.IsFalse(TokenType.Pipe.IsValueToken());
            Assert.IsFalse(TokenType.LeftBracket.IsValueToken());
            Assert.IsFalse(TokenType.RightBracket.IsValueToken());
            Assert.IsFalse(TokenType.LeftBrace.IsValueToken());
            Assert.IsFalse(TokenType.RightBrace.IsValueToken());
            Assert.IsFalse(TokenType.Indent.IsValueToken());
            Assert.IsFalse(TokenType.Dedent.IsValueToken());
            Assert.IsFalse(TokenType.Invalid.IsValueToken());
            Assert.IsFalse(TokenType.EndOfFile.IsValueToken());
        }

        #endregion

        #region IsStructuralToken Tests

        [TestMethod]
        public void IsStructuralToken_ColonType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Colon.IsStructuralToken());
        }

        [TestMethod]
        public void IsStructuralToken_CommaType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Comma.IsStructuralToken());
        }

        [TestMethod]
        public void IsStructuralToken_PipeType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Pipe.IsStructuralToken());
        }

        [TestMethod]
        public void IsStructuralToken_LeftBracketType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.LeftBracket.IsStructuralToken());
        }

        [TestMethod]
        public void IsStructuralToken_RightBracketType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.RightBracket.IsStructuralToken());
        }

        [TestMethod]
        public void IsStructuralToken_LeftBraceType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.LeftBrace.IsStructuralToken());
        }

        [TestMethod]
        public void IsStructuralToken_RightBraceType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.RightBrace.IsStructuralToken());
        }

        [TestMethod]
        public void IsStructuralToken_NonStructuralTypes_ReturnFalse()
        {
            Assert.IsFalse(TokenType.String.IsStructuralToken());
            Assert.IsFalse(TokenType.Identifier.IsStructuralToken());
            Assert.IsFalse(TokenType.Number.IsStructuralToken());
            Assert.IsFalse(TokenType.True.IsStructuralToken());
            Assert.IsFalse(TokenType.False.IsStructuralToken());
            Assert.IsFalse(TokenType.Null.IsStructuralToken());
            Assert.IsFalse(TokenType.Whitespace.IsStructuralToken());
            Assert.IsFalse(TokenType.Newline.IsStructuralToken());
            Assert.IsFalse(TokenType.Comment.IsStructuralToken());
            Assert.IsFalse(TokenType.Indent.IsStructuralToken());
            Assert.IsFalse(TokenType.Dedent.IsStructuralToken());
            Assert.IsFalse(TokenType.Invalid.IsStructuralToken());
            Assert.IsFalse(TokenType.EndOfFile.IsStructuralToken());
        }

        #endregion

        #region IsWhitespaceToken Tests

        [TestMethod]
        public void IsWhitespaceToken_WhitespaceType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Whitespace.IsWhitespaceToken());
        }

        [TestMethod]
        public void IsWhitespaceToken_NewlineType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Newline.IsWhitespaceToken());
        }

        [TestMethod]
        public void IsWhitespaceToken_IndentType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Indent.IsWhitespaceToken());
        }

        [TestMethod]
        public void IsWhitespaceToken_DedentType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Dedent.IsWhitespaceToken());
        }

        [TestMethod]
        public void IsWhitespaceToken_NonWhitespaceTypes_ReturnFalse()
        {
            Assert.IsFalse(TokenType.String.IsWhitespaceToken());
            Assert.IsFalse(TokenType.Identifier.IsWhitespaceToken());
            Assert.IsFalse(TokenType.Number.IsWhitespaceToken());
            Assert.IsFalse(TokenType.True.IsWhitespaceToken());
            Assert.IsFalse(TokenType.False.IsWhitespaceToken());
            Assert.IsFalse(TokenType.Null.IsWhitespaceToken());
            Assert.IsFalse(TokenType.Comment.IsWhitespaceToken());
            Assert.IsFalse(TokenType.Colon.IsWhitespaceToken());
            Assert.IsFalse(TokenType.Comma.IsWhitespaceToken());
            Assert.IsFalse(TokenType.Pipe.IsWhitespaceToken());
            Assert.IsFalse(TokenType.LeftBracket.IsWhitespaceToken());
            Assert.IsFalse(TokenType.RightBracket.IsWhitespaceToken());
            Assert.IsFalse(TokenType.LeftBrace.IsWhitespaceToken());
            Assert.IsFalse(TokenType.RightBrace.IsWhitespaceToken());
            Assert.IsFalse(TokenType.Invalid.IsWhitespaceToken());
            Assert.IsFalse(TokenType.EndOfFile.IsWhitespaceToken());
        }

        #endregion

        #region IsKeywordToken Tests

        [TestMethod]
        public void IsKeywordToken_TrueType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.True.IsKeywordToken());
        }

        [TestMethod]
        public void IsKeywordToken_FalseType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.False.IsKeywordToken());
        }

        [TestMethod]
        public void IsKeywordToken_NullType_ReturnsTrue()
        {
            Assert.IsTrue(TokenType.Null.IsKeywordToken());
        }

        [TestMethod]
        public void IsKeywordToken_NonKeywordTypes_ReturnFalse()
        {
            Assert.IsFalse(TokenType.String.IsKeywordToken());
            Assert.IsFalse(TokenType.Identifier.IsKeywordToken());
            Assert.IsFalse(TokenType.Number.IsKeywordToken());
            Assert.IsFalse(TokenType.Whitespace.IsKeywordToken());
            Assert.IsFalse(TokenType.Newline.IsKeywordToken());
            Assert.IsFalse(TokenType.Comment.IsKeywordToken());
            Assert.IsFalse(TokenType.Colon.IsKeywordToken());
            Assert.IsFalse(TokenType.Comma.IsKeywordToken());
            Assert.IsFalse(TokenType.Pipe.IsKeywordToken());
            Assert.IsFalse(TokenType.LeftBracket.IsKeywordToken());
            Assert.IsFalse(TokenType.RightBracket.IsKeywordToken());
            Assert.IsFalse(TokenType.LeftBrace.IsKeywordToken());
            Assert.IsFalse(TokenType.RightBrace.IsKeywordToken());
            Assert.IsFalse(TokenType.Indent.IsKeywordToken());
            Assert.IsFalse(TokenType.Dedent.IsKeywordToken());
            Assert.IsFalse(TokenType.Invalid.IsKeywordToken());
            Assert.IsFalse(TokenType.EndOfFile.IsKeywordToken());
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void AllTokenTypes_ClassifiedInAtLeastOneCategory()
        {
            // Ensure every TokenType value is classified in at least one category
            // This test ensures we don't miss any new enum values added in the future
            var allTypes = new[]
            {
                TokenType.String,
                TokenType.Identifier,
                TokenType.Number,
                TokenType.True,
                TokenType.False,
                TokenType.Null,
                TokenType.Whitespace,
                TokenType.Newline,
                TokenType.Comment,
                TokenType.Colon,
                TokenType.Comma,
                TokenType.Pipe,
                TokenType.LeftBracket,
                TokenType.RightBracket,
                TokenType.LeftBrace,
                TokenType.RightBrace,
                TokenType.Indent,
                TokenType.Dedent,
                TokenType.Invalid,
                TokenType.EndOfFile
            };

            foreach (var type in allTypes)
            {
                bool isClassified = type.IsValueToken() ||
                                   type.IsStructuralToken() ||
                                   type.IsWhitespaceToken() ||
                                   type.IsKeywordToken() ||
                                   type == TokenType.Comment ||
                                   type == TokenType.Invalid ||
                                   type == TokenType.EndOfFile;

                Assert.IsTrue(isClassified, $"TokenType.{type} is not classified in any category");
            }
        }

        [TestMethod]
        public void KeywordTokens_AreAlsoValueTokens()
        {
            // Keywords (true, false, null) should be both keywords AND value tokens
            Assert.IsTrue(TokenType.True.IsKeywordToken());
            Assert.IsTrue(TokenType.True.IsValueToken());

            Assert.IsTrue(TokenType.False.IsKeywordToken());
            Assert.IsTrue(TokenType.False.IsValueToken());

            Assert.IsTrue(TokenType.Null.IsKeywordToken());
            Assert.IsTrue(TokenType.Null.IsValueToken());
        }

        [TestMethod]
        public void ValueAndStructuralCategories_AreMutuallyExclusive()
        {
            // No token should be both a value token and a structural token
            var allTypes = new[]
            {
                TokenType.String, TokenType.Identifier, TokenType.Number,
                TokenType.True, TokenType.False, TokenType.Null,
                TokenType.Whitespace, TokenType.Newline, TokenType.Comment,
                TokenType.Colon, TokenType.Comma, TokenType.Pipe,
                TokenType.LeftBracket, TokenType.RightBracket,
                TokenType.LeftBrace, TokenType.RightBrace,
                TokenType.Indent, TokenType.Dedent,
                TokenType.Invalid, TokenType.EndOfFile
            };

            foreach (var type in allTypes)
            {
                bool isValue = type.IsValueToken();
                bool isStructural = type.IsStructuralToken();

                if (isValue && isStructural)
                {
                    Assert.Fail($"TokenType.{type} is classified as both Value and Structural");
                }
            }
        }

        #endregion
    }
}
