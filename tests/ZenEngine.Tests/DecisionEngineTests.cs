using FluentAssertions;
using System.Text.Json;
using Xunit;
using ZenEngine.Core;
using ZenEngine.Core.Loaders;
using ZenEngine.Core.Models;

namespace ZenEngine.Tests
{
    public class DecisionEngineTests
    {
        [Fact]
        public async Task SimpleDecision_ShouldEvaluateCorrectly()
        {
            // Arrange
            var decisionJson = """
            {
              "id": "test",
              "name": "Test",
              "nodes": {
                "input1": {
                  "id": "input1",
                  "name": "Input",
                  "type": "inputNode"
                },
                "output1": {
                  "id": "output1", 
                  "name": "Output",
                  "type": "outputNode"
                }
              },
              "edges": [
                {
                  "id": "edge1",
                  "sourceId": "input1",
                  "targetId": "output1"
                }
              ]
            }
            """;

            var content = JsonSerializer.Deserialize<DecisionContent>(decisionJson)!;
            var engine = DecisionEngine.Default;
            var decision = engine.CreateDecision(content);

            // Act
            var result = await decision.EvaluateAsync(new { input = 42 });

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().NotBeNull();
        }

        [Fact]
        public async Task ExpressionNode_ShouldTransformData()
        {
            // Arrange
            var decisionJson = """
            {
              "id": "expression-test",
              "name": "Expression Test",
              "nodes": {
                "input1": {
                  "id": "input1",
                  "name": "Input",
                  "type": "inputNode"
                },
                "expr1": {
                  "id": "expr1",
                  "name": "Expression",
                  "type": "expressionNode",
                  "content": {
                    "expressions": {
                      "doubled": "input * 2",
                      "nested.value": "input + 10"
                    }
                  }
                },
                "output1": {
                  "id": "output1",
                  "name": "Output", 
                  "type": "outputNode"
                }
              },
              "edges": [
                {
                  "id": "edge1",
                  "sourceId": "input1",
                  "targetId": "expr1"
                },
                {
                  "id": "edge2",
                  "sourceId": "expr1",
                  "targetId": "output1"
                }
              ]
            }
            """;

            var content = JsonSerializer.Deserialize<DecisionContent>(decisionJson)!;
            var engine = DecisionEngine.Default;
            var decision = engine.CreateDecision(content);

            // Act
            var result = await decision.EvaluateAsync(new { input = 5 });

            // Assert
            result.Should().NotBeNull();
            var resultDict = result.Result as Dictionary<string, object?>;
            resultDict.Should().NotBeNull();
            resultDict!["doubled"].Should().Be(10);
            
            var nested = resultDict["nested"] as Dictionary<string, object?>;
            nested.Should().NotBeNull();
            nested!["value"].Should().Be(15);
        }

        [Fact]
        public async Task MemoryLoader_ShouldLoadDecisionFromMemory()
        {
            // Arrange
            var content = new DecisionContent
            {
                Id = "memory-test",
                Name = "Memory Test",
                Nodes = new Dictionary<string, Node>
                {
                    ["input1"] = new() { Id = "input1", Name = "Input", Type = "inputNode" },
                    ["output1"] = new() { Id = "output1", Name = "Output", Type = "outputNode" }
                },
                Edges = new List<Edge>
                {
                    new() { Id = "edge1", SourceId = "input1", TargetId = "output1" }
                }
            };

            var memoryLoader = new MemoryLoader();
            memoryLoader.Add("test-decision", content);
            
            var engine = new DecisionEngine(memoryLoader);

            // Act
            var result = await engine.EvaluateAsync("test-decision", new { test = "data" });

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().NotBeNull();
        }

        [Fact]
        public async Task EvaluationOptions_WithTrace_ShouldIncludeTrace()
        {
            // Arrange
            var content = new DecisionContent
            {
                Id = "trace-test",
                Name = "Trace Test", 
                Nodes = new Dictionary<string, Node>
                {
                    ["input1"] = new() { Id = "input1", Name = "Input", Type = "inputNode" },
                    ["output1"] = new() { Id = "output1", Name = "Output", Type = "outputNode" }
                },
                Edges = new List<Edge>
                {
                    new() { Id = "edge1", SourceId = "input1", TargetId = "output1" }
                }
            };

            var engine = DecisionEngine.Default;
            var decision = engine.CreateDecision(content);

            // Act
            var result = await decision.EvaluateAsync(new { test = "data" }, new EvaluationOptions 
            { 
                IncludeTrace = true,
                IncludePerformance = true 
            });

            // Assert
            result.Should().NotBeNull();
            result.Trace.Should().NotBeNull();
            result.Trace.Should().HaveCountGreaterThan(0);
            result.Performance.Should().NotBeNull();
            result.Performance.Should().ContainKey("executionTimeMs");
        }
    }
}