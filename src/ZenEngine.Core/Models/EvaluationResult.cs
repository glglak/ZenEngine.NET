using System.Text.Json.Serialization;

namespace ZenEngine.Core.Models
{
    public class EvaluationResult
    {
        [JsonPropertyName("result")]
        public object? Result { get; set; }

        [JsonPropertyName("trace")]
        public List<TraceNode>? Trace { get; set; }

        [JsonPropertyName("performance")]
        public Dictionary<string, object>? Performance { get; set; }
    }

    public class TraceNode
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public object? Input { get; set; }

        [JsonPropertyName("output")]
        public object? Output { get; set; }

        [JsonPropertyName("executionTime")]
        public double ExecutionTime { get; set; }
    }
}
