using System.Collections.Generic;

using ToonTokenizer.Ast;

namespace ToonTokenizer
{
    /// <summary>
    /// Provides extension methods for navigating between tokens and AST nodes.
    /// </summary>
    public static class TokenAstExtensions
    {
        /// <summary>
        /// Finds the most specific AST node that contains this token's position.
        /// </summary>
        /// <param name="token">The token to find the AST node for.</param>
        /// <param name="document">The parsed TOON document.</param>
        /// <returns>The AST node containing this token, or null if not found.</returns>
        public static AstNode? GetAstNode(this Token token, ToonDocument document)
        {
            if (document == null)
                return null;

            return FindNodeAtPosition(document, token.Position);
        }

        /// <summary>
        /// Finds the PropertyNode that contains this token, if any.
        /// </summary>
        /// <param name="token">The token to find the property for.</param>
        /// <param name="document">The parsed TOON document.</param>
        /// <returns>The PropertyNode containing this token, or null if not found.</returns>
        public static PropertyNode? GetPropertyNode(this Token token, ToonDocument document)
        {
            if (document == null)
                return null;

            var node = FindNodeAtPosition(document, token.Position);

            // If we found a PropertyNode directly, return it
            if (node is PropertyNode prop)
                return prop;

            // Otherwise, find the containing property
            return FindContainingProperty(document, token.Position);
        }

        /// <summary>
        /// Gets the AST node at the specified position in the document.
        /// </summary>
        /// <param name="result">The parse result.</param>
        /// <param name="position">The character position (0-based).</param>
        /// <returns>The AST node at that position, or null if not found.</returns>
        public static AstNode? GetNodeAtPosition(this ToonParseResult result, int position)
        {
            if (result?.Document == null)
                return null;

            return FindNodeAtPosition(result.Document, position);
        }

        /// <summary>
        /// Gets the AST node for a specific token.
        /// </summary>
        /// <param name="result">The parse result.</param>
        /// <param name="token">The token to find the AST node for.</param>
        /// <returns>The AST node containing the token, or null if not found.</returns>
        public static AstNode? GetNodeForToken(this ToonParseResult result, Token token)
        {
            if (result?.Document == null || token == null)
                return null;

            return token.GetAstNode(result.Document);
        }

        /// <summary>
        /// Gets the PropertyNode at the specified line and column.
        /// </summary>
        /// <param name="result">The parse result.</param>
        /// <param name="line">The line number (1-based).</param>
        /// <param name="column">The column number (1-based).</param>
        /// <returns>The PropertyNode at that position, or null if not found.</returns>
        public static PropertyNode? GetPropertyAt(this ToonParseResult result, int line, int column)
        {
            if (result?.Document == null || result.Tokens == null)
                return null;

            var token = result.Tokens.GetTokenAt(line, column);
            return token?.GetPropertyNode(result.Document);
        }

        /// <summary>
        /// Gets all PropertyNodes in the document (including nested ones).
        /// </summary>
        /// <param name="result">The parse result.</param>
        /// <returns>A list of all PropertyNodes in the document.</returns>
        public static List<PropertyNode> GetAllProperties(this ToonParseResult result)
        {
            var properties = new List<PropertyNode>();
            if (result?.Document == null)
                return properties;

            CollectProperties(result.Document, properties);
            return properties;
        }

        /// <summary>
        /// Finds a property by its key path (e.g., "user.settings.theme").
        /// </summary>
        /// <param name="result">The parse result.</param>
        /// <param name="path">The dot-separated path to the property.</param>
        /// <returns>The PropertyNode at that path, or null if not found.</returns>
        public static PropertyNode? FindPropertyByPath(this ToonParseResult result, string path)
        {
            if (result?.Document == null || string.IsNullOrEmpty(path))
                return null;

            return FindPropertyByPath(result.Document, path);
        }

        #region Private Helper Methods

        private static AstNode? FindNodeAtPosition(AstNode node, int position)
        {
            if (position < node.StartPosition || position > node.EndPosition)
                return null;

            // Check children first for most specific match
            switch (node)
            {
                case ToonDocument doc:
                    foreach (var prop in doc.Properties)
                    {
                        var result = FindNodeAtPosition(prop, position);
                        if (result != null)
                            return result;
                    }
                    break;

                case PropertyNode prop:
                    // Check if position is in the value area
                    var valueResult = FindNodeAtPosition(prop.Value, position);
                    if (valueResult != null)
                        return valueResult;
                    // Position is in property key area
                    return prop;

                case ObjectNode obj:
                    foreach (var prop in obj.Properties)
                    {
                        var result = FindNodeAtPosition(prop, position);
                        if (result != null)
                            return result;
                    }
                    break;

                case ArrayNode arr:
                    foreach (var elem in arr.Elements)
                    {
                        var result = FindNodeAtPosition(elem, position);
                        if (result != null)
                            return result;
                    }
                    break;

                case TableArrayNode table:
                    foreach (var row in table.Rows)
                    {
                        foreach (var cell in row)
                        {
                            var result = FindNodeAtPosition(cell, position);
                            if (result != null)
                                return result;
                        }
                    }
                    break;
            }

            // Return the current node if no child matched
            return node;
        }

        private static PropertyNode? FindContainingProperty(ToonDocument document, int position)
        {
            foreach (var prop in document.Properties)
            {
                if (position >= prop.StartPosition && position <= prop.EndPosition)
                    return prop;

                if (prop.Value is ObjectNode obj)
                {
                    var nested = FindContainingPropertyInObject(obj, position);
                    if (nested != null)
                        return nested;
                }
            }
            return null;
        }

        private static PropertyNode? FindContainingPropertyInObject(ObjectNode obj, int position)
        {
            foreach (var prop in obj.Properties)
            {
                if (position >= prop.StartPosition && position <= prop.EndPosition)
                    return prop;

                if (prop.Value is ObjectNode nested)
                {
                    var result = FindContainingPropertyInObject(nested, position);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        private static void CollectProperties(AstNode node, List<PropertyNode> properties)
        {
            switch (node)
            {
                case ToonDocument doc:
                    foreach (var prop in doc.Properties)
                    {
                        properties.Add(prop);
                        CollectProperties(prop.Value, properties);
                    }
                    break;

                case ObjectNode obj:
                    foreach (var prop in obj.Properties)
                    {
                        properties.Add(prop);
                        CollectProperties(prop.Value, properties);
                    }
                    break;

                case ArrayNode arr:
                    foreach (var elem in arr.Elements)
                    {
                        CollectProperties(elem, properties);
                    }
                    break;
            }
        }

        private static PropertyNode? FindPropertyByPath(ToonDocument document, string path)
        {
            var parts = path.Split('.');
            PropertyNode? current = null;

            // Start with document-level properties
            var properties = document.Properties;

            for (int i = 0; i < parts.Length; i++)
            {
                var key = parts[i];
                PropertyNode? found = null;

                foreach (var prop in properties)
                {
                    if (prop.Key == key)
                    {
                        found = prop;
                        break;
                    }
                }

                if (found == null)
                    return null;

                current = found;

                // If this is the last part, return it
                if (i == parts.Length - 1)
                    return current;

                // Otherwise, navigate deeper
                if (current.Value is ObjectNode obj)
                {
                    properties = obj.Properties;
                }
                else
                {
                    // Can't navigate deeper into non-object
                    return null;
                }
            }

            return current;
        }

        #endregion
    }
}
