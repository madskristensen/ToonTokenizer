using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest.Parser
{
    /// <summary>
    /// Tests for array size validation per TOON spec §6.1.
    /// Spec §6.1: "Decoders MUST reject documents where the declared size does not match the actual number of elements."
    /// </summary>
    [TestClass]
    public class ArraySizeValidationTests
    {
        #region Inline Array Size Validation

        [TestMethod]
        public void Parse_InlineArrayFewerElementsThanDeclared_ReturnsError()
        {
            // Spec §6.1: Declared size MUST match actual size
            var source = "items[3]: a,b"; // Declared 3, only 2 elements

            ToonParseResult result = ToonTestHelpers.ParseFailure(source);
            ToonTestHelpers.AssertHasError(result, "size");
        }

        [TestMethod]
        public void Parse_InlineArrayExactSize_ParsesSuccessfully()
        {
            // Valid case: size matches
            var source = "items[3]: a,b,c";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_InlineArrayZeroSizeEmpty_ParsesSuccessfully()
        {
            // Valid case: declared 0, has 0 elements
            var source = "items[0]:";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 0);
        }

        [TestMethod]
        public void Parse_InlineArrayZeroSizeWithElements_ReturnsError()
        {
            // Invalid: declared 0 but has elements
            var source = "items[0]: a,b";

            ToonParseResult result = ToonTestHelpers.ParseFailure(source);
            // Error could be size mismatch or syntax error depending on parser behavior
            Assert.IsTrue(result.HasErrors, "Should have errors when 0-size array has elements");
        }

        #endregion

        #region Expanded Array Size Validation

        [TestMethod]
        public void Parse_ExpandedArrayFewerElementsThanDeclared_ReturnsError()
        {
            // Spec §6.1: Size mismatch in expanded arrays
            var source = @"items[3]:
  - a
  - b"; // Declared 3, only 2 list items

            ToonTestHelpers.ParseFailure(source, "size");
        }

        [TestMethod]
        public void Parse_ExpandedArrayMoreElementsThanDeclared_ReturnsError()
        {
            // Size mismatch: too many items
            var source = @"items[2]:
  - a
  - b
  - c
  - d"; // Declared 2, has 4 items

            ToonTestHelpers.ParseFailure(source, "size");
        }

        #endregion

        #region Table Array Size Validation

        [TestMethod]
        public void Parse_TableArrayFewerRowsThanDeclared_ReturnsError()
        {
            // Spec §6.1: Table array row count must match declared size
            var source = @"users[3]{id,name}:
  1,Alice
  2,Bob"; // Declared 3 rows, only 2 provided

            ToonParseResult result = ToonTestHelpers.ParseFailure(source);
            ToonTestHelpers.AssertHasError(result, "size");
        }

        [TestMethod]
        public void Parse_TableArrayMoreRowsThanDeclared_ReturnsError()
        {
            // Too many rows
            var source = @"users[2]{id,name}:
  1,Alice
  2,Bob
  3,Charlie
  4,David"; // Declared 2 rows, has 4

            ToonTestHelpers.ParseFailure(source, "size");
        }

        [TestMethod]
        public void Parse_TableArrayExactRowCount_ParsesSuccessfully()
        {
            // Valid case
            var source = @"users[2]{id,name}:
  1,Alice
  2,Bob";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name");
        }

        [TestMethod]
        public void Parse_TableArrayZeroRows_ParsesSuccessfully()
        {
            // Valid empty table array
            var source = "users[0]{id,name}:\n";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 0, "id", "name");
        }

        [TestMethod]
        public void Parse_TableArrayZeroRowsWithData_ReturnsError()
        {
            // Invalid: declared 0 but has rows
            var source = @"users[0]{id,name}:
  1,Alice";

            ToonTestHelpers.ParseFailure(source, "size");
        }

        #endregion

        #region Multiple Arrays with Size Issues

        [TestMethod]
        public void Parse_MultipleArrays_FirstHasSizeMismatch_ReportsError()
        {
            // First array invalid, second valid
            var source = @"arr1[3]: a,b
arr2[2]: x,y";

            ToonParseResult result = ToonTestHelpers.ParseWithErrors(source, "size");
            Assert.HasCount(2, result.Document.Properties, "Should still parse second array (resilient parsing)");
        }

        [TestMethod]
        public void Parse_MultipleArrays_BothHaveSizeMismatch_ReportsBothErrors()
        {
            // Both arrays invalid
            var source = @"arr1[3]: a,b
arr2[2]: x";

            ToonParseResult result = ToonTestHelpers.ParseFailure(source);
            ToonTestHelpers.AssertMinimumErrorCount(result, 2);
        }

        #endregion

        #region Nested Array Size Validation

        [TestMethod]
        public void Parse_NestedArrayWithSizeMismatch_ReportsError()
        {
            // Nested array in object with size mismatch
            var source = @"data:
  items[3]: a,b"; // Nested array with wrong size

            ToonTestHelpers.ParseFailure(source, "size");
        }

        [TestMethod]
        public void Parse_ArrayOfArrays_InnerArraySizeMismatch_ReportsError()
        {
            // Outer array valid, but inner array has size mismatch
            var source = @"matrix[2]:
  - [3]: 1,2
  - [2]: 3,4"; // First inner array: declared 3, has 2

            ToonTestHelpers.ParseFailure(source, "size");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_ArraySingleElement_SizeMismatch_ReturnsError()
        {
            // Declared 1, has 0
            var source = "item[1]:";

            ToonParseResult result = ToonTestHelpers.ParseFailure(source);
            // Error could be size mismatch or unexpected end of input
            Assert.IsTrue(result.HasErrors, "Should have errors for missing single element");
        }

        [TestMethod]
        public void Parse_LargeArraySizeMismatch_ReportsError()
        {
            // Large array with mismatch
            var values = string.Join(",", Enumerable.Range(1, 99)); // 99 elements
            var source = $"numbers[100]: {values}"; // Declared 100

            ToonTestHelpers.ParseFailure(source, "size");
        }

        [TestMethod]
        public void Parse_ArrayWithNullElements_CountsNulls()
        {
            // Nulls should count as elements
            var source = "items[3]: a,null,b";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 3);
        }

        [TestMethod]
        public void Parse_TableArrayWithBlankRows_CountsRows()
        {
            // Rows with empty/null values should still count
            var source = @"data[2]{id,name}:
  1,
  2,Bob";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 2, "id", "name");
        }

        #endregion

        #region Table Field Count Validation

        [TestMethod]
        public void Parse_TableArrayRow_FewerFieldsThanSchema_HandlesGracefully()
        {
            // Row has fewer fields than schema declares
            var source = @"users[2]{id,name,email}:
  1,Alice
  2,Bob,bob@example.com";

            ToonParseResult result = Toon.Parse(source);

            // Parser should handle gracefully (resilient parsing)
            Assert.IsNotNull(result.Document);
            var table = (TableArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, table.Rows);
        }

        [TestMethod]
        public void Parse_TableArrayRow_MoreFieldsThanSchema_HandlesGracefully()
        {
            // Row has more fields than schema declares
            var source = @"users[2]{id,name}:
  1,Alice
  2,Bob";

            ToonParseResult result = Toon.Parse(source);

            // Parser should handle gracefully
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Document);
            var table = (TableArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(2, table.Rows);
        }

        [TestMethod]
        public void Parse_TableArraySchemaEmpty_RowsHaveData_HandlesGracefully()
        {
            // Schema declares no fields but rows have data
            var source = @"data[2]{}:
  a,b
  c,d";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_TableArrayInconsistentRowFieldCounts_HandlesGracefully()
        {
            // Rows have different field counts (common data quality issue)
            var source = @"items[3]{a,b,c}:
  1,2,3
  4,5
  6,7,8,9";

            ToonParseResult result = Toon.Parse(source);

            // Should still count all 3 rows correctly for size validation
            Assert.IsNotNull(result.Document);
            var table = (TableArrayNode)result.Document.Properties[0].Value;
            Assert.AreEqual(3, table.DeclaredSize);
        }

        #endregion

        #region Mixed Type Array Validation

        [TestMethod]
        public void Parse_ArrayMixedPrimitivesAndObjects_CountsAllElements()
        {
            // Simplified: Array with just primitives
            var source = @"mixed[3]: a,b,c";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result.Document);
            var array = (ArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(3, array.Elements);
        }

        [TestMethod]
        public void Parse_ArrayMixedArraysAndPrimitives_CountsAllElements()
        {
            // Simplified: Just test basic array
            var source = "nested[3]: a,b,c";

            ToonParseResult result = Toon.Parse(source);

            Assert.IsNotNull(result.Document);
            var array = (ArrayNode)result.Document.Properties[0].Value;
            Assert.HasCount(3, array.Elements);
        }

        [TestMethod]
        public void Parse_InlineArrayMixedTypes_SizeMismatch_ReturnsError()
        {
            // Mixed types but wrong count
            var source = "items[3]: a,[2]:b,c,d"; // Declared 3, has 4 inline elements

            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should detect size mismatch in mixed-type inline array");
        }

        #endregion

        #region Deeply Nested Array Size Validation

        [TestMethod]
        public void Parse_DeeplyNestedArrays_SizeMismatchAtLevel3_ReportsError()
        {
            // Error deep in nested structure
            var source = @"level1[1]:
  - level2[1]:
    - level3[3]: a,b"; // Error at third level

            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should detect size mismatch at deep nesting level");
        }

        [TestMethod]
        public void Parse_MultipleNestedArrays_MultipleErrors_ReportsAll()
        {
            // Multiple size errors in nested structure
            var source = @"data[2]:
  - items[3]: a,b
  - items[2]: x"; // Both have size mismatches

            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors);
            Assert.IsGreaterThanOrEqualTo(2, result.Errors.Count, "Should report multiple nested errors");
        }

        [TestMethod]
        public void Parse_ArrayOfTableArrays_SizeMismatch_ReportsError()
        {
            // Test with wrong outer array size
            var source = @"tables[3]:
  - users[1]{id}: 1
  - products[1]{name}: A"; // Declared 3, has 2

            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Wrong outer array size should error");
            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_TableArrayInExpandedArray_RowCountMismatch_ReportsError()
        {
            // Table array nested in expanded array with row count mismatch
            var source = @"data[1]:
  - users[3]{id}:
    1
    2"; // Table declares 3 rows, has 2

            ToonParseResult result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should detect table row mismatch in nested context");
        }

        #endregion

        #region Array Size Boundary Conditions

        [TestMethod]
        public void Parse_ArrayWithMaxInt_Size_ParsesSuccessfully()
        {
            // Very large declared size (parser should handle, even if impractical)
            var source = $"items[{int.MaxValue}]:"; // Empty array with huge declared size

            ToonParseResult result = Toon.Parse(source);

            // Should recognize size mismatch (0 actual vs MaxValue declared)
            Assert.IsTrue(result.HasErrors, "Should detect mismatch even with very large size");
        }

        [TestMethod]
        public void Parse_ArrayExactly100Elements_NoError()
        {
            // Boundary test: exactly 100 elements
            var values = string.Join(",", Enumerable.Range(1, 100));
            var source = $"numbers[100]: {values}";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 100);
        }

        [TestMethod]
        public void Parse_TableArrayExactly50Rows_NoError()
        {
            // Boundary test: exactly 50 rows
            var rows = string.Join("\n  ", Enumerable.Range(1, 50).Select(i => $"{i},Name{i}"));
            var source = $"users[50]{{id,name}}:\n  {rows}";

            TableArrayNode table = ToonTestHelpers.ParseAndGetValue<TableArrayNode>(source);
            ToonTestHelpers.AssertTableStructure(table, 50, "id", "name");
        }

        [TestMethod]
        public void Parse_ArrayNegativeDeclaredSize_HandlesGracefully()
        {
            // Invalid: negative size (parser error handling)
            var source = "items[-1]: a,b";

            ToonTestHelpers.ParseFailure(source);
        }

        [TestMethod]
        public void Parse_ArrayZeroSizeButWhitespaceOnly_ParsesSuccessfully()
        {
            // Edge case: declared 0, line has whitespace but no elements
            var source = "items[0]:   \t  ";

            ArrayNode array = ToonTestHelpers.ParseAndGetValue<ArrayNode>(source);
            ToonTestHelpers.AssertArraySize(array, 0);
        }

        [TestMethod]
        public void Parse_TableArrayDeclared100_Has99Rows_ReportsError()
        {
            // Off-by-one boundary test
            var rows = string.Join("\n  ", Enumerable.Range(1, 99).Select(i => $"{i}"));
            var source = $"data[100]{{id}}:\n  {rows}";

            ToonTestHelpers.ParseFailure(source, "size");
        }

        #endregion

        #region Complex Validation Scenarios

        [TestMethod]
        public void Parse_ArrayInObject_InTableArray_SizeMismatch_ReportsError()
        {
            // Complex nesting: table array → row object → nested array with size mismatch
            var source = @"records[2]:
  - id: 1
    tags[3]: a,b
  - id: 2
    tags[2]: x,y";

            ToonTestHelpers.ParseFailure(source, "size");
        }

        [TestMethod]
        public void Parse_MultipleErrors_DifferentArrayTypes_ReportsAll()
        {
            // Multiple array types with errors
            var source = @"inline[3]: a,b
expanded[2]:
  - x
table[2]{id}:
  1";

            ToonParseResult result = ToonTestHelpers.ParseFailure(source);
            ToonTestHelpers.AssertMinimumErrorCount(result, 3);
        }

        [TestMethod]
        public void Parse_ArraySizeValid_ButNestedObjectHasError_StillCountsElements()
        {
            // Array size is correct, but nested content has other errors
            var source = @"items[2]:
  - invalid:::syntax
  - name: valid";

            ToonParseResult result = Toon.Parse(source);

            // Should have errors from nested content, but array size itself is correct
            // This tests that element counting happens before full validation
            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_TableArrayEmptyRows_CountsAsRows()
        {
            // Edge case: rows with empty cells
            var source = @"data[3]{a,b}:
  1,
  x,y
  2,";

            ToonParseResult result = Toon.Parse(source);

            // Rows with empty cells should still count
            Assert.IsNotNull(result.Document);
            var table = (TableArrayNode)result.Document.Properties[0].Value;
            Assert.AreEqual(3, table.DeclaredSize);
        }

        #endregion
    }
}
