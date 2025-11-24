using System.Collections.Generic;

namespace ToonTokenizer.Ast
{
    /// <summary>
    /// Represents the root document node containing all top-level properties.
    /// </summary>
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
