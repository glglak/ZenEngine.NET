using System.Text.Json;
using ZenEngine.Core.Exceptions;
using ZenEngine.Core.Loaders;
using ZenEngine.Core.Models;
using ZenEngine.Core.Nodes;
using ZenEngine.Core.Expressions;

namespace ZenEngine.Core
{
    public interface IDecision
    {
        Task<EvaluationResult> EvaluateAsync(object context, EvaluationOptions? options = null);
    }

    public interface IDecisionEngine
    {
        IDecision CreateDecision(DecisionContent content);
        Task<IDecision> GetDecisionAsync(string key);
        Task<EvaluationResult> EvaluateAsync(string key, object context, EvaluationOptions? options = null);
    }

    public class EvaluationOptions
    {
        public bool IncludeTrace { get; set; } = false;
        public bool IncludePerformance { get; set; } = false;
        public int MaxExecutionTimeMs { get; set; } = 30000; // 30 seconds default
    }

    public class Decision : IDecision
    {
        private readonly DecisionContent _content;
        private readonly Dictionary<string, INodeHandler> _nodeHandlers;
        private readonly IDecisionEngine _engine;

        public Decision(DecisionContent content, Dictionary<string, INodeHandler> nodeHandlers, IDecisionEngine engine)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _nodeHandlers = nodeHandlers ?? throw new ArgumentNullException(nameof(nodeHandlers));
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public static Decision From(DecisionContent content, Dictionary<string, INodeHandler> nodeHandlers, IDecisionEngine engine)
        {
            return new Decision(content, nodeHandlers, engine);
        }

        public async Task<EvaluationResult> EvaluateAsync(object context, EvaluationOptions? options = null)
        {
            options ??= new EvaluationOptions();
            var trace = options.IncludeTrace ? new List<TraceNode>() : null;
            var startTime = DateTime.UtcNow;

            try
            {
                using var cts = new CancellationTokenSource(options.MaxExecutionTimeMs);
                
                // Find the input node
                var inputNode = _content.Nodes.Values.FirstOrDefault(n => n.Type == "inputNode");
                if (inputNode == null)
                    throw new ZenEngineException("No input node found in decision graph");

                var result = await ExecuteNodeAsync(inputNode, context, trace, cts.Token);

                var endTime = DateTime.UtcNow;
                var performance = options.IncludePerformance
                    ? new Dictionary<string, object> { { "executionTimeMs", (endTime - startTime).TotalMilliseconds } }
                    : null;

                return new EvaluationResult
                {
                    Result = result,
                    Trace = trace,
                    Performance = performance
                };
            }
            catch (OperationCanceledException)
            {
                throw new ZenEngineException($"Decision evaluation timed out after {options.MaxExecutionTimeMs}ms");
            }
        }

        private async Task<object?> ExecuteNodeAsync(Node node, object context, List<TraceNode>? trace, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodeStartTime = DateTime.UtcNow;

            if (!_nodeHandlers.TryGetValue(node.Type, out var handler))
            {
                throw new ZenEngineException($"No handler found for node type: {node.Type}");
            }

            try
            {
                var result = await handler.ExecuteAsync(node, context, _engine);

                if (trace != null)
                {
                    trace.Add(new TraceNode
                    {
                        Id = node.Id,
                        Name = node.Name,
                        Type = node.Type,
                        Input = context,
                        Output = result,
                        ExecutionTime = (DateTime.UtcNow - nodeStartTime).TotalMilliseconds
                    });
                }

                // If this is an output node, return the result
                if (node.Type == "outputNode")
                {
                    return result;
                }

                // Find connected nodes and continue execution
                var connectedEdges = _content.Edges.Where(e => e.SourceId == node.Id).ToList();
                
                if (connectedEdges.Count == 0)
                {
                    return result; // No outgoing edges, return current result
                }

                // For simplicity, take the first connected node
                // In a more complex implementation, you'd handle multiple outputs based on switch conditions
                var nextEdge = connectedEdges.First();
                if (_content.Nodes.TryGetValue(nextEdge.TargetId, out var nextNode))
                {
                    return await ExecuteNodeAsync(nextNode, result ?? context, trace, cancellationToken);
                }

                return result;
            }
            catch (Exception ex) when (!(ex is ZenEngineException))
            {
                throw new NodeExecutionException(node.Id, node.Type, ex.Message);
            }
        }
    }

    public class DecisionEngine : IDecisionEngine
    {
        private readonly IDecisionLoader _loader;
        private readonly Dictionary<string, INodeHandler> _nodeHandlers;
        private readonly Dictionary<string, IDecision> _decisionCache = new();

        public DecisionEngine(IDecisionLoader? loader = null)
        {
            _loader = loader ?? new NoopLoader();
            _nodeHandlers = CreateDefaultNodeHandlers();
        }

        public static DecisionEngine Default => new();

        public IDecision CreateDecision(DecisionContent content)
        {
            return new Decision(content, _nodeHandlers, this);
        }

        public async Task<IDecision> GetDecisionAsync(string key)
        {
            if (_decisionCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var content = await _loader.LoadAsync(key);
            var decision = CreateDecision(content);
            _decisionCache[key] = decision;
            return decision;
        }

        public async Task<EvaluationResult> EvaluateAsync(string key, object context, EvaluationOptions? options = null)
        {
            var decision = await GetDecisionAsync(key);
            return await decision.EvaluateAsync(context, options);
        }

        private Dictionary<string, INodeHandler> CreateDefaultNodeHandlers()
        {
            var evaluator = new SimpleExpressionEvaluator();
            
            var handlers = new List<INodeHandler>
            {
                new InputNodeHandler(),
                new OutputNodeHandler(),
                new DecisionTableHandler(evaluator),
                new ExpressionNodeHandler(evaluator),
                new SwitchNodeHandler(evaluator)
            };

            return handlers.ToDictionary(h => h.NodeType, h => h);
        }
    }
}
