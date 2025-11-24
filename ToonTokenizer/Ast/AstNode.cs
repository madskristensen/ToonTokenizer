using System;

namespace ToonTokenizer.Ast
{
    /// <summary>
    /// Base class for all AST nodes in the TOON language.
    /// </summary>
    public abstract class AstNode
    {
        /// <summary>
        /// The starting line number of this node (1-based).
        /// </summary>
        public int StartLine { get; set; }
        
        /// <summary>
        /// The starting column number of this node (1-based).
        /// </summary>
        public int StartColumn { get; set; }
        
        /// <summary>
        /// The ending line number of this node (1-based).
        /// </summary>
        public int EndLine { get; set; }
        
        /// <summary>
        /// The ending column number of this node (1-based).
        /// </summary>
        public int EndColumn { get; set; }
        
        /// <summary>
        /// The absolute starting position in the source text (0-based).
        /// </summary>
        public int StartPosition { get; set; }
        
        /// <summary>
        /// The absolute ending position in the source text (0-based).
        /// </summary>
        public int EndPosition { get; set; }

        /// <summary>
        /// Accept a visitor for the visitor pattern.
        /// </summary>
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }
}
