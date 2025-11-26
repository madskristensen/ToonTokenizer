using ToonTokenizer;

namespace ToonTokenizerTest
{
    /// <summary>
    /// Helper methods for encoder-focused tests.
    /// </summary>
    public static class ToonEncoderTestHelpers
    {
        /// <summary>
        /// Encodes JSON and asserts output contains all expected substrings.
        /// </summary>
        public static string EncodeContains(string json, params string[] expectedSubstrings)
        {
            var toon = Toon.Encode(json);
            foreach (var s in expectedSubstrings)
            {
                Assert.Contains(s, toon);
            }
            return toon;
        }

        /// <summary>
        /// Encodes JSON with options and asserts output contains all expected substrings.
        /// </summary>
        public static string EncodeContains(string json, ToonEncoderOptions options, params string[] expectedSubstrings)
        {
            var toon = Toon.Encode(json, options);
            foreach (var s in expectedSubstrings)
            {
                Assert.Contains(s, toon);
            }
            return toon;
        }

        /// <summary>
        /// Encodes JSON and asserts output does not contain any of the forbidden substrings.
        /// </summary>
        public static string EncodeNotContains(string json, params string[] forbiddenSubstrings)
        {
            var toon = Toon.Encode(json);
            foreach (var s in forbiddenSubstrings)
            {
                Assert.DoesNotContain(s, toon);
            }
            return toon;
        }
    }
}
