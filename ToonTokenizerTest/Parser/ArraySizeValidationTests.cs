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
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for size mismatch");
            var error = result.Errors.FirstOrDefault(e => e.Message.Contains("size") || e.Message.Contains("mismatch") || e.Message.Contains("expected"));
            Assert.IsNotNull(error, "Should have size mismatch error");
        }

        [TestMethod]
        public void Parse_InlineArrayExactSize_ParsesSuccessfully()
        {
            // Valid case: size matches
            var source = "items[3]: a,b,c";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors, "Should not have errors when size matches");
            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.AreEqual(3, array.DeclaredSize);
            Assert.HasCount(3, array.Elements);
        }

        [TestMethod]
        public void Parse_InlineArrayZeroSizeEmpty_ParsesSuccessfully()
        {
            // Valid case: declared 0, has 0 elements
            var source = "items[0]:";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors, "Should not have errors for empty array");
            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.AreEqual(0, array.DeclaredSize);
            Assert.IsEmpty(array.Elements);
        }

        [TestMethod]
        public void Parse_InlineArrayZeroSizeWithElements_ReturnsError()
        {
            // Invalid: declared 0 but has elements
            var source = "items[0]: a,b";
            var result = Toon.Parse(source);

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

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for expanded array size mismatch");
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

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for too many elements");
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

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for table array row count mismatch");
            var error = result.Errors.FirstOrDefault(e => e.Message.Contains("size") || e.Message.Contains("row") || e.Message.Contains("expected"));
            Assert.IsNotNull(error, "Should have row count mismatch error");
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

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for too many rows");
        }

        [TestMethod]
        public void Parse_TableArrayExactRowCount_ParsesSuccessfully()
        {
            // Valid case
            var source = @"users[2]{id,name}:
  1,Alice
  2,Bob";

            var result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors, "Should not have errors when row count matches");
            var table = (TableArrayNode)result.Document!.Properties[0].Value;
            Assert.AreEqual(2, table.DeclaredSize);
            Assert.HasCount(2, table.Rows);
        }

        [TestMethod]
        public void Parse_TableArrayZeroRows_ParsesSuccessfully()
        {
            // Valid empty table array
            var source = "users[0]{id,name}:\n";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors, "Should not have errors for empty table array");
            var table = (TableArrayNode)result.Document!.Properties[0].Value;
            Assert.AreEqual(0, table.DeclaredSize);
            Assert.IsEmpty(table.Rows);
        }

        [TestMethod]
        public void Parse_TableArrayZeroRowsWithData_ReturnsError()
        {
            // Invalid: declared 0 but has rows
            var source = @"users[0]{id,name}:
  1,Alice";

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors when 0-row table has data");
        }

        #endregion

        #region Multiple Arrays with Size Issues

        [TestMethod]
        public void Parse_MultipleArrays_FirstHasSizeMismatch_ReportsError()
        {
            // First array invalid, second valid
            var source = @"arr1[3]: a,b
arr2[2]: x,y";

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for first array");
            // Should still parse second array
            Assert.HasCount(2, result.Document!.Properties);
        }

        [TestMethod]
        public void Parse_MultipleArrays_BothHaveSizeMismatch_ReportsBothErrors()
        {
            // Both arrays invalid
            var source = @"arr1[3]: a,b
arr2[2]: x";

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.IsGreaterThanOrEqualTo(2, result.Errors.Count, "Should have at least 2 errors");
        }

        #endregion

        #region Nested Array Size Validation

        [TestMethod]
        public void Parse_NestedArrayWithSizeMismatch_ReportsError()
        {
            // Nested array in object with size mismatch
            var source = @"data:
  items[3]: a,b"; // Nested array with wrong size

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for nested array size mismatch");
        }

        [TestMethod]
        public void Parse_ArrayOfArrays_InnerArraySizeMismatch_ReportsError()
        {
            // Outer array valid, but inner array has size mismatch
            var source = @"matrix[2]:
  - [3]: 1,2
  - [2]: 3,4"; // First inner array: declared 3, has 2

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for inner array size mismatch");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_ArraySingleElement_SizeMismatch_ReturnsError()
        {
            // Declared 1, has 0
            var source = "item[1]:";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for missing single element");
        }

        [TestMethod]
        public void Parse_LargeArraySizeMismatch_ReportsError()
        {
            // Large array with mismatch
            var values = string.Join(",", Enumerable.Range(1, 99)); // 99 elements
            var source = $"numbers[100]: {values}"; // Declared 100

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors for large array size mismatch");
        }

        [TestMethod]
        public void Parse_ArrayWithNullElements_CountsNulls()
        {
            // Nulls should count as elements
            var source = "items[3]: a,null,b";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors, "Nulls should count as elements");
            var array = (ArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(3, array.Elements);
        }

        [TestMethod]
        public void Parse_TableArrayWithBlankRows_CountsRows()
        {
            // Rows with empty/null values should still count
            var source = @"data[2]{id,name}:
  1,
  2,Bob";

            var result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors, "Blank cells should not affect row count");
            var table = (TableArrayNode)result.Document!.Properties[0].Value;
            Assert.HasCount(2, table.Rows);
        }

        #endregion
    }
}
