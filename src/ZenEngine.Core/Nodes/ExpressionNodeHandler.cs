using ZenEngine.Core.Exceptions;
using ZenEngine.Core.Expressions;
using ZenEngine.Core.Models;

namespace ZenEngine.Core.Nodes
{
    public class ExpressionNodeHandler : BaseNodeHandler
    {
        private readonly IExpressionEvaluator _evaluator;

        public ExpressionNodeHandler(IExpressionEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public override string NodeType => "expressionNode";

        public override Task<object?> ExecuteAsync(Node node, object context, IDecisionEngine engine)
        {
            var expressionNode = GetNodeContent<ExpressionNode>(node);
            if (expressionNode?.Expressions == null)
                return Task.FromResult<object?>(context);

            var result = new Dictionary<string, object?>();

            foreach (var (field, expression) in expressionNode.Expressions)
            {
                try
                {
                    var value = _evaluator.Evaluate(expression, context);
                    SetNestedField(result, field, value);
                }
                catch (Exception ex)
                {
                    throw new NodeExecutionException(node.Id, node.Type, $"Expression '{expression}' failed: {ex.Message}");
                }
            }

            return Task.FromResult<object?>(result);
        }

        private void SetNestedField(Dictionary<string, object?> target, string fieldPath, object? value)
        {
            var parts = fieldPath.Split('.');
            var current = target;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (!current.ContainsKey(part))
                {
                    current[part] = new Dictionary<string, object?>();
                }
                current = (Dictionary<string, object?>)current[part]!;
            }

            current[parts.Last()] = value;
        }
    }
}