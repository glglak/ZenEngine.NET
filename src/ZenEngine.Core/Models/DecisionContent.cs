using System.Text.Json.Serialization;

namespace ZenEngine.Core.Models
{
    public class DecisionContent
    {
        [JsonPropertyName("nodes")]
        public Dictionary<string, Node> Nodes { get; set; } = new();

        [JsonPropertyName("edges")]
        public List<Edge> Edges { get; set; } = new();

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class Node
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public object? Content { get; set; }

        [JsonPropertyName("position")]
        public NodePosition? Position { get; set; }
    }

    public class NodePosition
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class Edge
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("sourceId")]
        public string SourceId { get; set; } = string.Empty;

        [JsonPropertyName("targetId")]
        public string TargetId { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "edge";
    }

    public class DecisionTable
    {
        [JsonPropertyName("hitPolicy")]
        public string HitPolicy { get; set; } = "first";

        [JsonPropertyName("inputs")]
        public List<DecisionTableInput> Inputs { get; set; } = new();

        [JsonPropertyName("outputs")]
        public List<DecisionTableOutput> Outputs { get; set; } = new();

        [JsonPropertyName("rules")]
        public List<List<object?>> Rules { get; set; } = new();
    }

    public class DecisionTableInput
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "expression";

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
    }

    public class DecisionTableOutput
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "expression";

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
    }

    public class ExpressionNode
    {
        [JsonPropertyName("expressions")]
        public Dictionary<string, string> Expressions { get; set; } = new();
    }

    public class SwitchNode
    {
        [JsonPropertyName("hitPolicy")]
        public string HitPolicy { get; set; } = "first";

        [JsonPropertyName("statements")]
        public List<SwitchStatement> Statements { get; set; } = new();
    }

    public class SwitchStatement
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }
    }

    public class FunctionNode
    {
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }
}
