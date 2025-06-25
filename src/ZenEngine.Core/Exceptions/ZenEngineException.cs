namespace ZenEngine.Core.Exceptions
{
    public class ZenEngineException : Exception
    {
        public ZenEngineException(string message) : base(message) { }
        public ZenEngineException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class NodeExecutionException : ZenEngineException
    {
        public string NodeId { get; }
        public string NodeType { get; }

        public NodeExecutionException(string nodeId, string nodeType, string message) 
            : base($"Error executing node '{nodeId}' of type '{nodeType}': {message}")
        {
            NodeId = nodeId;
            NodeType = nodeType;
        }
    }

    public class ExpressionEvaluationException : ZenEngineException
    {
        public string Expression { get; }

        public ExpressionEvaluationException(string expression, string message) 
            : base($"Error evaluating expression '{expression}': {message}")
        {
            Expression = expression;
        }
    }
}
