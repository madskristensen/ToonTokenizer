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
    public class ToonEncoder
    {
        private readonly ToonEncoderOptions _options;

        /// <summary>
        /// Creates a new ToonEncoder with default options.
        /// </summary>
        public ToonEncoder() : this(new ToonEncoderOptions())
        {
        }

        /// <summary>
        /// Creates a new ToonEncoder with the specified options.
        /// </summary>
        public ToonEncoder(ToonEncoderOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Encodes a JSON string to TOON format.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>A TOON formatted string.</returns>
        public string EncodeFromJson(string json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            using var document = JsonDocument.Parse(json);
            var sb = new StringBuilder();
            
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                EncodeObject(document.RootElement, sb, 0, _options.Delimiter);
            }
            else if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                // Root array - encode with a wrapper property
                sb.Append("items[");
                sb.Append(document.RootElement.GetArrayLength());
                sb.Append("]:");
                EncodeArrayElements(document.RootElement, sb, 0, _options.Delimiter);
            }
            else
            {
                // Root primitive - encode with a wrapper property
                sb.Append("value: ");
                EncodeValue(document.RootElement, sb, _options.Delimiter);
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
                    if (_options.UseTableArrays && TryEncodeAsTableArray(property.Name, value, sb, indentLevel, activeDelimiter))
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
                    EncodeValue(value, sb, _options.Delimiter);
                    sb.Append('\n');
                }
            }
        }

        private bool TryEncodeAsTableArray(string propertyName, JsonElement array, StringBuilder sb, int indentLevel, string activeDelimiter)
        {
            if (array.GetArrayLength() == 0)
                return false;

            // Check if all elements are objects with the same keys
            var firstElement = array.EnumerateArray().FirstOrDefault();
            if (firstElement.ValueKind != JsonValueKind.Object)
                return false;

            var schema = firstElement.EnumerateObject().Select(p => p.Name).ToList();
            if (schema.Count == 0)
                return false;

            // Verify all elements have the same schema and only primitive values
            foreach (var element in array.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                    return false;

                var props = element.EnumerateObject().ToList();
                if (props.Count != schema.Count)
                    return false;

                for (int i = 0; i < schema.Count; i++)
                {
                    if (props[i].Name != schema[i])
                        return false;
                    if (!IsPrimitive(props[i].Value))
                        return false;
                }
            }

            // Encode as table array
            sb.Append('[');
            sb.Append(array.GetArrayLength());
            sb.Append("]{");
            sb.Append(string.Join(",", schema.Select(EscapeKey)));
            sb.Append("}:\n");

            foreach (var element in array.EnumerateArray())
            {
                WriteIndent(sb, indentLevel + 1);
                // Tabular row cells use active delimiter for quoting
                var values = element.EnumerateObject().Select(p => FormatValue(p.Value, activeDelimiter));
                sb.Append(string.Join(",", values));
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
                    var props = element.EnumerateObject().ToList();
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
                            _options.UseTableArrays &&
                            IsTabularArray(firstProp.Value))
                        {
                            // §10 v3.0: Emit tabular header on hyphen line
                            sb.Append(EscapeKey(firstProp.Name));
                            var arrayLength = firstProp.Value.GetArrayLength();
                            var firstElem = firstProp.Value.EnumerateArray().FirstOrDefault();
                            var schema = firstElem.EnumerateObject().Select(p => p.Name).ToList();
                            sb.Append('[');
                            sb.Append(arrayLength);
                            sb.Append("]{");
                            sb.Append(string.Join(",", schema.Select(EscapeKey)));
                            sb.Append("}:\n");
                            
                            // Rows at depth +2
                            foreach (var rowElement in firstProp.Value.EnumerateArray())
                            {
                                WriteIndent(sb, indentLevel + 2);
                                var values = rowElement.EnumerateObject().Select(p => FormatValue(p.Value, activeDelimiter));
                                sb.Append(string.Join(",", values));
                                sb.Append('\n');
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
                                    EncodePropertyArray(props[i].Name, value, sb, indentLevel + 1, activeDelimiter);
                                }
                                else
                                {
                                    sb.Append(": ");
                                    EncodeValue(value, sb, _options.Delimiter);
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
                                EncodePropertyArray(firstProp.Name, value, sb, indentLevel, activeDelimiter);
                            }
                            else
                            {
                                sb.Append(": ");
                                EncodeValue(value, sb, _options.Delimiter);
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
                                    EncodePropertyArray(props[i].Name, value, sb, indentLevel + 1, activeDelimiter);
                                }
                                else
                                {
                                    sb.Append(": ");
                                    EncodeValue(value, sb, _options.Delimiter);
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

            var firstElement = array.EnumerateArray().FirstOrDefault();
            if (firstElement.ValueKind != JsonValueKind.Object)
                return false;

            var schema = firstElement.EnumerateObject().Select(p => p.Name).ToList();
            if (schema.Count == 0)
                return false;

            foreach (var element in array.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                    return false;

                var props = element.EnumerateObject().ToList();
                if (props.Count != schema.Count)
                    return false;

                for (int i = 0; i < schema.Count; i++)
                {
                    if (props[i].Name != schema[i])
                        return false;
                    if (!IsPrimitive(props[i].Value))
                        return false;
                }
            }

            return true;
        }

        private void EncodePropertyArray(string propertyName, JsonElement value, StringBuilder sb, int indentLevel, string activeDelimiter)
        {
            int arrayLength = value.GetArrayLength();
            
            if (_options.UseTableArrays && TryEncodeAsTableArray(propertyName, value, sb, indentLevel, activeDelimiter))
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
            var values = new List<string>();
            foreach (var element in array.EnumerateArray())
            {
                values.Add(FormatValue(element, activeDelimiter));
            }
            sb.Append(string.Join(",", values));
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
            if (Regex.IsMatch(value, @"^-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?$") || 
                Regex.IsMatch(value, @"^0\d+$"))
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
            if (!Regex.IsMatch(key, @"^[A-Za-z_][A-Za-z0-9_.]*$"))
                return $"\"{EscapeString(key)}\"";
            return key;
        }

        private void WriteIndent(StringBuilder sb, int indentLevel)
        {
            sb.Append(new string(' ', indentLevel * _options.IndentSize));
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
