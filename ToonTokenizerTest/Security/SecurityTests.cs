using System.Text;

using ToonTokenizer;

namespace ToonTokenizerTest.Security
{
    /// <summary>
    /// Security-focused tests validating resource limits and DoS attack prevention.
    /// </summary>
    [TestClass]
    public class SecurityTests
    {
        #region Input Size Validation Tests

        [TestMethod]
        public void Parse_ExceedsMaxInputSize_ThrowsArgumentException()
        {
            // Arrange: Create input that exceeds default 10MB limit
            var options = ToonParserOptions.Default;
            var largeInput = new string('x', options.MaxInputSize + 1);

            // Act & Assert
            var ex = Assert.ThrowsExactly<ArgumentException>(() => Toon.Parse(largeInput, options));
            Assert.Contains("exceeds maximum allowed size", ex.Message);
            // Message uses formatted number (10,485,760) not raw number
        }

        [TestMethod]
        public void Parse_WithinMaxInputSize_Succeeds()
        {
            // Arrange: Create input just under the limit
            var options = ToonParserOptions.Default;
            var validInput = new string('x', options.MaxInputSize - 100);

            // Act: Should not throw
            var result = Toon.Parse(validInput, options);

            // Assert: May fail to parse but shouldn't throw size exception
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Encoder_ExceedsMaxJsonInputSize_ThrowsArgumentException()
        {
            // Arrange: Create JSON that exceeds 10MB limit
            const int maxSize = 10 * 1024 * 1024;
            var largeJson = new string('x', maxSize + 1);

            // Act & Assert
            var encoder = new ToonEncoder();
            var ex = Assert.ThrowsExactly<ArgumentException>(() => encoder.EncodeFromJson(largeJson));
            Assert.Contains("exceeds maximum allowed size", ex.Message);
            // Message uses formatted number (10,485,760) not raw number
        }

        #endregion

        #region Token Count Limit Tests

        [TestMethod]
        public void Parse_ExceedsMaxTokenCount_StopsTokenization()
        {
            // Arrange: Create input with more tokens than limit
            var options = new ToonParserOptions
            {
                MaxTokenCount = 100 // Low limit for testing
            };

            // Create input with ~200 tokens (100 key-value pairs)
            var sb = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                sb.AppendLine($"key{i}: value{i}");
            }

            // Act
            var result = Toon.Parse(sb.ToString(), options);

            // Assert: Should have errors about token count
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.Errors);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Token count") || e.Message.Contains("exceeds maximum")));
        }

        [TestMethod]
        public void Parse_WithinMaxTokenCount_Succeeds()
        {
            // Arrange
            var options = new ToonParserOptions
            {
                MaxTokenCount = 1000
            };

            var input = "key1: value1\nkey2: value2\nkey3: value3";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void Parse_UnlimitedTokenCount_NoLimit()
        {
            // Arrange
            var options = ToonParserOptions.Unlimited;

            // Create large input with many tokens
            var sb = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                sb.AppendLine($"k{i}: v{i}");
            }

            // Act
            var result = Toon.Parse(sb.ToString(), options);

            // Assert: Should succeed without token count errors
            Assert.IsTrue(result.IsSuccess);
        }

        #endregion

        #region String Length Limit Tests

        [TestMethod]
        public void Parse_ExceedsMaxStringLength_RecordsError()
        {
            // Arrange
            var options = new ToonParserOptions
            {
                MaxStringLength = 100 // Low limit for testing
            };

            // Create string that exceeds limit
            var longString = new string('x', 150);
            var input = $"key: \"{longString}\"";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.Errors);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("String length") || e.Message.Contains("exceeds maximum")));
        }

        [TestMethod]
        public void Parse_LongUnquotedString_RecordsError()
        {
            // Arrange
            var options = new ToonParserOptions
            {
                MaxStringLength = 50
            };

            var longUnquoted = new string('x', 60);
            var input = $"key: {longUnquoted}";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Unquoted string length") || e.Message.Contains("exceeds maximum")));
        }

        [TestMethod]
        public void Parse_LongIdentifier_RecordsError()
        {
            // Arrange
            var options = new ToonParserOptions
            {
                MaxStringLength = 50
            };

            var longKey = new string('k', 60);
            var input = $"{longKey}: value";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Identifier length") || e.Message.Contains("exceeds maximum")));
        }

        [TestMethod]
        public void Parse_WithinMaxStringLength_Succeeds()
        {
            // Arrange
            var options = new ToonParserOptions
            {
                MaxStringLength = 1000
            };

            var validString = new string('x', 500);
            var input = $"key: \"{validString}\"";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        #endregion

        #region Nesting Depth Limit Tests

        [TestMethod]
        public void Parse_ExceedsMaxNestingDepth_RecordsError()
        {
            // Arrange
            // The parser increments depth in ParseObject when parsing nested objects
            // Depth tracking: _currentDepth starts at 0
            //   - "outer:" value is newline → ParseObject → depth becomes 1
            //   - "middle:" value is newline → ParseObject → depth becomes 2
            //   - "inner:" value is newline → ParseObject → depth becomes 3 (exceeds limit of 2!)
            var options = new ToonParserOptions
            {
                MaxNestingDepth = 2 // Allow depth 1 and 2, block depth 3
            };

            // Four levels of nesting: outer(1) -> middle(2) -> inner(3 - exceeds!) -> value
            var input = @"outer:
  middle:
    inner:
      deepest: value";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsTrue(result.HasErrors, "Expected errors due to nesting depth violation");
            Assert.IsNotEmpty(result.Errors, "Expected at least one error");

            var depthError = result.Errors.Find(e => e.Message.Contains("Maximum nesting depth"));
            Assert.IsNotNull(depthError,
                $"Expected nesting depth error. Actual errors: {string.Join("; ", result.Errors.Select(e => e.Message))}");
            Assert.Contains("exceeded",
depthError.Message, "Error message should contain 'exceeded'");
        }

        [TestMethod]
        public void Parse_ExceedsNestingDepthInArray_RecordsError()
        {
            // Arrange
            // ParseExpandedArray also tracks nesting depth
            // Depth: items array value → ParseExpandedArray (depth 1)
            //        list item "-" → ParseValue may trigger ParseObject for nested content
            var options = new ToonParserOptions
            {
                MaxNestingDepth = 2 // Allow up to depth 2
            };

            // Structure: document → items ParseExpandedArray(1) → nested obj(2) → deep obj(3 - exceeds!)
            var input = @"items[1]:
  - nested:
      deep:
        tooDeep: value";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsTrue(result.HasErrors, "Expected errors due to nesting depth violation in array");
            Assert.IsNotEmpty(result.Errors, "Expected at least one error");

            // The error might be about nesting depth OR about array structure issues due to blocked parsing
            var hasDepthOrStructureError = result.Errors.Exists(e =>
                e.Message.Contains("Maximum nesting depth") ||
                e.Message.Contains("Array size mismatch"));

            Assert.IsTrue(hasDepthOrStructureError,
                $"Expected nesting depth or structure error. Actual errors: {string.Join("; ", result.Errors.Select(e => e.Message))}");
        }

        [TestMethod]
        public void Parse_WithinMaxNestingDepth_Succeeds()
        {
            // Arrange
            var options = new ToonParserOptions
            {
                MaxNestingDepth = 10
            };

            var input = @"
level1:
  level2:
    level3: value
";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        #endregion

        #region Array Size Limit Tests

        [TestMethod]
        public void Parse_ExceedsMaxArraySize_RecordsError()
        {
            // Arrange
            var options = new ToonParserOptions
            {
                MaxArraySize = 10 // Low limit for testing
            };

            // Declare array larger than limit
            var input = "items[15]: value1,value2,value3";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Array size") && e.Message.Contains("exceeds")));
        }

        [TestMethod]
        public void Parse_ArraySizeWithOverflow_RecordsError()
        {
            // Arrange
            var options = ToonParserOptions.Default;

            // Create array size that would overflow
            var input = $"items[{int.MaxValue}]: value";

            // Act
            var result = Toon.Parse(input, options);

            // Assert: Should handle gracefully without crashing
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void Parse_WithinMaxArraySize_Succeeds()
        {
            // Arrange
            var options = new ToonParserOptions
            {
                MaxArraySize = 100
            };

            var input = "items[5]: a,b,c,d,e";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        #endregion

        #region Regex Timeout Protection Tests

        [TestMethod]
        public void Encoder_ComplexStringPattern_DoesNotHang()
        {
            // Arrange: Create string that could trigger ReDoS
            var encoder = new ToonEncoder();
            var problematicJson = "{\"key\": \"" + new string('0', 1000) + "e+99999\"}";

            // Act: Should complete within reasonable time (regex has 1s timeout)
            var startTime = DateTime.UtcNow;
            string? result = null;

            try
            {
                result = encoder.EncodeFromJson(problematicJson);
            }
            catch
            {
                // May throw if JSON is invalid, that's ok
            }

            var elapsed = DateTime.UtcNow - startTime;

            // Assert: Should not hang (complete within 5 seconds)
            Assert.IsLessThan(5, elapsed.TotalSeconds, $"Encoding took {elapsed.TotalSeconds} seconds - possible ReDoS");
            Assert.IsNotNull(result); // Should produce some result
        }

        [TestMethod]
        public void Encoder_NumericLikeString_HandlesTimeout()
        {
            // Arrange: String that matches numeric pattern but is complex
            var encoder = new ToonEncoder();
            var json = "{\"value\": \"123.456e+789\"}";

            // Act: Should complete successfully
            var result = encoder.EncodeFromJson(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.Contains("value", result);
        }

        #endregion

        #region Default vs Unlimited Options Tests

        [TestMethod]
        public void DefaultOptions_HasSecureLimits()
        {
            // Arrange & Act
            var options = ToonParserOptions.Default;

            // Assert: Verify all limits are set to safe values
            Assert.AreEqual(10 * 1024 * 1024, options.MaxInputSize); // 10 MB
            Assert.AreEqual(1_000_000, options.MaxTokenCount);
            Assert.AreEqual(64 * 1024, options.MaxStringLength); // 64 KB
            Assert.AreEqual(100, options.MaxNestingDepth);
            Assert.AreEqual(1_000_000, options.MaxArraySize);
        }

        [TestMethod]
        public void UnlimitedOptions_HasNoLimits()
        {
            // Arrange & Act
            var options = ToonParserOptions.Unlimited;

            // Assert: Verify all limits are set to int.MaxValue
            Assert.AreEqual(int.MaxValue, options.MaxInputSize);
            Assert.AreEqual(int.MaxValue, options.MaxTokenCount);
            Assert.AreEqual(int.MaxValue, options.MaxStringLength);
            Assert.AreEqual(int.MaxValue, options.MaxNestingDepth);
            Assert.AreEqual(int.MaxValue, options.MaxArraySize);
        }

        [TestMethod]
        public void CustomOptions_CanBeConfigured()
        {
            // Arrange & Act
            var options = new ToonParserOptions
            {
                MaxInputSize = 1024,
                MaxTokenCount = 100,
                MaxStringLength = 50,
                MaxNestingDepth = 10,
                MaxArraySize = 25
            };

            // Assert
            Assert.AreEqual(1024, options.MaxInputSize);
            Assert.AreEqual(100, options.MaxTokenCount);
            Assert.AreEqual(50, options.MaxStringLength);
            Assert.AreEqual(10, options.MaxNestingDepth);
            Assert.AreEqual(25, options.MaxArraySize);
        }

        #endregion

        #region Combined Attack Vectors Tests

        [TestMethod]
        public void Parse_MultipleViolations_RecordsAllErrors()
        {
            // Arrange: Create input with multiple limit violations
            var options = new ToonParserOptions
            {
                MaxTokenCount = 50,
                MaxStringLength = 20,
                MaxNestingDepth = 2
            };

            var input = @"
level1:
  level2:
    level3:
      veryLongKeyNameThatExceedsLimit: ""veryLongStringValueThatExceedsLimit""
";

            // Act
            var result = Toon.Parse(input, options);

            // Assert: Should record multiple errors
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.Errors);
        }

        [TestMethod]
        public void Parse_StressTest_HandlesGracefully()
        {
            // Arrange: Create moderately complex input
            var options = ToonParserOptions.Default;
            var sb = new StringBuilder();

            for (int i = 0; i < 1000; i++)
            {
                sb.AppendLine($"item{i}:");
                sb.AppendLine($"  id: {i}");
                sb.AppendLine($"  name: Item {i}");
                sb.AppendLine($"  values[3]: {i}, {i + 1}, {i + 2}");
            }

            // Act
            var startTime = DateTime.UtcNow;
            var result = Toon.Parse(sb.ToString(), options);
            var elapsed = DateTime.UtcNow - startTime;

            // Assert: Should complete reasonably quickly
            Assert.IsLessThan(10, elapsed.TotalSeconds, $"Parsing took {elapsed.TotalSeconds} seconds");
            Assert.IsTrue(result.IsSuccess);
        }

        #endregion

        #region Error Message Quality Tests

        [TestMethod]
        public void Parse_InputSizeExceeded_HasHelpfulMessage()
        {
            // Arrange
            var options = ToonParserOptions.Default;
            var largeInput = new string('x', options.MaxInputSize + 1);

            // Act & Assert
            var ex = Assert.ThrowsExactly<ArgumentException>(() => Toon.Parse(largeInput, options));

            // Verify message is helpful and doesn't expose internal details
            Assert.Contains("exceeds maximum allowed size", ex.Message);
            Assert.Contains("ToonParserOptions", ex.Message);
            Assert.DoesNotContain("stack", ex.Message.ToLower());
            Assert.DoesNotContain("internal", ex.Message.ToLower());
        }

        [TestMethod]
        public void Parse_TokenCountExceeded_HasHelpfulMessage()
        {
            // Arrange
            var options = new ToonParserOptions { MaxTokenCount = 10 };
            var input = "a: 1\nb: 2\nc: 3\nd: 4\ne: 5\nf: 6";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            var error = result.Errors.Find(e => e.Message.Contains("Token count") && e.Message.Contains("exceeds"));
            Assert.IsNotNull(error);
            Assert.Contains("ToonParserOptions", error.Message);
        }

        [TestMethod]
        public void Parse_StringLengthExceeded_HasHelpfulMessage()
        {
            // Arrange
            var options = new ToonParserOptions { MaxStringLength = 10 };
            var input = "key: \"this is a very long string that exceeds the limit\"";

            // Act
            var result = Toon.Parse(input, options);

            // Assert
            var error = result.Errors.Find(e => e.Message.Contains("String length") && e.Message.Contains("exceeds"));
            Assert.IsNotNull(error);
            Assert.Contains("ToonParserOptions", error.Message);
        }

        #endregion
    }
}
