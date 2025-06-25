# ZenEngine.NET

A .NET Core 8 implementation of the GoRules Zen Engine - an Open-Source Business Rules Engine (BRE) that executes JSON Decision Models (JDM).

## Features

- **Cross-platform**: Runs on .NET Core 8
- **JSON Decision Models**: Load and execute JDM files
- **Multiple Node Types**: Support for Input, Output, Decision Table, Expression, and Switch nodes
- **Flexible Loaders**: File system, memory, and custom loaders
- **Async/Await**: Full async support throughout
- **Tracing & Performance**: Optional execution tracing and performance metrics
- **Type Safe**: Strongly typed with nullable reference types

## Installation

```bash
dotnet add package ZenEngine.Core
```

## Quick Start

```csharp
using ZenEngine.Core;
using ZenEngine.Core.Models;
using System.Text.Json;

// Create a simple decision
var decisionJson = """
{
  "id": "simple-decision",
  "name": "Simple Decision", 
  "nodes": {
    "input1": {
      "id": "input1",
      "name": "Input",
      "type": "inputNode"
    },
    "expression1": {
      "id": "expression1", 
      "name": "Transform",
      "type": "expressionNode",
      "content": {
        "expressions": {
          "result": "input * 2"
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
      "targetId": "expression1"
    },
    {
      "id": "edge2",
      "sourceId": "expression1",
      "targetId": "output1"
    }
  ]
}
""";

// Execute the decision
var content = JsonSerializer.Deserialize<DecisionContent>(decisionJson)!;
var engine = DecisionEngine.Default;
var decision = engine.CreateDecision(content);

var result = await decision.EvaluateAsync(new { input = 15 });
Console.WriteLine(JsonSerializer.Serialize(result.Result));
// Output: {"result": 30}
```

## Node Types

### Input Node
Entry point for decision data.

### Output Node  
Final result of the decision execution.

### Decision Table Node
Spreadsheet-like rules evaluation with hit policies (first, collect).

### Expression Node
Transform data using expressions with support for nested field assignment.

### Switch Node
Conditional branching based on expression evaluation.

## Loaders

### NoopLoader (Default)
For direct decision creation without loading from external sources.

### FilesystemLoader
Load decisions from JSON files on disk.

```csharp
var engine = new DecisionEngine(new FilesystemLoader("./decisions", keepInMemory: true));
var result = await engine.EvaluateAsync("pricing.json", context);
```

### MemoryLoader
In-memory decision storage.

```csharp
var loader = new MemoryLoader();
loader.Add("my-decision", decisionContent);
var engine = new DecisionEngine(loader);
```

### ClosureLoader
Custom loading logic via delegates.

```csharp
var loader = new ClosureLoader(async key => {
    // Custom loading logic (database, API, etc.)
    return await LoadFromDatabase(key);
});
```

## Expression Language

The expression evaluator supports:
- Field access: `customer.age`, `input.value`
- Nested field assignment: `"nested.field": "expression"`
- Literals: numbers, strings, booleans
- Simple comparisons and operations

## Error Handling

- `ZenEngineException`: Base exception type
- `NodeExecutionException`: Node-specific execution errors
- `ExpressionEvaluationException`: Expression parsing/evaluation errors

## Tracing & Performance

```csharp
var options = new EvaluationOptions 
{
    IncludeTrace = true,
    IncludePerformance = true,
    MaxExecutionTimeMs = 5000
};

var result = await decision.EvaluateAsync(context, options);

// Access trace information
foreach (var trace in result.Trace)
{
    Console.WriteLine($"Node {trace.Id}: {trace.ExecutionTime}ms");
}

// Performance metrics
var executionTime = result.Performance["executionTimeMs"];
```

## Building from Source

```bash
# Clone the repository
git clone https://github.com/glglak/ZenEngine.NET.git
cd ZenEngine.NET

# Build the solution
dotnet build

# Run tests
dotnet test

# Run examples
cd examples/ZenEngine.Examples
dotnet run
```

## Compatibility

This implementation aims for compatibility with the original Rust Zen Engine:
- Same JDM format
- Similar API patterns  
- Compatible node types and behaviors
- Cross-language decision model sharing

## Official Implementations

For the official GoRules implementations, see:
- [Rust (Original)](https://github.com/gorules/zen)
- [NodeJS Bindings](https://www.npmjs.com/package/@gorules/zen-engine)
- [Python Bindings](https://pypi.org/project/zen-engine/)
- [Go Bindings](https://pkg.go.dev/github.com/gorules/zen-go)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## License

MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [GoRules](https://gorules.io/) for the original Zen Engine and JDM specification
- The .NET Community for excellent tooling and ecosystem
