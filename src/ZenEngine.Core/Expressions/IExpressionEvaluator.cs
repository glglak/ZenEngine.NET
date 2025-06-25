using System.Text.Json;
using ZenEngine.Core.Exceptions;

namespace ZenEngine.Core.Expressions
{
    public interface IExpressionEvaluator
    {
        object? Evaluate(string expression, object context);
        bool EvaluateCondition(string expression, object context);
    }

    // Simple expression evaluator - in production, you'd want something more sophisticated
    public class SimpleExpressionEvaluator : IExpressionEvaluator
    {
        public object? Evaluate(string expression, object context)
        {
            try
            {
                // Convert context to JsonElement for easy navigation
                var json = JsonSerializer.Serialize(context);
                var contextElement = JsonSerializer.Deserialize<JsonElement>(json);

                // Handle simple field access like "input.customer.age"
                if (expression.Contains('.'))
                {
                    return EvaluateFieldAccess(expression, contextElement);
                }

                // Handle literals
                if (expression.StartsWith('"') && expression.EndsWith('"'))
                {
                    return expression[1..^1]; // Remove quotes
                }

                if (int.TryParse(expression, out var intValue))
                {
                    return intValue;
                }

                if (double.TryParse(expression, out var doubleValue))
                {
                    return doubleValue;
                }

                if (bool.TryParse(expression, out var boolValue))
                {
                    return boolValue;
                }

                // Try to access as property name
                if (contextElement.TryGetProperty(expression, out var prop))
                {
                    return GetJsonElementValue(prop);
                }

                return expression; // Return as string if no other interpretation works
            }
            catch (Exception ex)
            {
                throw new ExpressionEvaluationException(expression, ex.Message);
            }
        }

        public bool EvaluateCondition(string expression, object context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                    return true; // Empty condition is always true

                var result = Evaluate(expression, context);
                
                if (result is bool boolResult)
                    return boolResult;

                if (result is string stringResult)
                    return !string.IsNullOrEmpty(stringResult);

                return result != null;
            }
            catch
            {
                return false; // Failed condition evaluation is false
            }
        }

        private object? EvaluateFieldAccess(string expression, JsonElement context)
        {
            var parts = expression.Split('.');
            var current = context;

            foreach (var part in parts)
            {
                if (current.TryGetProperty(part, out var prop))
                {
                    current = prop;
                }
                else
                {
                    return null;
                }
            }

            return GetJsonElementValue(current);
        }

        private object? GetJsonElementValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText()),
                JsonValueKind.Array => JsonSerializer.Deserialize<object[]>(element.GetRawText()),
                _ => null
            };
        }
    }
}