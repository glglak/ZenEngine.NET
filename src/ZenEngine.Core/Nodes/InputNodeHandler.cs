using ZenEngine.Core.Models;

namespace ZenEngine.Core.Nodes
{
    public class InputNodeHandler : BaseNodeHandler
    {
        public override string NodeType => "inputNode";

        public override Task<object?> ExecuteAsync(Node node, object context, IDecisionEngine engine)
        {
            // Input node simply passes through the context
            return Task.FromResult<object?>(context);
        }
    }

    public class OutputNodeHandler : BaseNodeHandler
    {
        public override string NodeType => "outputNode";

        public override Task<object?> ExecuteAsync(Node node, object context, IDecisionEngine engine)
        {
            // Output node returns the context as the final result
            return Task.FromResult<object?>(context);
        }
    }
}