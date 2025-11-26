using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest
{
    /// <summary>
    /// Shared test helper methods for ToonTokenizer tests.
    /// Reduces duplication and improves test maintainability.
    /// </summary>
    public static class ToonTestHelpers
    {
        #region Parse Helpers

        /// <summary>
        /// Parses source and asserts successful parsing with no errors.
        /// </summary>
        /// <param name="source">TOON source to parse</param>
        /// <returns>Successful parse result</returns>
        public static ToonParseResult ParseSuccess(string source)
        {
            ToonParseResult result = Toon.Parse(source);
            Assert.IsTrue(result.IsSuccess,
                $"Parse failed with {result.Errors.Count} errors: {string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Message}"))}");
            Assert.IsNotNull(result.Document, "Document should not be null for successful parse");
            return result;
        }

        /// <summary>
        /// Parses source and asserts parsing failure with errors.
        /// </summary>
        /// <param name="source">TOON source to parse</param>
        /// <param name="errorSubstring">Optional substring that should appear in at least one error message</param>
        /// <param name="errorCode">Optional error code that should be present</param>
        /// <returns>Failed parse result</returns>
        public static ToonParseResult ParseFailure(string source, string? errorSubstring = null, string? errorCode = null)
        {
            ToonParseResult result = Toon.Parse(source);
            Assert.IsTrue(result.HasErrors, "Expected parse to fail but it succeeded");

            if (errorSubstring != null)
            {
                ToonError? matchingError = result.Errors.FirstOrDefault(e =>
                    e.Message.Contains(errorSubstring, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(matchingError,
                    $"Expected error containing '{errorSubstring}'. Got: {string.Join("; ", result.Errors.Select(e => e.Message))}");
            }

            if (errorCode != null)
            {
                ToonError? matchingError = result.Errors.FirstOrDefault(e => e.Code == errorCode);
                Assert.IsNotNull(matchingError,
                    $"Expected error code '{errorCode}'. Got: {string.Join(", ", result.Errors.Select(e => e.Code))}");
            }

            return result;
        }

        /// <summary>
        /// Parses source and returns the first property's value as the specified type.
        /// Works with any AstNode type (ValueNode, ArrayNode, TableArrayNode, etc.).
        /// </summary>
        /// <typeparam name="T">Expected AST node type</typeparam>
        /// <param name="source">TOON source to parse</param>
        /// <param name="propertyIndex">Index of property to retrieve (default: 0)</param>
        /// <returns>Typed AST node</returns>
        public static T ParseAndGetValue<T>(string source, int propertyIndex = 0) where T : AstNode
        {
            ToonParseResult result = ParseSuccess(source);
            Assert.IsLessThan(result.Document.Properties.Count,
propertyIndex, $"Property index {propertyIndex} out of range (count: {result.Document.Properties.Count})");

            AstNode value = result.Document.Properties[propertyIndex].Value;
            Assert.IsInstanceOfType(value, typeof(T),
                $"Expected value type {typeof(T).Name} but got {value.GetType().Name}");
            return (T)value;
        }

        /// <summary>
        /// Parses source and asserts it contains errors but document is still created (resilient parsing).
        /// </summary>
        /// <param name="source">TOON source to parse</param>
        /// <param name="errorSubstring">Optional substring that should appear in at least one error message</param>
        /// <returns>Parse result with errors but valid document</returns>
        public static ToonParseResult ParseWithErrors(string source, string? errorSubstring = null)
        {
            ToonParseResult result = Toon.Parse(source);
            Assert.IsTrue(result.HasErrors, "Expected parse to have errors");
            Assert.IsNotNull(result.Document, "Document should be created even with errors (resilient parsing)");

            if (errorSubstring != null)
            {
                ToonError? matchingError = result.Errors.FirstOrDefault(e =>
                    e.Message.Contains(errorSubstring, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(matchingError,
                    $"Expected error containing '{errorSubstring}'. Got: {string.Join("; ", result.Errors.Select(e => e.Message))}");
            }

            return result;
        }

        #endregion

        #region Array Helpers

        /// <summary>
        /// Asserts that an array has the expected declared size and actual element count.
        /// </summary>
        /// <param name="array">Array node to validate</param>
        /// <param name="expectedSize">Expected size</param>
        public static void AssertArraySize(ArrayNode array, int expectedSize)
        {
            Assert.AreEqual(expectedSize, array.DeclaredSize,
                $"Array declared size mismatch: expected {expectedSize}, got {array.DeclaredSize}");
            Assert.HasCount(expectedSize, array.Elements,
                $"Array element count mismatch: expected {expectedSize}, got {array.Elements.Count}");
        }

        /// <summary>
        /// Asserts that an array contains the expected string values.
        /// </summary>
        /// <param name="array">Array node to validate</param>
        /// <param name="expectedValues">Expected string values in order</param>
        public static void AssertArrayElements(ArrayNode array, params string[] expectedValues)
        {
            Assert.HasCount(expectedValues.Length, array.Elements,
                $"Array element count mismatch: expected {expectedValues.Length}, got {array.Elements.Count}");

            for (int i = 0; i < expectedValues.Length; i++)
            {
                Assert.IsInstanceOfType(array.Elements[i], typeof(StringValueNode),
                    $"Element {i} is not a StringValueNode");
                var element = (StringValueNode)array.Elements[i];
                Assert.AreEqual(expectedValues[i], element.Value,
                    $"Element {i} mismatch: expected '{expectedValues[i]}', got '{element.Value}'");
            }
        }

        #endregion

        #region Table Array Helpers

        /// <summary>
        /// Asserts that a table array has the expected structure.
        /// </summary>
        /// <param name="table">Table array node to validate</param>
        /// <param name="expectedRows">Expected number of rows</param>
        /// <param name="expectedSchema">Expected schema field names</param>
        public static void AssertTableStructure(TableArrayNode table, int expectedRows, params string[] expectedSchema)
        {
            Assert.AreEqual(expectedRows, table.DeclaredSize,
                $"Table declared size mismatch: expected {expectedRows}, got {table.DeclaredSize}");
            Assert.HasCount(expectedRows, table.Rows,
                $"Table row count mismatch: expected {expectedRows}, got {table.Rows.Count}");
            CollectionAssert.AreEqual(expectedSchema, table.Schema.ToArray(),
                $"Schema mismatch: expected [{string.Join(", ", expectedSchema)}], got [{string.Join(", ", table.Schema)}]");
        }

        /// <summary>
        /// Asserts that a specific table cell contains the expected string value.
        /// </summary>
        /// <param name="table">Table array node</param>
        /// <param name="rowIndex">Row index (0-based)</param>
        /// <param name="fieldIndex">Field index (0-based)</param>
        /// <param name="expectedValue">Expected string value</param>
        public static void AssertTableCellValue(TableArrayNode table, int rowIndex, int fieldIndex, string expectedValue)
        {
            Assert.IsLessThan(table.Rows.Count,
rowIndex, $"Row index {rowIndex} out of range (row count: {table.Rows.Count})");
            Assert.IsLessThan(table.Rows[rowIndex].Count,
fieldIndex, $"Field index {fieldIndex} out of range in row {rowIndex} (field count: {table.Rows[rowIndex].Count})");

            AstNode cell = table.Rows[rowIndex][fieldIndex];
            Assert.IsInstanceOfType(cell, typeof(StringValueNode),
                $"Cell [{rowIndex},{fieldIndex}] is not a StringValueNode");
            var value = (StringValueNode)cell;
            Assert.AreEqual(expectedValue, value.Value,
                $"Cell [{rowIndex},{fieldIndex}] mismatch: expected '{expectedValue}', got '{value.Value}'");
        }

        #endregion

        #region Error Validation Helpers

        /// <summary>
        /// Asserts that a parse result has at least one error matching the criteria.
        /// </summary>
        /// <param name="result">Parse result to check</param>
        /// <param name="errorSubstring">Substring that should appear in error message</param>
        /// <param name="errorCode">Optional error code to check</param>
        public static void AssertHasError(ToonParseResult result, string errorSubstring, string? errorCode = null)
        {
            Assert.IsTrue(result.HasErrors, "Expected parse result to have errors");

            ToonError? matchingError = result.Errors.FirstOrDefault(e =>
                e.Message.Contains(errorSubstring, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(matchingError,
                $"Expected error containing '{errorSubstring}'. Got: {string.Join("; ", result.Errors.Select(e => e.Message))}");

            if (errorCode != null)
            {
                Assert.AreEqual(errorCode, matchingError.Code,
                    $"Error code mismatch: expected '{errorCode}', got '{matchingError.Code}'");
            }
        }

        /// <summary>
        /// Asserts that a parse result has at least the specified number of errors.
        /// </summary>
        /// <param name="result">Parse result to check</param>
        /// <param name="minimumErrorCount">Minimum expected error count</param>
        public static void AssertMinimumErrorCount(ToonParseResult result, int minimumErrorCount)
        {
            Assert.IsTrue(result.HasErrors, "Expected parse result to have errors");
            Assert.IsGreaterThanOrEqualTo(minimumErrorCount, result.Errors.Count,
                $"Expected at least {minimumErrorCount} errors, got {result.Errors.Count}");
        }

        #endregion

        #region Value Helpers

        /// <summary>
        /// Gets a string value from a property.
        /// </summary>
        /// <param name="result">Parse result</param>
        /// <param name="propertyIndex">Property index (default: 0)</param>
        /// <returns>String value</returns>
        public static string GetStringValue(ToonParseResult result, int propertyIndex = 0)
        {
            StringValueNode node = ParseAndGetValue<StringValueNode>(result.Document.Properties[propertyIndex].Key + ": dummy", propertyIndex);
            return node.Value;
        }

        /// <summary>
        /// Gets a number value from a property.
        /// </summary>
        /// <param name="result">Parse result</param>
        /// <param name="propertyIndex">Property index (default: 0)</param>
        /// <returns>Number value</returns>
        public static double GetNumberValue(ToonParseResult result, int propertyIndex = 0)
        {
            AstNode value = result.Document.Properties[propertyIndex].Value;
            Assert.IsInstanceOfType<NumberValueNode>(value);
            return ((NumberValueNode)value).Value;
        }

        /// <summary>
        /// Gets a boolean value from a property.
        /// </summary>
        /// <param name="result">Parse result</param>
        /// <param name="propertyIndex">Property index (default: 0)</param>
        /// <returns>Boolean value</returns>
        public static bool GetBooleanValue(ToonParseResult result, int propertyIndex = 0)
        {
            AstNode value = result.Document.Properties[propertyIndex].Value;
            Assert.IsInstanceOfType<BooleanValueNode>(value);
            return ((BooleanValueNode)value).Value;
        }

        #endregion
    }
}
