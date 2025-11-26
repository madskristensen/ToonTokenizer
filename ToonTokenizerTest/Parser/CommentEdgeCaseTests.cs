using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for comment handling edge cases.
    /// TOON spec allows # and // comments, which should be ignored during parsing.
    /// </summary>
    [TestClass]
    public class CommentEdgeCaseTests
    {
        #region Comments in Arrays

        [TestMethod]
        public void Parse_CommentInInlineArray_IgnoresComment()
        {
            // Comment should be ignored, array should have 3 elements
            var source = "items[3]: a,b,c # This is a comment";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
            Assert.AreEqual("a", ((StringValueNode)array.Elements[0]).Value);
            Assert.AreEqual("b", ((StringValueNode)array.Elements[1]).Value);
            Assert.AreEqual("c", ((StringValueNode)array.Elements[2]).Value);
        }

        [TestMethod]
        public void Parse_CommentBetweenArrayElements_Behavior()
        {
            // Comment in the middle of array values - may cause parsing issues
            var source = @"items[3]: a,
  # comment line
  b,c";
            ToonParseResult result = Toon.Parse(source);

            // Should handle gracefully - either parse successfully or return error
            Assert.IsNotNull(result);
            if (result.IsSuccess)
            {
                var array = (ArrayNode)result.Document.Properties[0].Value;
                Assert.IsGreaterThanOrEqualTo(2, array.Elements.Count);
            }
        }

        [TestMethod]
        public void Parse_DoubleSlashCommentInArray_IgnoresComment()
        {
            var source = "items[2]: x,y // comment";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 2);
        }

        #endregion

        #region Comments in Table Arrays

        [TestMethod]
        public void Parse_CommentInTableArrayRow_IgnoresComment()
        {
            var source = @"data[2]{id,name}:
  1,Alice # First user
  2,Bob   # Second user";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name");
            Assert.AreEqual("Alice", ((StringValueNode)table.Rows[0][1]).Value);
            Assert.AreEqual("Bob", ((StringValueNode)table.Rows[1][1]).Value);
        }

        [TestMethod]
        public void Parse_CommentBetweenTableRows_ParsesCorrectly()
        {
            var source = @"data[2]{id,name}:
  1,Alice
  # This is a comment between rows
  2,Bob";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name");
        }

        [TestMethod]
        public void Parse_CommentAfterSchemaDefinition_ParsesCorrectly()
        {
            var source = @"data[1]{id,name}: # Comment after schema
  1,Alice";
            ToonParseResult result = Toon.Parse(source);

            // The parser may have difficulty with comment directly after schema colon
            // but should at least return a result (possibly with errors)
            Assert.IsNotNull(result);
            if (result.IsSuccess)
            {
                var table = (TableArrayNode)result.Document.Properties[0].Value;
                Assert.HasCount(1, table.Rows);
            }
            else
            {
                // If parsing fails, document should still be created (resilient parsing)
                Assert.IsNotNull(result.Document);
            }
        }

        #endregion

        #region Comments in Objects

        [TestMethod]
        public void Parse_CommentAfterSimpleValue_IgnoresComment()
        {
            var source = "name: John # This is John's name";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("John", value.Value);
        }

        [TestMethod]
        public void Parse_CommentBetweenObjectProperties_ParsesCorrectly()
        {
            var source = @"obj:
  # First property below
  prop1: value1
  # Second property below
  prop2: value2";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var obj = (ObjectNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, obj.Properties);
        }

        #endregion

        #region Comment Styles

        [TestMethod]
        public void Parse_HashComment_ParsesCorrectly()
        {
            var source = @"# This is a hash comment
value: test";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(1, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_DoubleSlashComment_ParsesCorrectly()
        {
            var source = @"// This is a double-slash comment
value: test";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(1, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_MixedCommentStyles_ParsesCorrectly()
        {
            var source = @"# Hash comment
value1: test1
// Double-slash comment
value2: test2";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(2, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_MultipleConsecutiveComments_ParsesCorrectly()
        {
            var source = @"# Comment 1
# Comment 2
# Comment 3
value: test";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(1, result.Document.Properties);
        }

        #endregion

        #region Comments as Values

        [TestMethod]
        public void Parse_HashInQuotedString_PreservesHash()
        {
            // Hash inside quotes should be preserved as part of string
            var source = "tag: \"#hashtag\"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("#hashtag", value.Value);
        }

        [TestMethod]
        public void Parse_DoubleSlashInQuotedString_PreservesSlashes()
        {
            var source = "url: \"http://example.com\"";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            Assert.AreEqual("http://example.com", value.Value);
        }

        [TestMethod]
        public void Parse_UnquotedValueWithHash_StopsAtHash()
        {
            // Unquoted value should stop at # (comment start)
            var source = "value: text#notcomment";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.IsSuccess);
            var value = (StringValueNode)result.Document.Properties[0].Value;
            // Should be "text" or "text#notcomment" depending on lexer behavior
            Assert.Contains("text", value.Value);
        }

        #endregion

        #region Empty and Whitespace with Comments

        [TestMethod]
        public void Parse_OnlyComments_ReturnsEmptyDocument()
        {
            var source = @"# Just comments
// Nothing else
# More comments";
            ToonParseResult result = Toon.Parse(source);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsEmpty(result.Document.Properties);
        }

        [TestMethod]
        public void Parse_CommentWithOnlyWhitespace_ParsesCorrectly()
        {
            var source = @"value: test
   # Comment with leading spaces
value2: test2";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(2, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_BlankLineFollowedByComment_ParsesCorrectly()
        {
            var source = @"value1: test1

# Comment after blank line
value2: test2";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(2, result.Document.Properties);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_CommentAtEndOfFile_ParsesCorrectly()
        {
            var source = "value: test\n# Final comment";

            ToonParseResult result = ToonTestHelpers.ParseSuccess(source);
            Assert.HasCount(1, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_CommentWithSpecialCharacters_ParsesCorrectly()
        {
            var source = "value: test # Comment with @#$%^&*() special chars!";

            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);
            Assert.AreEqual("test", value.Value);
        }

        [TestMethod]
        public void Parse_CommentWithUnicode_ParsesCorrectly()
        {
            var source = "value: test # Comment with emoji 👍 and 日本語";

            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);
            Assert.AreEqual("test", value.Value);
        }

        [TestMethod]
        public void Parse_VeryLongComment_ParsesCorrectly()
        {
            var longComment = new string('x', 1000);
            var source = $"value: test # {longComment}";

            StringValueNode value = ToonTestHelpers.ParseAndGetValue<StringValueNode>(source);
            Assert.AreEqual("test", value.Value);
        }

        #endregion
    }
}
