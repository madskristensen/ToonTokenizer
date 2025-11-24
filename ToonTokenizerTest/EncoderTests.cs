using ToonTokenizer;

namespace ToonTokenizerTest
{
    [TestClass]
    public class EncoderTests
    {
        [TestMethod]
        public void Encode_SimpleObject_ProducesCorrectToon()
        {
            var json = @"{""name"":""John"",""age"":30,""active"":true}";
            var toon = Toon.Encode(json);

            Assert.Contains("name: John", toon);
            Assert.Contains("age: 30", toon);
            Assert.Contains("active: true", toon);
        }

        [TestMethod]
        public void Encode_NestedObject_ProducesCorrectToon()
        {
            var json = @"{""user"":{""name"":""Jane"",""email"":""jane@example.com""}}";
            var toon = Toon.Encode(json);

            Assert.Contains("user:", toon);
            Assert.Contains("  name: Jane", toon);
            Assert.Contains("  email: jane@example.com", toon);
        }

        [TestMethod]
        public void Encode_SimpleArray_ProducesInlineArray()
        {
            var json = @"{""colors"":[""red"",""green"",""blue""]}";
            var toon = Toon.Encode(json);

            Assert.Contains("colors[3]: red,green,blue", toon);
        }

        [TestMethod]
        public void Encode_ArrayOfNumbers_ProducesInlineArray()
        {
            var json = @"{""numbers"":[1,2,3,4,5]}";
            var toon = Toon.Encode(json);

            Assert.Contains("numbers[5]: 1,2,3,4,5", toon);
        }

        [TestMethod]
        public void Encode_ArrayOfObjects_ProducesTableArray()
        {
            var json = @"{""users"":[{""id"":1,""name"":""Alice""},{""id"":2,""name"":""Bob""}]}";
            var toon = Toon.Encode(json);

            Assert.Contains("users[2]{id,name}:", toon);
            Assert.Contains("1,Alice", toon);
            Assert.Contains("2,Bob", toon);
        }

        [TestMethod]
        public void Encode_ComplexStructure_ProducesCorrectToon()
        {
            var json = @"{
                ""app"": {
                    ""name"": ""MyApp"",
                    ""version"": ""1.0"",
                    ""features"": [""auth"", ""api"", ""ui""]
                }
            }";
            var toon = Toon.Encode(json);

            Assert.Contains("app:", toon);
            Assert.Contains("name: MyApp", toon);
            Assert.Contains("version: \"1.0\"", toon);
            Assert.Contains("features[3]: auth,api,ui", toon);
        }

        [TestMethod]
        public void Encode_StringWithSpaces_QuotesValue()
        {
            var json = @"{""message"":""Hello World""}";
            var toon = Toon.Encode(json);

            Assert.Contains("message: \"Hello World\"", toon);
        }

        [TestMethod]
        public void Encode_NullValue_ProducesNull()
        {
            var json = @"{""optional"":null}";
            var toon = Toon.Encode(json);

            Assert.Contains("optional: null", toon);
        }

        [TestMethod]
        public void Encode_BooleanValues_ProducesCorrectFormat()
        {
            var json = @"{""enabled"":true,""debug"":false}";
            var toon = Toon.Encode(json);

            Assert.Contains("enabled: true", toon);
            Assert.Contains("debug: false", toon);
        }

        [TestMethod]
        public void Encode_FloatingPointNumbers_PreservesDecimals()
        {
            var json = @"{""pi"":3.14,""e"":2.718}";
            var toon = Toon.Encode(json);

            Assert.Contains("pi: 3.14", toon);
            Assert.Contains("e: 2.718", toon);
        }

        [TestMethod]
        public void Encode_WithCustomIndent_UsesCorrectSpacing()
        {
            var json = @"{""user"":{""name"":""Test""}}";
            var options = new ToonEncoderOptions { IndentSize = 4 };
            var toon = Toon.Encode(json, options);

            Assert.Contains("    name: Test", toon);
        }

        [TestMethod]
        public void Encode_TableArraysDisabled_UsesListFormat()
        {
            var json = @"{""items"":[{""a"":1},{""a"":2}]}";
            var options = new ToonEncoderOptions { UseTableArrays = false };
            var toon = Toon.Encode(json, options);

            Assert.Contains("items[2]:", toon);
            Assert.Contains("- ", toon);
        }

        [TestMethod]
        public void Encode_EmptyArray_ProducesEmptyArrayNotation()
        {
            var json = @"{""items"":[]}";
            var toon = Toon.Encode(json);

            Assert.Contains("items[0]:", toon);
        }

        [TestMethod]
        public void Encode_NestedArrays_ProducesCorrectFormat()
        {
            var json = @"{""matrix"":[[1,2],[3,4]]}";
            var toon = Toon.Encode(json);

            Assert.Contains("matrix[2]:", toon);
            Assert.Contains("- [2]: 1,2", toon);
            Assert.Contains("- [2]: 3,4", toon);
        }

        [TestMethod]
        public void Encode_RoundTrip_ParsesBackToSameStructure()
        {
            var json = @"{""name"":""Test"",""value"":42,""active"":true}";
            var toon = Toon.Encode(json);
            var document = Toon.Parse(toon);

            Assert.HasCount(3, document.Properties);
            Assert.AreEqual("name", document.Properties[0].Key);
            Assert.AreEqual("value", document.Properties[1].Key);
            Assert.AreEqual("active", document.Properties[2].Key);
        }
    }
}
