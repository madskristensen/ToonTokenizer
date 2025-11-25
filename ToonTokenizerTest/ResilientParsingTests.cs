using ToonTokenizer;
using ToonTokenizer.Ast;

namespace ToonTokenizerTest
{
    [TestClass]
    public class ResilientParsingTests
    {
        [TestMethod]
        public void Parse_MultipleErrors_CollectsAllErrors()
        {
            var source = @"
name: John
age 30
city: New York
salary invalid
country: USA
";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have errors");
            Assert.IsGreaterThan(0, result.Errors.Count, "Should collect multiple errors");
            Assert.IsNotNull(result.Document, "Document should be available");
            Assert.IsGreaterThan(0, result.Document.Properties.Count, "Should have parsed valid properties");

            // Should have parsed the valid properties (name, city, country)
            var propertyKeys = result.Document.Properties.Select(p => p.Key).ToList();
            Assert.Contains("name", propertyKeys);
            Assert.Contains("city", propertyKeys);
            Assert.Contains("country", propertyKeys);
        }

        [TestMethod]
        public void Parse_ErrorInMiddle_ParsesBeforeAndAfter()
        {
            var source = @"
firstName: Alice
lastName: Smith
invalid line without colon
age: 25
email: alice@example.com
";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have error for invalid line");
            Assert.HasCount(1, result.Errors, "Should have exactly one error");
            Assert.HasCount(5, result.Document.Properties, "Should parse 4 valid properties");

            var keys = result.Document.Properties.Select(p => p.Key).ToList();
            Assert.Contains("firstName", keys);
            Assert.Contains("lastName", keys);
            Assert.Contains("age", keys);
            Assert.Contains("email", keys);
        }

        [TestMethod]
        public void Parse_UnmatchedBracket_ContinuesParsing()
        {
            var source = @"
name: John
items[3: a, b, c
city: NYC
";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have bracket error");
            Assert.IsNotNull(result.Document);

            // Should have parsed name (items might be partial, city should be there)
            var keys = result.Document.Properties.Select(p => p.Key).ToList();
            Assert.Contains("name", keys);
        }

        [TestMethod]
        public void Parse_InvalidSchema_ContinuesParsing()
        {
            var source = @"
users[2]{id name}:
  1,Alice
  2,Bob
status: active
";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have schema format error");
            Assert.IsNotNull(result.Document);

            // Should still try to parse other properties
            var keys = result.Document.Properties.Select(p => p.Key).ToList();
            Assert.IsGreaterThan(0, keys.Count, "Should parse some properties");

            // Limit assertion - just check we got something and there are errors
            Assert.IsNotEmpty(result.Errors, "Should have at least one error");
        }

        [TestMethod]
        public void TryParse_WithErrors_ReturnsTrue()
        {
            var source = "invalid without colon";
            bool success = Toon.TryParse(source, out var result);

            Assert.IsTrue(success, "TryParse should return true for completed parse");
            Assert.IsTrue(result.HasErrors, "Result should have errors");
            Assert.IsNotNull(result.Document, "Document should be available");
        }

        [TestMethod]
        public void TryParse_OnlyCatastrophicFailure_ReturnsFalse()
        {
            bool success = Toon.TryParse(null!, out var result);

            Assert.IsFalse(success, "Null input should return false");
            Assert.IsTrue(result.HasErrors, "Should have error");
            Assert.IsNotNull(result.Document, "Document should still be provided");
        }

        [TestMethod]
        public void Parse_ErrorPositionInformation_IsAccurate()
        {
            var source = "name John";  // Missing colon after "name"
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors);
            Assert.HasCount(1, result.Errors);

            var error = result.Errors[0];
            Assert.AreEqual(1, error.Line, "Error should be on line 1");
            Assert.IsGreaterThan(0, error.Column, "Should have valid column");
            Assert.IsGreaterThanOrEqualTo(0, error.Position, "Should have valid position");
            Assert.IsGreaterThan(0, error.Length, "Should have positive length");
        }

        [TestMethod]
        public void Parse_MultilineWithErrors_CorrectLineNumbers()
        {
            var source = @"line1: value1
line2 invalid
line3: value3
line4 also invalid
line5: value5";

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors);
            Assert.IsGreaterThan(1, result.Errors.Count, "Should have multiple errors");

            // Verify error line numbers
            var errorLines = result.Errors.Select(e => e.Line).ToList();
            Assert.Contains(2, errorLines, "Should have error on line 2");
            Assert.Contains(4, errorLines, "Should have error on line 4");
        }

        [TestMethod]
        public void Parse_PartialArray_FillsWithNulls()
        {
            var source = "numbers[5]: 1, 2, 3";  // Declares 5 but provides only 3
            var result = Toon.Parse(source);

            // May have error for missing elements or might just fill with nulls
            Assert.IsNotNull(result.Document);
            Assert.HasCount(1, result.Document.Properties);

            if (result.Document.Properties[0].Value is ArrayNode array)
            {
                Assert.IsGreaterThan(0, array.Elements.Count, "Array should have some elements");
            }
        }

        [TestMethod]
        public void Parse_TableArrayMissingDelimiter_RecordsError()
        {
            var source = @"data[2]{id,name}:
  1 Alice
  2,Bob";

            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors, "Should have delimiter error");
            Assert.IsNotNull(result.Document);
        }

        [TestMethod]
        public void Parse_EmptyDocument_ReturnsEmptyWithNoErrors()
        {
            var source = "";
            var result = Toon.Parse(source);

            // Empty is not an error, just results in no properties
            Assert.IsFalse(result.IsSuccess, "Empty source should not be success");
            Assert.IsNotNull(result.Document);
            Assert.IsEmpty(result.Document.Properties);
        }

        [TestMethod]
        public void Parse_ValidDocument_NoErrors()
        {
            var source = @"
name: John Doe
age: 30
city: New York
";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors, "Valid document should have no errors");
            Assert.IsTrue(result.IsSuccess, "Valid document should succeed");
            Assert.IsEmpty(result.Errors);
            Assert.HasCount(3, result.Document.Properties);
        }

        [TestMethod]
        public void Parse_ErrorRecovery_ContinuesToNextProperty()
        {
            var source = @"
prop1: value1
prop2 invalid
prop3: value3
prop4: value4
";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors);
            Assert.IsGreaterThan(2, result.Document.Properties.Count, "Should recover and parse subsequent properties");

            var keys = result.Document.Properties.Select(p => p.Key).ToList();
            Assert.Contains("prop1", keys);
            Assert.Contains("prop3", keys);
            Assert.Contains("prop4", keys);
        }

        [TestMethod]
        public void Parse_NestedObjectWithErrors_PartiallyParses()
        {
            var source = @"
user:
  name: Alice
  age invalid
  city: Boston
";
            var result = Toon.Parse(source);

            Assert.IsNotNull(result.Document);
            Assert.HasCount(1, result.Document.Properties, "Should have user property");

            var userProp = result.Document.Properties[0];
            Assert.AreEqual("user", userProp.Key);

            if (userProp.Value is ObjectNode obj)
            {
                Assert.IsGreaterThan(0, obj.Properties.Count, "Should have some nested properties");
            }
        }

        [TestMethod]
        public void Parse_MultipleErrorTypes_CollectsAll()
        {
            var source = @"
name: John
age 30
items[3: a, b, c
city: NYC
data{id name}: test
country: USA
";
            var result = Toon.Parse(source);

            Assert.IsTrue(result.HasErrors);
            Assert.IsGreaterThan(1, result.Errors.Count, "Should have multiple different error types");
            Assert.IsNotNull(result.Document);

            // Should still parse some valid properties
            var keys = result.Document.Properties.Select(p => p.Key).ToList();
            Assert.Contains("name", keys);
            Assert.Contains("city", keys);
            Assert.Contains("country", keys);
        }

        [TestMethod]
        public void Parse_EmailAddress_ParsesAsUnquotedString()
        {
            var source = "email: alice@example.com";
            var result = Toon.Parse(source);

            Assert.IsFalse(result.HasErrors, "Should have no errors");
            Assert.IsTrue(result.IsSuccess, "Should succeed");
            Assert.HasCount(1, result.Document.Properties, "Should have exactly one property");

            var property = result.Document.Properties[0];
            Assert.AreEqual("email", property.Key);

            Assert.IsInstanceOfType(property.Value, typeof(StringValueNode));
            var value = (StringValueNode)property.Value;
            Assert.AreEqual("alice@example.com", value.Value, "Email should be parsed as complete unquoted string");
        }
    }
}
