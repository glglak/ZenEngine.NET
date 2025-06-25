using ZenEngine.Core.Models;

namespace ZenEngine.Core.Nodes
{
    public interface INodeHandler
    {
        string NodeType { get; }
        Task<object?> ExecuteAsync(Node node, object context, IDecisionEngine engine);
    }

    public abstract class BaseNodeHandler : INodeHandler
    {
        public abstract string NodeType { get; }
        public abstract Task<object?> ExecuteAsync(Node node, object context, IDecisionEngine engine);

        protected T? GetNodeContent<T>(Node node) where T : class
        {
            if (node.Content == null) return null;
            
            var json = System.Text.Json.JsonSerializer.Serialize(node.Content);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }
}