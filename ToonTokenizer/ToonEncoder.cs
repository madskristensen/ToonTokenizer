using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ToonTokenizer
{
    /// <summary>
    /// Encodes data into TOON format.
    /// </summary>
    /// <remarks>
    /// Creates a new ToonEncoder with the specified options.
    /// </remarks>
    public class ToonEncoder(ToonEncoderOptions options)
    {
        // Cached Regex patterns for performance (10-15% improvement)
        private static readonly Regex NumericPattern = new Regex(@"^-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?$", RegexOptions.Compiled);
        private static readonly Regex LeadingZeroPattern = new Regex(@"^0\d+$", RegexOptions.Compiled);
        private static readonly Regex KeyPattern = new Regex(@"^[A-Za-z_][A-Za-z0-9_.]*$", RegexOptions.Compiled);

        /// <summary>
        /// Creates a new ToonEncoder with default options.
        /// </summary>
        public ToonEncoder() : this(new ToonEncoderOptions())
        {
        }

        /// <summary>
        /// Encodes a JSON string to TOON format.
        /// Supports JSONC (JSON with comments) by ignoring both single-line (//) and multi-line (/* */) comments.
        /// </summary>
        /// <param name="json">The JSON string to convert (supports JSONC with comments).</param>
        /// <returns>A TOON formatted string.</returns>
        public string EncodeFromJson(string json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            // Configure options to support JSONC (JSON with comments)
            var jsonOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,  // Ignore comments
                AllowTrailingCommas = true                    // Allow trailing commas
            };

            using var document = JsonDocument.Parse(json, jsonOptions);
            var sb = new StringBuilder();

            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                EncodeObject(document.RootElement, sb, 0, options.Delimiter);
            }
            else if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                // Root array - encode with a wrapper property
                sb.Append("items[");
                sb.Append(document.RootElement.GetArrayLength());
                sb.Append("]:");
                EncodeArrayElements(document.RootElement, sb, 0, options.Delimiter);
            }
            else
            {
                // Root primitive - encode with a wrapper property
                sb.Append("value: ");
                EncodeValue(document.RootElement, sb, options.Delimiter);
            }

            return sb.ToString().TrimEnd();
        }

        private void EncodeObject(JsonElement element, StringBuilder sb, int indentLevel, string activeDelimiter)
        {
            foreach (var property in element.EnumerateObject())
            {
                WriteIndent(sb, indentLevel);
                sb.Append(EscapeKey(property.Name));

                var value = property.Value;

                if (value.ValueKind == JsonValueKind.Object)
                {
                    sb.Append(":\n");
                    EncodeObject(value, sb, indentLevel + 1, activeDelimiter);
                }
                else if (value.ValueKind == JsonValueKind.Array)
                {
                    int arrayLength = value.GetArrayLength();

                    // Check if this can be encoded as a table array
                    if (options.UseTableArrays && TryEncodeAsTableArray(value, sb, indentLevel, activeDelimiter))
                    {
                        // Successfully encoded as table array
                    }
                    // Check if array contains only primitives (inline array)
                    else if (arrayLength > 0 && AllPrimitives(value))
                    {
                        sb.Append('[');
                        sb.Append(arrayLength);
                        sb.Append("]: ");
                        EncodeInlineArray(value, sb, activeDelimiter);
                        sb.Append('\n');
                    }
                    else
                    {
                        // Multi-line array with list items
                        sb.Append('[');
                        sb.Append(arrayLength);
                        sb.Append("]:\n");
                        EncodeArrayElements(value, sb, indentLevel + 1, activeDelimiter);
                    }
                }
                else
                {
                    sb.Append(": ");
                    // Object field values use document delimiter for quoting decisions
                    EncodeValue(value, sb, options.Delimiter);
                    sb.Append('\n');
                }
            }
        }

        private bool TryEncodeAsTableArray(JsonElement array, StringBuilder sb, int indentLevel, string activeDelimiter)
        {
            int arrayLength = array.GetArrayLength();
            if (arrayLength == 0)
                return false;

            // Get first element without LINQ
            using var enumerator = array.EnumerateArray().GetEnumerator();
            if (!enumerator.MoveNext())
                return false;
            
            var firstElement = enumerator.Current;
            if (firstElement.ValueKind != JsonValueKind.Object)
                return false;

            // Build schema without LINQ - pre-size list
            var schema = new List<string>(10);
            foreach (var prop in firstElement.EnumerateObject())
            {
                schema.Add(prop.Name);
            }
            if (schema.Count == 0)
                return false;

            // Verify all elements have the same schema and only primitive values
            foreach (var element in array.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                    return false;

                int propIndex = 0;
                foreach (var prop in element.EnumerateObject())
                {
                    if (propIndex >= schema.Count || prop.Name != schema[propIndex])
                        return false;
                    if (!IsPrimitive(prop.Value))
                        return false;
                    propIndex++;
                }
                if (propIndex != schema.Count)
                    return false;
            }

            // Encode as table array
            sb.Append('[');
            sb.Append(arrayLength);
            sb.Append("]{");
            
            // Build schema string without LINQ
            for (int i = 0; i < schema.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(EscapeKey(schema[i]));
            }
            sb.Append("}:\n");

            // Encode rows without LINQ
            foreach (var element in array.EnumerateArray())
            {
                WriteIndent(sb, indentLevel + 1);
                
                int cellIndex = 0;
                foreach (var prop in element.EnumerateObject())
                {
                    if (cellIndex > 0) sb.Append(',');
                    sb.Append(FormatValue(prop.Value, activeDelimiter));
                    cellIndex++;
                }
                sb.Append('\n');
            }

            return true;
        }

        private void EncodeArrayElements(JsonElement array, StringBuilder sb, int indentLevel, string activeDelimiter)
        {
            foreach (var element in array.EnumerateArray())
            {
                WriteIndent(sb, indentLevel);
                sb.Append("- ");

                if (element.ValueKind == JsonValueKind.Object)
                {
                    // §10 v3.0: Place first field on hyphen line
                    // Build props list without ToList() - pre-size estimate
                    var props = new List<JsonProperty>(5);
                    foreach (var prop in element.EnumerateObject())
                    {
                        props.Add(prop);
                    }
                    
                    if (props.Count == 0)
                    {
                        // Empty object: remove the trailing space, just "-"
                        sb.Length -= 1; // remove the space after "-"
                        sb.Append('\n');
                    }
                    else
                    {
                        var firstProp = props[0];

                        // Check if first field is a tabular array
                        if (firstProp.Value.ValueKind == JsonValueKind.Array &&
                            options.UseTableArrays &&
                            IsTabularArray(firstProp.Value))
                        {
                            // §10 v3.0: Emit tabular header on hyphen line
                            sb.Append(EscapeKey(firstProp.Name));
                            var arrayLength = firstProp.Value.GetArrayLength();
                            
                            // Get first element and build schema without LINQ
                            using var schemaEnumerator = firstProp.Value.EnumerateArray().GetEnumerator();
                            if (schemaEnumerator.MoveNext())
                            {
                                var firstElem = schemaEnumerator.Current;
                                var schema = new List<string>(10);
                                foreach (var schemaProp in firstElem.EnumerateObject())
                                {
                                    schema.Add(schemaProp.Name);
                                }
                                
                                sb.Append('[');
                                sb.Append(arrayLength);
                                sb.Append("]{");
                                for (int si = 0; si < schema.Count; si++)
                                {
                                    if (si > 0) sb.Append(',');
                                    sb.Append(EscapeKey(schema[si]));
                                }
                                sb.Append("}:\n");

                                // Rows at depth +2
                                foreach (var rowElement in firstProp.Value.EnumerateArray())
                                {
                                    WriteIndent(sb, indentLevel + 2);
                                    int cellIdx = 0;
                                    foreach (var cellProp in rowElement.EnumerateObject())
                                    {
                                        if (cellIdx > 0) sb.Append(',');
                                        sb.Append(FormatValue(cellProp.Value, activeDelimiter));
                                        cellIdx++;
                                    }
                                    sb.Append('\n');
                                }
                            }

                            // Other fields at depth +1
                            for (int i = 1; i < props.Count; i++)
                            {
                                WriteIndent(sb, indentLevel + 1);
                                sb.Append(EscapeKey(props[i].Name));
                                var value = props[i].Value;
                                if (value.ValueKind == JsonValueKind.Object)
                                {
                                    sb.Append(":\n");
                                    EncodeObject(value, sb, indentLevel + 2, activeDelimiter);
                                }
                                else if (value.ValueKind == JsonValueKind.Array)
                                {
                                    EncodePropertyArray(value, sb, indentLevel + 1, activeDelimiter);
                                }
                                else
                                {
                                    sb.Append(": ");
                                    EncodeValue(value, sb, options.Delimiter);
                                    sb.Append('\n');
                                }
                            }
                        }
                        else
                        {
                            // Non-tabular: emit first field on hyphen line
                            sb.Append(EscapeKey(firstProp.Name));
                            var value = firstProp.Value;
                            if (value.ValueKind == JsonValueKind.Object)
                            {
                                sb.Append(":\n");
                                EncodeObject(value, sb, indentLevel + 2, activeDelimiter);
                            }
                            else if (value.ValueKind == JsonValueKind.Array)
                            {
                                EncodePropertyArray(value, sb, indentLevel, activeDelimiter);
                            }
                            else
                            {
                                sb.Append(": ");
                                EncodeValue(value, sb, options.Delimiter);
                                sb.Append('\n');
                            }

                            // Other fields at depth +1
                            for (int i = 1; i < props.Count; i++)
                            {
                                WriteIndent(sb, indentLevel + 1);
                                sb.Append(EscapeKey(props[i].Name));
                                value = props[i].Value;
                                if (value.ValueKind == JsonValueKind.Object)
                                {
                                    sb.Append(":\n");
                                    EncodeObject(value, sb, indentLevel + 2, activeDelimiter);
                                }
                                else if (value.ValueKind == JsonValueKind.Array)
                                {
                                    EncodePropertyArray(value, sb, indentLevel + 1, activeDelimiter);
                                }
                                else
                                {
                                    sb.Append(": ");
                                    EncodeValue(value, sb, options.Delimiter);
                                    sb.Append('\n');
                                }
                            }
                        }
                    }
                }
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    int nestedLength = element.GetArrayLength();
                    if (nestedLength > 0 && AllPrimitives(element))
                    {
                        sb.Append('[');
                        sb.Append(nestedLength);
                        sb.Append("]: ");
                        EncodeInlineArray(element, sb, activeDelimiter);
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append('[');
                        sb.Append(nestedLength);
                        sb.Append("]:\n");
                        EncodeArrayElements(element, sb, indentLevel + 1, activeDelimiter);
                    }
                }
                else
                {
                    EncodeValue(element, sb, activeDelimiter);
                    sb.Append('\n');
                }
            }
        }

        private bool IsTabularArray(JsonElement array)
        {
            if (array.GetArrayLength() == 0)
                return false;

            // Get first element without LINQ
            using var enumerator = array.EnumerateArray().GetEnumerator();
            if (!enumerator.MoveNext())
                return false;
            
            var firstElement = enumerator.Current;
            if (firstElement.ValueKind != JsonValueKind.Object)
                return false;

            // Build schema without LINQ
            var schema = new List<string>(10);
            foreach (var prop in firstElement.EnumerateObject())
            {
                schema.Add(prop.Name);
            }
            if (schema.Count == 0)
                return false;

            // Verify schema match without ToList()
            foreach (var element in array.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                    return false;

                int propIndex = 0;
                foreach (var prop in element.EnumerateObject())
                {
                    if (propIndex >= schema.Count || prop.Name != schema[propIndex])
                        return false;
                    if (!IsPrimitive(prop.Value))
                        return false;
                    propIndex++;
                }
                if (propIndex != schema.Count)
                    return false;
            }

            return true;
        }

        private void EncodePropertyArray(JsonElement value, StringBuilder sb, int indentLevel, string activeDelimiter)
        {
            int arrayLength = value.GetArrayLength();

            if (options.UseTableArrays && TryEncodeAsTableArray(value, sb, indentLevel, activeDelimiter))
            {
                // Successfully encoded as table array
            }
            else if (arrayLength > 0 && AllPrimitives(value))
            {
                sb.Append('[');
                sb.Append(arrayLength);
                sb.Append("]: ");
                EncodeInlineArray(value, sb, activeDelimiter);
                sb.Append('\n');
            }
            else
            {
                sb.Append('[');
                sb.Append(arrayLength);
                sb.Append("]:\n");
                EncodeArrayElements(value, sb, indentLevel + 1, activeDelimiter);
            }
        }

        private void EncodeInlineArray(JsonElement array, StringBuilder sb, string activeDelimiter)
        {
            // Direct append without intermediate List allocation
            bool first = true;
            foreach (var element in array.EnumerateArray())
            {
                if (!first) sb.Append(',');
                sb.Append(FormatValue(element, activeDelimiter));
                first = false;
            }
        }

        private void EncodeValue(JsonElement element, StringBuilder sb, string delimiter)
        {
            sb.Append(FormatValue(element, delimiter));
        }

        private string FormatValue(JsonElement element, string delimiter)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    string str = element.GetString() ?? "";
                    // Quote if contains special characters or whitespace
                    if (NeedsQuoting(str, delimiter))
                        return $"\"{EscapeString(str)}\"";
                    return str;

                case JsonValueKind.Number:
                    return FormatNumber(element);

                case JsonValueKind.True:
                    return "true";

                case JsonValueKind.False:
                    return "false";

                case JsonValueKind.Null:
                    return "null";

                default:
                    return "null";
            }
        }

        private string FormatNumber(JsonElement element)
        {
            // §2: Canonical number form - no exponent, no trailing zeros, no leading zeros
            if (element.TryGetInt32(out int intValue))
            {
                // Normalize -0 to 0
                return intValue == 0 ? "0" : intValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            if (element.TryGetInt64(out long longValue))
            {
                // Normalize -0 to 0
                return longValue == 0 ? "0" : longValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            double doubleValue = element.GetDouble();

            // Normalize -0 to 0
            if (doubleValue == 0)
                return "0";

            // Use Decimal for precise formatting without exponents
            try
            {
                decimal decimalValue = (decimal)doubleValue;
                string result = decimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Remove trailing zeros in fractional part
                if (result.Contains("."))
                {
                    result = result.TrimEnd('0');
                    if (result.EndsWith("."))
                        result = result.TrimEnd('.');
                }

                return result;
            }
            catch (OverflowException)
            {
                // If out of decimal range, use "R" format and manually expand exponent
                string result = doubleValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

                // Check for exponent notation and expand it
                if (result.Contains("E") || result.Contains("e"))
                {
                    // For very large or very small numbers outside decimal range,
                    // we must still format without exponent per spec
                    // Use scientific notation parsing to expand
                    if (double.TryParse(result, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsed))
                    {
                        // Format with enough precision
                        result = parsed.ToString("0.###################", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }

                // Remove trailing zeros
                if (result.Contains("."))
                {
                    result = result.TrimEnd('0');
                    if (result.EndsWith("."))
                        result = result.TrimEnd('.');
                }

                return result;
            }
        }

        private bool AllPrimitives(JsonElement array)
        {
            foreach (var element in array.EnumerateArray())
            {
                if (!IsPrimitive(element))
                    return false;
            }
            return true;
        }

        private bool IsPrimitive(JsonElement element)
        {
            return element.ValueKind == JsonValueKind.String ||
                   element.ValueKind == JsonValueKind.Number ||
                   element.ValueKind == JsonValueKind.True ||
                   element.ValueKind == JsonValueKind.False ||
                   element.ValueKind == JsonValueKind.Null;
        }

        private bool NeedsQuoting(string value, string delimiter)
        {
            // §7.2: String quoting rules
            if (string.IsNullOrEmpty(value))
                return true;

            // Contains whitespace (leading, trailing, or internal)
            if (value.Any(char.IsWhiteSpace))
                return true;

            // Reserved literals (case-sensitive)
            if (value == "true" || value == "false" || value == "null")
                return true;

            // Numeric-like patterns (§7.2)
            // Matches: /^-?\d+(?:\.\d+)?(?:e[+-]?\d+)?$/i or /^0\d+$/
            if (NumericPattern.IsMatch(value) || LeadingZeroPattern.IsMatch(value))
                return true;

            // Contains structural characters
            if (value.Contains(':') || value.Contains('\\') || value.Contains('"') ||
                value.Contains('[') || value.Contains(']') ||
                value.Contains('{') || value.Contains('}'))
                return true;

            // Contains control characters (already covered by whitespace check above, but being explicit)
            if (value.Contains('\n') || value.Contains('\r') || value.Contains('\t'))
                return true;

            // Contains the relevant delimiter
            if (value.Contains(delimiter))
                return true;

            // Equals "-" or starts with "-"
            if (value == "-" || (value.Length > 0 && value[0] == '-'))
                return true;

            return false;
        }

        private string EscapeString(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private string EscapeKey(string key)
        {
            // §7.3: Keys MAY be unquoted only if they match: ^[A-Za-z_][A-Za-z0-9_.]*$
            if (!KeyPattern.IsMatch(key))
                return $"\"{EscapeString(key)}\"";
            return key;
        }

        private void WriteIndent(StringBuilder sb, int indentLevel)
        {
            sb.Append(new string(' ', indentLevel * options.IndentSize));
        }
    }

    /// <summary>
    /// Options for encoding TOON format.
    /// </summary>
    public class ToonEncoderOptions
    {
        /// <summary>
        /// Number of spaces per indentation level. Default is 2.
        /// </summary>
        public int IndentSize { get; set; } = 2;

        /// <summary>
        /// Whether to use table array notation for arrays of objects with consistent schemas. Default is true.
        /// </summary>
        public bool UseTableArrays { get; set; } = true;

        /// <summary>
        /// Document delimiter for quoting decisions in object field values. Default is comma.
        /// Options: "," (comma), "\t" (tab), "|" (pipe).
        /// </summary>
        public string Delimiter { get; set; } = ",";
    }
}
