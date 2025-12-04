using System;

namespace ToonTokenizer
{
    /// <summary>
    /// Configuration options for TOON parsing with security-focused resource limits.
    /// These limits protect against denial-of-service attacks and excessive resource consumption.
    /// </summary>
    /// <remarks>
    /// The default limits are designed to handle typical use cases while preventing abuse.
    /// For trusted input or specific requirements, limits can be increased via custom instances.
    /// 
    /// <para><b>Security Considerations:</b></para>
    /// <list type="bullet">
    /// <item>Always use limits when parsing untrusted input (user uploads, API payloads, etc.)</item>
    /// <item>Consider reducing limits for high-throughput scenarios to prevent resource exhaustion</item>
    /// <item>Monitor parser performance and adjust limits based on actual usage patterns</item>
    /// <item>Use <see cref="Unlimited"/> only for trusted, validated input sources</item>
    /// </list>
    /// </remarks>
    public class ToonParserOptions
    {
        private int _maxInputSize = DefaultMaxInputSize;
        private int _maxNestingDepth = DefaultMaxNestingDepth;
        private int _maxArraySize = DefaultMaxArraySize;
        private int _maxTokenCount = DefaultMaxTokenCount;
        private int _maxStringLength = DefaultMaxStringLength;

        /// <summary>
        /// Default maximum input size in bytes (10 MB).
        /// Sufficient for large configuration files and most use cases.
        /// </summary>
        public const int DefaultMaxInputSize = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// Default maximum nesting depth (100 levels).
        /// Prevents stack overflow from deeply nested structures.
        /// </summary>
        public const int DefaultMaxNestingDepth = 100;

        /// <summary>
        /// Default maximum array size (1 million elements).
        /// Prevents memory exhaustion from extremely large arrays.
        /// </summary>
        public const int DefaultMaxArraySize = 1_000_000;

        /// <summary>
        /// Default maximum token count (1 million tokens).
        /// Roughly equivalent to 10 MB of typical TOON content.
        /// </summary>
        public const int DefaultMaxTokenCount = 1_000_000;

        /// <summary>
        /// Default maximum string/key length (64 KB).
        /// Sufficient for any reasonable property name or value.
        /// </summary>
        public const int DefaultMaxStringLength = 65_536;

        /// <summary>
        /// Gets the default parser options with recommended security limits.
        /// Use this for parsing untrusted input.
        /// </summary>
        public static ToonParserOptions Default { get; } = new ToonParserOptions();

        /// <summary>
        /// Gets parser options with unlimited resource limits.
        /// <b>WARNING:</b> Use only for trusted input. Vulnerable to DoS attacks.
        /// </summary>
        /// <remarks>
        /// This configuration disables all security limits and should only be used when:
        /// <list type="bullet">
        /// <item>Input source is completely trusted and validated</item>
        /// <item>Running in an isolated/sandboxed environment</item>
        /// <item>Input size and structure are guaranteed to be reasonable</item>
        /// </list>
        /// For production systems processing user input, always use <see cref="Default"/> or custom limits.
        /// </remarks>
        public static ToonParserOptions Unlimited { get; } = new ToonParserOptions
        {
            MaxInputSize = int.MaxValue,
            MaxNestingDepth = int.MaxValue,
            MaxArraySize = int.MaxValue,
            MaxTokenCount = int.MaxValue,
            MaxStringLength = int.MaxValue
        };

        /// <summary>
        /// Gets or sets the maximum input size in bytes.
        /// Protects against memory exhaustion from extremely large files.
        /// </summary>
        /// <value>The maximum input size in bytes. Default is 10 MB.</value>
        /// <exception cref="ArgumentOutOfRangeException">Value is less than 1.</exception>
        /// <remarks>
        /// This limit applies to the total source string length before parsing begins.
        /// Recommended values:
        /// <list type="bullet">
        /// <item><b>Low-risk scenarios (config files):</b> 1-10 MB</item>
        /// <item><b>Medium-risk (user uploads):</b> 1-5 MB</item>
        /// <item><b>High-risk (public APIs):</b> 100 KB - 1 MB</item>
        /// </list>
        /// </remarks>
        public int MaxInputSize
        {
            get => _maxInputSize;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxInputSize must be at least 1 byte");
                _maxInputSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum nesting depth for objects and arrays.
        /// Protects against stack overflow from deeply nested structures.
        /// </summary>
        /// <value>The maximum nesting depth. Default is 100 levels.</value>
        /// <exception cref="ArgumentOutOfRangeException">Value is less than 1.</exception>
        /// <remarks>
        /// Nesting depth counts each level of object or array nesting:
        /// <code>
        /// root:          # depth 0
        ///   child:       # depth 1
        ///     nested:    # depth 2
        /// </code>
        /// Most real-world documents have depth &lt; 10. Deep nesting often indicates:
        /// <list type="bullet">
        /// <item>Generated or malformed data</item>
        /// <item>Potential DoS attack attempt</item>
        /// <item>Poor data structure design</item>
        /// </list>
        /// </remarks>
        public int MaxNestingDepth
        {
            get => _maxNestingDepth;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxNestingDepth must be at least 1");
                _maxNestingDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum array size (number of elements).
        /// Protects against memory exhaustion from extremely large arrays.
        /// </summary>
        /// <value>The maximum array size. Default is 1 million elements.</value>
        /// <exception cref="ArgumentOutOfRangeException">Value is less than 1.</exception>
        /// <remarks>
        /// This limit applies to both declared array size and actual element count.
        /// Large arrays can consume significant memory:
        /// <list type="bullet">
        /// <item><b>1,000 elements:</b> ~50-100 KB (typical)</item>
        /// <item><b>10,000 elements:</b> ~500 KB - 1 MB</item>
        /// <item><b>1,000,000 elements:</b> ~50-100 MB</item>
        /// </list>
        /// Consider reducing this limit for high-throughput APIs or memory-constrained environments.
        /// </remarks>
        public int MaxArraySize
        {
            get => _maxArraySize;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxArraySize must be at least 1");
                _maxArraySize = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum token count during lexical analysis.
        /// Protects against excessive tokenization time and memory usage.
        /// </summary>
        /// <value>The maximum token count. Default is 1 million tokens.</value>
        /// <exception cref="ArgumentOutOfRangeException">Value is less than 1.</exception>
        /// <remarks>
        /// Token count roughly correlates with document complexity:
        /// <list type="bullet">
        /// <item><b>Simple document (10 properties):</b> ~30-50 tokens</item>
        /// <item><b>Medium document (100 properties):</b> ~300-500 tokens</item>
        /// <item><b>Large document (1000 properties):</b> ~3,000-5,000 tokens</item>
        /// </list>
        /// This limit prevents tokenization of maliciously crafted files with excessive tokens.
        /// </remarks>
        public int MaxTokenCount
        {
            get => _maxTokenCount;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxTokenCount must be at least 1");
                _maxTokenCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum length for strings and property keys.
        /// Protects against memory exhaustion from extremely long strings.
        /// </summary>
        /// <value>The maximum string length in characters. Default is 64 KB (65,536 characters).</value>
        /// <exception cref="ArgumentOutOfRangeException">Value is less than 1.</exception>
        /// <remarks>
        /// This limit applies to:
        /// <list type="bullet">
        /// <item>Property keys (identifiers)</item>
        /// <item>String values (quoted and unquoted)</item>
        /// <item>Individual tokens during lexical analysis</item>
        /// </list>
        /// Typical usage:
        /// <list type="bullet">
        /// <item><b>Property keys:</b> Usually &lt; 100 characters</item>
        /// <item><b>String values:</b> Usually &lt; 10 KB</item>
        /// <item><b>Very long strings:</b> 10-64 KB (uncommon but valid)</item>
        /// </list>
        /// </remarks>
        public int MaxStringLength
        {
            get => _maxStringLength;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxStringLength must be at least 1");
                _maxStringLength = value;
            }
        }

        /// <summary>
        /// Creates a new instance with default security limits.
        /// </summary>
        public ToonParserOptions()
        {
        }

        /// <summary>
        /// Creates a custom parser options instance with specified limits.
        /// </summary>
        /// <param name="maxInputSize">Maximum input size in bytes.</param>
        /// <param name="maxNestingDepth">Maximum nesting depth.</param>
        /// <param name="maxArraySize">Maximum array size.</param>
        /// <param name="maxTokenCount">Maximum token count.</param>
        /// <param name="maxStringLength">Maximum string length.</param>
        /// <exception cref="ArgumentOutOfRangeException">Any parameter is less than 1.</exception>
        public ToonParserOptions(
            int maxInputSize = DefaultMaxInputSize,
            int maxNestingDepth = DefaultMaxNestingDepth,
            int maxArraySize = DefaultMaxArraySize,
            int maxTokenCount = DefaultMaxTokenCount,
            int maxStringLength = DefaultMaxStringLength)
        {
            MaxInputSize = maxInputSize;
            MaxNestingDepth = maxNestingDepth;
            MaxArraySize = maxArraySize;
            MaxTokenCount = maxTokenCount;
            MaxStringLength = maxStringLength;
        }

        /// <summary>
        /// Creates a copy of the current options with adjusted limits.
        /// </summary>
        /// <returns>A new <see cref="ToonParserOptions"/> instance with the same limits.</returns>
        public ToonParserOptions Clone()
        {
            return new ToonParserOptions(
                maxInputSize: MaxInputSize,
                maxNestingDepth: MaxNestingDepth,
                maxArraySize: MaxArraySize,
                maxTokenCount: MaxTokenCount,
                maxStringLength: MaxStringLength
            );
        }

        /// <summary>
        /// Returns a string representation of the current limits.
        /// </summary>
        public override string ToString()
        {
            return $"ToonParserOptions {{ " +
                   $"MaxInputSize={FormatBytes(MaxInputSize)}, " +
                   $"MaxNestingDepth={MaxNestingDepth}, " +
                   $"MaxArraySize={MaxArraySize:N0}, " +
                   $"MaxTokenCount={MaxTokenCount:N0}, " +
                   $"MaxStringLength={MaxStringLength:N0} }}";
        }

        private static string FormatBytes(int bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024.0 * 1024):F1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F1} KB";
            return $"{bytes} bytes";
        }
    }
}
