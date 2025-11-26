using System.Collections.Generic;

namespace ToonTokenizer.Ast
{
    /// <summary>
    /// Represents a property (key-value pair) in the TOON document.
    /// </summary>
    public class PropertyNode : AstNode
    {
        /// <summary>
        /// The property key/name.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The value of this property (can be a primitive, object, or array).
        /// </summary>
        public AstNode Value { get; set; } = null!;

        /// <summary>
        /// The indentation level of this property.
        /// </summary>
        public int IndentLevel { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitProperty(this);
        }
    }

    /// <summary>
    /// Represents an object (nested structure with properties).
    /// </summary>
    public class ObjectNode : AstNode
    {
        /// <summary>
        /// The properties contained in this object.
        /// </summary>
        public List<PropertyNode> Properties { get; }

        public ObjectNode()
        {
            Properties = [];
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitObject(this);
        }
    }

    /// <summary>
    /// Represents a simple array with elements.
    /// </summary>
    /// <remarks>
    /// Arrays in TOON can have an optional declared size: [n]. Per TOON spec §6.1,
    /// the parser validates that the actual number of elements matches the declared size.
    /// Use DeclaredSize property to check the declared size (-1 if not specified).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Inline array: items[3]: a,b,c
    /// var array = (ArrayNode)result.Document.Properties[0].Value;
    /// Console.WriteLine(array.DeclaredSize);  // 3
    /// Console.WriteLine(array.Elements.Count); // 3
    /// </code>
    /// </example>
    public class ArrayNode : AstNode
    {
        /// <summary>
        /// The elements in this array.
        /// </summary>
        public List<AstNode> Elements { get; }

        /// <summary>
        /// The declared size of the array (from [size] notation), or -1 if not specified.
        /// </summary>
        public int DeclaredSize { get; set; }

        public ArrayNode()
        {
            Elements = [];
            DeclaredSize = -1;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitArray(this);
        }
    }

    /// <summary>
    /// Represents a table-style array with schema definition.
    /// Example: arrayName[3]{id,name,value}: ...
    /// </summary>
    /// <remarks>
    /// Table arrays are TOON's killer feature for compact data representation.
    /// The schema defines field names once, and each row provides values in the same order.
    /// This is significantly more token-efficient than repeating keys in JSON arrays.
    /// </remarks>
    /// <example>
    /// <code>
    /// // TOON: users[2]{id,name}: 1,Alice 2,Bob
    /// var table = (TableArrayNode)result.Document.Properties[0].Value;
    /// Console.WriteLine(string.Join(",", table.Schema));  // "id,name"
    /// Console.WriteLine(table.Rows.Count);                 // 2
    /// Console.WriteLine(table.Rows[0][0]);                 // "1" (first row, id)
    /// Console.WriteLine(table.Rows[0][1]);                 // "Alice" (first row, name)
    /// </code>
    /// </example>
    public class TableArrayNode : AstNode
    {
        /// <summary>
        /// The declared size of the array.
        /// </summary>
        public int DeclaredSize { get; set; }

        /// <summary>
        /// The field names in the schema (from {field1,field2,...}).
        /// </summary>
        public List<string> Schema { get; }

        /// <summary>
        /// The rows of data. Each row is a list of values corresponding to the schema.
        /// </summary>
        public List<List<AstNode>> Rows { get; }

        public TableArrayNode()
        {
            Schema = [];
            Rows = [];
            DeclaredSize = -1;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitTableArray(this);
        }
    }
}
