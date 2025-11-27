using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Core
{
    /// <summary>
    /// Tests for ToonParseResult factory methods, properties, and constructors.
    /// </summary>
    [TestClass]
    public class ToonParseResultTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_InitializesWithEmptyCollections()
        {
            var result = new ToonParseResult();

            Assert.IsNotNull(result.Document);
            Assert.IsNotNull(result.Errors);
            Assert.IsNotNull(result.Tokens);
            Assert.IsEmpty(result.Errors);
            Assert.IsEmpty(result.Tokens);
            Assert.IsEmpty(result.Document.Properties);
        }

        [TestMethod]
        public void Constructor_WithDocumentAndErrors_SetsProperties()
        {
            var doc = new ToonDocument();
            doc.Properties.Add(new PropertyNode { Key = "test" });
            var errors = new List<ToonError> { new("test error", 0, 1, 1, 1) };

            var result = new ToonParseResult(doc, errors);

            Assert.AreSame(doc, result.Document);
            Assert.AreSame(errors, result.Errors);
            Assert.HasCount(1, result.Document.Properties);
            Assert.HasCount(1, result.Errors);
        }

        [TestMethod]
        public void Constructor_WithNullDocument_CreatesEmptyDocument()
        {
            var result = new ToonParseResult(null, new List<ToonError>());

            Assert.IsNotNull(result.Document);
            Assert.IsEmpty(result.Document.Properties);
        }

        [TestMethod]
        public void Constructor_WithNullErrors_CreatesEmptyList()
        {
            var doc = new ToonDocument();
            var result = new ToonParseResult(doc, null);

            Assert.IsNotNull(result.Errors);
            Assert.IsEmpty(result.Errors);
        }

        #endregion

        #region Success Factory Method Tests

        [TestMethod]
        public void Success_WithDocument_CreatesSuccessResult()
        {
            var doc = new ToonDocument();
            doc.Properties.Add(new PropertyNode { Key = "name", Value = new StringValueNode { Value = "test" } });

            var result = ToonParseResult.Success(doc);

            Assert.AreSame(doc, result.Document);
            Assert.IsEmpty(result.Errors);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public void Success_WithDocumentAndTokens_StoresTokens()
        {
            var doc = new ToonDocument();
            var tokens = new List<Token>
            {
                new(TokenType.Identifier, "name", 1, 1, 0, 4),
                new(TokenType.Colon, ":", 1, 5, 4, 1)
            };

            var result = ToonParseResult.Success(doc, tokens);

            Assert.AreSame(doc, result.Document);
            Assert.AreSame(tokens, result.Tokens);
            Assert.HasCount(2, result.Tokens);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void Success_WithNullDocument_CreatesEmptyDocument()
        {
            var result = ToonParseResult.Success(null);

            Assert.IsNotNull(result.Document);
            Assert.IsEmpty(result.Document.Properties);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void Success_WithNullTokens_CreatesEmptyTokenList()
        {
            var doc = new ToonDocument();
            var result = ToonParseResult.Success(doc, null);

            Assert.IsNotNull(result.Tokens);
            Assert.IsEmpty(result.Tokens);
        }

        #endregion

        #region Partial Factory Method Tests

        [TestMethod]
        public void Partial_WithDocumentAndErrors_CreatesPartialResult()
        {
            var doc = new ToonDocument();
            doc.Properties.Add(new PropertyNode { Key = "valid" });
            var errors = new List<ToonError>
            {
                new("Error 1", 10, 1, 2, 5),
                new("Error 2", 20, 1, 3, 10)
            };

            var result = ToonParseResult.Partial(doc, errors);

            Assert.AreSame(doc, result.Document);
            Assert.AreSame(errors, result.Errors);
            Assert.HasCount(1, result.Document.Properties);
            Assert.HasCount(2, result.Errors);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void Partial_WithTokens_StoresTokens()
        {
            var doc = new ToonDocument();
            var errors = new List<ToonError> { new("test", 0, 1, 1, 1) };
            var tokens = new List<Token> { new(TokenType.Identifier, "name", 1, 1, 0, 4) };

            var result = ToonParseResult.Partial(doc, errors, tokens);

            Assert.AreSame(tokens, result.Tokens);
            Assert.HasCount(1, result.Tokens);
        }

        [TestMethod]
        public void Partial_WithNullDocument_CreatesEmptyDocument()
        {
            var errors = new List<ToonError> { new("test", 0, 1, 1, 1) };
            var result = ToonParseResult.Partial(null, errors);

            Assert.IsNotNull(result.Document);
            Assert.IsEmpty(result.Document.Properties);
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void Partial_WithNullErrors_CreatesEmptyErrorList()
        {
            var doc = new ToonDocument();
            var result = ToonParseResult.Partial(doc, null);

            Assert.IsNotNull(result.Errors);
            Assert.IsEmpty(result.Errors);
            Assert.IsTrue(result.IsSuccess); // No errors = success
        }

        [TestMethod]
        public void Partial_WithNullTokens_CreatesEmptyTokenList()
        {
            var doc = new ToonDocument();
            var errors = new List<ToonError> { new("test", 0, 1, 1, 1) };
            var result = ToonParseResult.Partial(doc, errors, null);

            Assert.IsNotNull(result.Tokens);
            Assert.IsEmpty(result.Tokens);
        }

        #endregion

        #region Failure Factory Method Tests (List Overload)

        [TestMethod]
        public void Failure_WithErrorList_CreatesFailureResult()
        {
            var errors = new List<ToonError>
            {
                new("Syntax error", 0, 1, 1, 1),
                new("Parse error", 10, 1, 2, 5)
            };

            var result = ToonParseResult.Failure(errors);

            Assert.IsNotNull(result.Document);
            Assert.IsEmpty(result.Document.Properties);
            Assert.AreSame(errors, result.Errors);
            Assert.HasCount(2, result.Errors);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void Failure_WithErrorListAndTokens_StoresTokens()
        {
            var errors = new List<ToonError> { new("test", 0, 1, 1, 1) };
            var tokens = new List<Token> { new(TokenType.Invalid, "!", 1, 1, 0, 1) };

            var result = ToonParseResult.Failure(errors, tokens);

            Assert.AreSame(tokens, result.Tokens);
            Assert.HasCount(1, result.Tokens);
        }

        [TestMethod]
        public void Failure_WithNullErrorList_CreatesEmptyErrorList()
        {
            var result = ToonParseResult.Failure((List<ToonError>)null);

            Assert.IsNotNull(result.Errors);
            Assert.IsEmpty(result.Errors);
            Assert.IsTrue(result.IsSuccess); // No errors = success (edge case)
        }

        [TestMethod]
        public void Failure_WithEmptyErrorList_CreatesSuccess()
        {
            var errors = new List<ToonError>();
            var result = ToonParseResult.Failure(errors);

            Assert.IsEmpty(result.Errors);
            Assert.IsTrue(result.IsSuccess); // Empty error list = success
        }

        #endregion

        #region Failure Factory Method Tests (Single Error Overload)

        [TestMethod]
        public void Failure_WithSingleError_CreatesFailureResult()
        {
            var error = new ToonError("Fatal error", 0, 1, 1, 1);

            var result = ToonParseResult.Failure(error);

            Assert.IsNotNull(result.Document);
            Assert.IsEmpty(result.Document.Properties);
            Assert.HasCount(1, result.Errors);
            Assert.AreSame(error, result.Errors[0]);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void Failure_WithSingleErrorAndTokens_StoresTokens()
        {
            var error = new ToonError("test", 0, 1, 1, 1);
            var tokens = new List<Token> { new(TokenType.EndOfFile, "", 1, 1, 0, 0) };

            var result = ToonParseResult.Failure(error, tokens);

            Assert.AreSame(tokens, result.Tokens);
            Assert.HasCount(1, result.Tokens);
            Assert.HasCount(1, result.Errors);
        }

        [TestMethod]
        public void Failure_WithSingleErrorAndNullTokens_CreatesEmptyTokenList()
        {
            var error = new ToonError("test", 0, 1, 1, 1);
            var result = ToonParseResult.Failure(error, null);

            Assert.IsNotNull(result.Tokens);
            Assert.IsEmpty(result.Tokens);
        }

        #endregion

        #region IsSuccess and HasErrors Property Tests

        [TestMethod]
        public void IsSuccess_WithNoErrors_ReturnsTrue()
        {
            var result = new ToonParseResult();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public void IsSuccess_WithErrors_ReturnsFalse()
        {
            var result = new ToonParseResult();
            result.Errors.Add(new ToonError("test", 0, 1, 1, 1));

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void IsSuccess_WithMultipleErrors_ReturnsFalse()
        {
            var result = new ToonParseResult();
            result.Errors.Add(new ToonError("error 1", 0, 1, 1, 1));
            result.Errors.Add(new ToonError("error 2", 5, 1, 2, 1));
            result.Errors.Add(new ToonError("error 3", 10, 1, 3, 1));

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasErrors);
            Assert.HasCount(3, result.Errors);
        }

        [TestMethod]
        public void HasErrors_IsInverseOfIsSuccess()
        {
            var resultSuccess = ToonParseResult.Success(new ToonDocument());
            Assert.IsTrue(resultSuccess.IsSuccess);
            Assert.IsFalse(resultSuccess.HasErrors);

            var resultFailure = ToonParseResult.Failure(new ToonError("test", 0, 1, 1, 1));
            Assert.IsFalse(resultFailure.IsSuccess);
            Assert.IsTrue(resultFailure.HasErrors);
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void SuccessResult_HasCorrectState()
        {
            var doc = new ToonDocument();
            doc.Properties.Add(new PropertyNode { Key = "key1" });
            var tokens = new List<Token> { new(TokenType.Identifier, "key1", 1, 1, 0, 4) };

            var result = ToonParseResult.Success(doc, tokens);

            Assert.AreSame(doc, result.Document);
            Assert.HasCount(1, result.Document.Properties);
            Assert.HasCount(1, result.Tokens);
            Assert.IsEmpty(result.Errors);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public void PartialResult_HasCorrectState()
        {
            var doc = new ToonDocument();
            doc.Properties.Add(new PropertyNode { Key = "valid" });
            var errors = new List<ToonError> { new("parse error", 10, 1, 2, 5) };
            var tokens = new List<Token> { new(TokenType.Identifier, "valid", 1, 1, 0, 5) };

            var result = ToonParseResult.Partial(doc, errors, tokens);

            Assert.AreSame(doc, result.Document);
            Assert.HasCount(1, result.Document.Properties);
            Assert.HasCount(1, result.Errors);
            Assert.HasCount(1, result.Tokens);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void FailureResult_HasCorrectState()
        {
            var errors = new List<ToonError>
            {
                new("catastrophic error", 0, 1, 1, 1)
            };
            var tokens = new List<Token> { new(TokenType.Invalid, "bad", 1, 1, 0, 3) };

            var result = ToonParseResult.Failure(errors, tokens);

            Assert.IsNotNull(result.Document);
            Assert.IsEmpty(result.Document.Properties);
            Assert.HasCount(1, result.Errors);
            Assert.HasCount(1, result.Tokens);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void AllFactoryMethods_NeverReturnNullCollections()
        {
            var success = ToonParseResult.Success(null, null);
            Assert.IsNotNull(success.Document);
            Assert.IsNotNull(success.Errors);
            Assert.IsNotNull(success.Tokens);

            var partial = ToonParseResult.Partial(null, null, null);
            Assert.IsNotNull(partial.Document);
            Assert.IsNotNull(partial.Errors);
            Assert.IsNotNull(partial.Tokens);

            var failure1 = ToonParseResult.Failure((List<ToonError>)null, null);
            Assert.IsNotNull(failure1.Document);
            Assert.IsNotNull(failure1.Errors);
            Assert.IsNotNull(failure1.Tokens);

            var failure2 = ToonParseResult.Failure(new ToonError("test", 0, 1, 1, 1), null);
            Assert.IsNotNull(failure2.Document);
            Assert.IsNotNull(failure2.Errors);
            Assert.IsNotNull(failure2.Tokens);
        }

        #endregion
    }
}
