using System.Collections.Generic;

namespace ToonTokenizer.Ast
{
    /// <summary>
    /// Represents the root document node containing all top-level properties.
    /// </summary>
    /// <remarks>
    /// This is the entry point of the AST (Abstract Syntax Tree) for a parsed TOON document.
    /// A document consists of zero or more top-level PropertyNode objects. Use the visitor
    /// pattern (via Accept method) to traverse the document tree.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Toon.Parse("name: John\nage: 30");
    /// var document = result.Document;
    /// foreach (var property in document.Properties) {
    ///     Console.WriteLine($"{property.Key}: {property.Value}");
    /// }
    /// </code>
    /// </example>
    public class ToonDocument : AstNode
    {
        /// <summary>
        /// The top-level properties in the document.
        /// </summary>
        public List<PropertyNode> Properties { get; }

        public ToonDocument()
        {
            Properties = [];
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitDocument(this);
        }
    }
}
