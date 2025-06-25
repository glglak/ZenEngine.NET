using System.Text.Json;
using ZenEngine.Core.Expressions;
using ZenEngine.Core.Models;

namespace ZenEngine.Core.Nodes
{
    public class DecisionTableHandler : BaseNodeHandler
    {
        private readonly IExpressionEvaluator _evaluator;

        public DecisionTableHandler(IExpressionEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public override string NodeType => "decisionTableNode";

        public override Task<object?> ExecuteAsync(Node node, object context, IDecisionEngine engine)
        {
            var table = GetNodeContent<DecisionTable>(node);
            if (table == null)
                return Task.FromResult<object?>(null);

            var matchingRules = new List<Dictionary<string, object?>>();

            foreach (var rule in table.Rules)
            {
                if (EvaluateRule(rule, table.Inputs, context))
                {
                    var output = CreateRuleOutput(rule, table);
                    matchingRules.Add(output);

                    if (table.HitPolicy.Equals("first", StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult<object?>(output);
                    }
                }
            }

            return table.HitPolicy.Equals("collect", StringComparison.OrdinalIgnoreCase)
                ? Task.FromResult<object?>(matchingRules)
                : Task.FromResult<object?>(matchingRules.FirstOrDefault());
        }

        private bool EvaluateRule(List<object?> rule, List<DecisionTableInput> inputs, object context)
        {
            for (int i = 0; i < inputs.Count && i < rule.Count; i++)
            {
                var cellValue = rule[i];
                if (cellValue == null || string.IsNullOrWhiteSpace(cellValue.ToString()))
                    continue; // Empty cell is always true

                var condition = cellValue.ToString() ?? string.Empty;
                if (!_evaluator.EvaluateCondition(condition, context))
                    return false;
            }
            return true;
        }

        private Dictionary<string, object?> CreateRuleOutput(List<object?> rule, DecisionTable table)
        {
            var result = new Dictionary<string, object?>();
            var outputStartIndex = table.Inputs.Count;

            for (int i = 0; i < table.Outputs.Count; i++)
            {
                var outputIndex = outputStartIndex + i;
                if (outputIndex < rule.Count)
                {
                    var output = table.Outputs[i];
                    var value = rule[outputIndex];
                    
                    // Handle nested field names with dots
                    SetNestedField(result, output.Field, value);
                }
            }

            return result;
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