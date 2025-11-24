namespace ToonTokenizer.Ast
{
    /// <summary>
    /// Visitor interface for traversing the AST using the visitor pattern.
    /// </summary>
    public interface IAstVisitor<T>
    {
        T VisitDocument(ToonDocument node);
        T VisitProperty(PropertyNode node);
        T VisitObject(ObjectNode node);
        T VisitArray(ArrayNode node);
        T VisitTableArray(TableArrayNode node);
        T VisitStringValue(StringValueNode node);
        T VisitNumberValue(NumberValueNode node);
        T VisitBooleanValue(BooleanValueNode node);
        T VisitNullValue(NullValueNode node);
    }
}
