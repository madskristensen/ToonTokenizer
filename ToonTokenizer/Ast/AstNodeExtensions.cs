using ToonTokenizer;

namespace ToonTokenizer.Ast
{
    /// <summary>
    /// Extension methods for setting position information on AST nodes.
    /// </summary>
    public static class AstNodeExtensions
    {
        /// <summary>
        /// Sets the position information on an AST node from a single token.
        /// </summary>
        /// <typeparam name="T">The type of AST node</typeparam>
        /// <param name="node">The AST node to set position on</param>
        /// <param name="token">The token to copy position information from</param>
        /// <returns>The same node (for method chaining)</returns>
        public static T WithPositionFrom<T>(this T node, Token token) where T : AstNode
        {
            node.StartLine = token.Line;
            node.StartColumn = token.Column;
            node.StartPosition = token.Position;
            node.EndLine = token.Line;
            node.EndColumn = token.Column + token.Length;
            node.EndPosition = token.Position + token.Length;
            return node;
        }

        /// <summary>
        /// Sets the position information on an AST node from a range of tokens.
        /// </summary>
        /// <typeparam name="T">The type of AST node</typeparam>
        /// <param name="node">The AST node to set position on</param>
        /// <param name="startToken">The token marking the start of the range</param>
        /// <param name="endToken">The token marking the end of the range</param>
        /// <returns>The same node (for method chaining)</returns>
        public static T WithPositionFromRange<T>(this T node, Token startToken, Token endToken) where T : AstNode
        {
            node.StartLine = startToken.Line;
            node.StartColumn = startToken.Column;
            node.StartPosition = startToken.Position;
            node.EndLine = endToken.Line;
            node.EndColumn = endToken.Column + endToken.Length;
            node.EndPosition = endToken.Position + endToken.Length;
            return node;
        }

        /// <summary>
        /// Sets the position information on an AST node from explicit values.
        /// </summary>
        /// <typeparam name="T">The type of AST node</typeparam>
        /// <param name="node">The AST node to set position on</param>
        /// <param name="startLine">Start line number (1-based)</param>
        /// <param name="startColumn">Start column number (1-based)</param>
        /// <param name="startPosition">Start position in source (0-based)</param>
        /// <param name="endLine">End line number (1-based)</param>
        /// <param name="endColumn">End column number (1-based)</param>
        /// <param name="endPosition">End position in source (0-based)</param>
        /// <returns>The same node (for method chaining)</returns>
        public static T WithPosition<T>(this T node, int startLine, int startColumn, int startPosition, 
                                        int endLine, int endColumn, int endPosition) where T : AstNode
        {
            node.StartLine = startLine;
            node.StartColumn = startColumn;
            node.StartPosition = startPosition;
            node.EndLine = endLine;
            node.EndColumn = endColumn;
            node.EndPosition = endPosition;
            return node;
        }
    }
}
