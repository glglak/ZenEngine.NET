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

                // Handle simple mathematical expressions
                if (expression.Contains("*") || expression.Contains("+") || expression.Contains("-") || expression.Contains("/"))
                {
                    return EvaluateSimpleMath(expression, contextElement);
                }

                // Handle conditional expressions with ternary operator
                if (expression.Contains("?") && expression.Contains(":"))
                {
                    return EvaluateConditional(expression, contextElement);
                }

                // Handle comparison expressions
                if (expression.Contains(">=") || expression.Contains("<=") || expression.Contains(">") || expression.Contains("<") || expression.Contains("=="))
                {
                    return EvaluateComparison(expression, contextElement);
                }

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

        private object? EvaluateSimpleMath(string expression, JsonElement context)
        {
            // Handle simple arithmetic operations
            var operators = new[] { "*", "/", "+", "-" };
            
            foreach (var op in operators)
            {
                if (expression.Contains(op))
                {
                    var parts = expression.Split(new[] { op }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        var left = EvaluateOperand(parts[0].Trim(), context);
                        var right = EvaluateOperand(parts[1].Trim(), context);

                        if (left is double leftDouble && right is double rightDouble)
                        {
                            return op switch
                            {
                                "*" => leftDouble * rightDouble,
                                "/" => rightDouble != 0 ? leftDouble / rightDouble : 0,
                                "+" => leftDouble + rightDouble,
                                "-" => leftDouble - rightDouble,
                                _ => null
                            };
                        }
                    }
                    break; // Only handle the first operator found
                }
            }

            return expression;
        }

        private object? EvaluateConditional(string expression, JsonElement context)
        {
            // Handle ternary operator: condition ? trueValue : falseValue
            var questionIndex = expression.IndexOf('?');
            var colonIndex = expression.LastIndexOf(':');
            
            if (questionIndex > 0 && colonIndex > questionIndex)
            {
                var condition = expression[..questionIndex].Trim();
                var trueValue = expression[(questionIndex + 1)..colonIndex].Trim();
                var falseValue = expression[(colonIndex + 1)..].Trim();

                var conditionResult = EvaluateCondition(condition, context);
                return conditionResult 
                    ? EvaluateOperand(trueValue, context) 
                    : EvaluateOperand(falseValue, context);
            }

            return expression;
        }

        private object? EvaluateComparison(string expression, JsonElement context)
        {
            var operators = new[] { ">=", "<=", ">", "<", "==" };
            
            foreach (var op in operators)
            {
                if (expression.Contains(op))
                {
                    var parts = expression.Split(new[] { op }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        var left = EvaluateOperand(parts[0].Trim(), context);
                        var right = EvaluateOperand(parts[1].Trim(), context);

                        if (left is double leftDouble && right is double rightDouble)
                        {
                            return op switch
                            {
                                ">=" => leftDouble >= rightDouble,
                                "<=" => leftDouble <= rightDouble,
                                ">" => leftDouble > rightDouble,
                                "<" => leftDouble < rightDouble,
                                "==" => Math.Abs(leftDouble - rightDouble) < 0.0001,
                                _ => false
                            };
                        }

                        if (left is bool leftBool && right is bool rightBool)
                        {
                            return op == "==" ? leftBool == rightBool : false;
                        }

                        return op == "==" ? Equals(left, right) : false;
                    }
                    break;
                }
            }

            return false;
        }

        private object? EvaluateOperand(string operand, JsonElement context)
        {
            operand = operand.Trim();

            // Try to parse as number
            if (double.TryParse(operand, out var doubleValue))
            {
                return doubleValue;
            }

            // Try to parse as boolean
            if (bool.TryParse(operand, out var boolValue))
            {
                return boolValue;
            }

            // Try to parse as string literal
            if (operand.StartsWith('"') && operand.EndsWith('"'))
            {
                return operand[1..^1];
            }

            // Try to access as field
            if (operand.Contains('.'))
            {
                return EvaluateFieldAccess(operand, context);
            }

            // Try to access as property name
            if (context.TryGetProperty(operand, out var prop))
            {
                return GetJsonElementValue(prop);
            }

            return operand;
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
                JsonValueKind.Number => element.TryGetInt32(out var i) ? (double)i : element.GetDouble(),
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
