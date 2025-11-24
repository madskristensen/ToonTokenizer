using System.Collections.Generic;
using System.Text;

using ToonTokenizer.Ast;

namespace ToonTokenizer
{
    /// <summary>
    /// Provides methods to traverse and inspect the AST.
    /// </summary>
    public static class AstExtensions
    {
        /// <summary>
        /// Gets all properties at any level in the document.
        /// </summary>
        public static List<PropertyNode> GetAllProperties(this ToonDocument document)
        {
            var properties = new List<PropertyNode>();
            CollectProperties(document.Properties, properties);
            return properties;
        }

        private static void CollectProperties(List<PropertyNode> source, List<PropertyNode> target)
        {
            foreach (var prop in source)
            {
                target.Add(prop);

                if (prop.Value is ObjectNode obj)
                {
                    CollectProperties(obj.Properties, target);
                }
            }
        }

        /// <summary>
        /// Finds a property by key path (e.g., "parent.child.grandchild").
        /// </summary>
        public static PropertyNode? FindProperty(this ToonDocument document, string keyPath)
        {
            var parts = keyPath.Split('.');
            List<PropertyNode> current = document.Properties;

            foreach (var part in parts)
            {
                var prop = current.Find(p => p.Key == part);
                if (prop == null)
                    return null;

                if (parts[parts.Length - 1] == part)
                    return prop;

                if (prop.Value is ObjectNode obj)
                {
                    current = obj.Properties;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the depth of nesting for a property.
        /// </summary>
        public static int GetDepth(this PropertyNode property)
        {
            return property.IndentLevel / 2; // Assuming 2-space indentation
        }

        /// <summary>
        /// Converts the AST to a debug string representation.
        /// </summary>
        public static string ToDebugString(this AstNode node)
        {
            var sb = new StringBuilder();
            AppendDebugString(node, sb, 0);
            return sb.ToString();
        }

        private static void AppendDebugString(AstNode node, StringBuilder sb, int indent)
        {
            string indentStr = new(' ', indent * 2);

            switch (node)
            {
                case ToonDocument doc:
                    sb.AppendLine($"{indentStr}Document:");
                    foreach (var prop in doc.Properties)
                        AppendDebugString(prop, sb, indent + 1);
                    break;

                case PropertyNode prop:
                    sb.AppendLine($"{indentStr}Property: {prop.Key}");
                    AppendDebugString(prop.Value, sb, indent + 1);
                    break;

                case ObjectNode obj:
                    sb.AppendLine($"{indentStr}Object:");
                    foreach (var prop in obj.Properties)
                        AppendDebugString(prop, sb, indent + 1);
                    break;

                case ArrayNode arr:
                    sb.AppendLine($"{indentStr}Array[{arr.DeclaredSize}]:");
                    foreach (var elem in arr.Elements)
                        AppendDebugString(elem, sb, indent + 1);
                    break;

                case TableArrayNode table:
                    sb.AppendLine($"{indentStr}TableArray[{table.DeclaredSize}] {{{string.Join(",", table.Schema)}}}:");
                    foreach (var row in table.Rows)
                    {
                        sb.AppendLine($"{indentStr}  Row:");
                        foreach (var val in row)
                            AppendDebugString(val, sb, indent + 2);
                    }
                    break;

                case StringValueNode str:
                    sb.AppendLine($"{indentStr}String: \"{str.Value}\"");
                    break;

                case NumberValueNode num:
                    sb.AppendLine($"{indentStr}Number: {num.Value}");
                    break;

                case BooleanValueNode boolVal:
                    sb.AppendLine($"{indentStr}Boolean: {boolVal.Value}");
                    break;

                case NullValueNode _:
                    sb.AppendLine($"{indentStr}Null");
                    break;
            }
        }
    }
}
