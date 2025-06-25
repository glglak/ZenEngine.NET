using ZenEngine.Core.Expressions;
using ZenEngine.Core.Models;

namespace ZenEngine.Core.Nodes
{
    public class SwitchNodeHandler : BaseNodeHandler
    {
        private readonly IExpressionEvaluator _evaluator;

        public SwitchNodeHandler(IExpressionEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public override string NodeType => "switchNode";

        public override Task<object?> ExecuteAsync(Node node, object context, IDecisionEngine engine)
        {
            var switchNode = GetNodeContent<SwitchNode>(node);
            if (switchNode?.Statements == null)
                return Task.FromResult<object?>(context);

            foreach (var statement in switchNode.Statements.Where(s => !s.IsDefault))
            {
                if (_evaluator.EvaluateCondition(statement.Condition, context))
                {
                    // In a real implementation, you'd follow the edge to the next node
                    // For now, we'll just pass through the context
                    return Task.FromResult<object?>(context);
                }
            }

            // If no condition matched, use default if available
            var defaultStatement = switchNode.Statements.FirstOrDefault(s => s.IsDefault);
            if (defaultStatement != null)
            {
                return Task.FromResult<object?>(context);
            }

            return Task.FromResult<object?>(null);
        }
    }
}