namespace ToonTokenizer.Ast
{
    /// <summary>
    /// Base class for all value nodes (primitives).
    /// </summary>
    public abstract class ValueNode : AstNode
    {
        /// <summary>
        /// The raw string representation of this value.
        /// </summary>
        public string RawValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a string value.
    /// </summary>
    public class StringValueNode : ValueNode
    {
        /// <summary>
        /// The parsed string value (without quotes if quoted).
        /// </summary>
        public string Value { get; set; } = string.Empty;

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitStringValue(this);
        }
    }

    /// <summary>
    /// Represents a numeric value (integer or floating-point).
    /// </summary>
    public class NumberValueNode : ValueNode
    {
        /// <summary>
        /// The parsed numeric value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Whether this number is an integer.
        /// </summary>
        public bool IsInteger { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitNumberValue(this);
        }
    }

    /// <summary>
    /// Represents a boolean value (true or false).
    /// </summary>
    public class BooleanValueNode : ValueNode
    {
        /// <summary>
        /// The boolean value.
        /// </summary>
        public bool Value { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitBooleanValue(this);
        }
    }

    /// <summary>
    /// Represents a null value.
    /// </summary>
    public class NullValueNode : ValueNode
    {
        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitNullValue(this);
        }
    }
}
