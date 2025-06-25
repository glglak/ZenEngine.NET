using ZenEngine.Core.Models;

namespace ZenEngine.Core.Loaders
{
    public interface IDecisionLoader
    {
        Task<DecisionContent> LoadAsync(string key);
    }

    public class NoopLoader : IDecisionLoader
    {
        public Task<DecisionContent> LoadAsync(string key)
        {
            throw new InvalidOperationException($"Cannot load decision '{key}' with NoopLoader. Use CreateDecision instead.");
        }
    }

    public class FilesystemLoader : IDecisionLoader
    {
        private readonly string _rootPath;
        private readonly bool _keepInMemory;
        private readonly Dictionary<string, DecisionContent> _cache = new();

        public FilesystemLoader(string rootPath, bool keepInMemory = false)
        {
            _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            _keepInMemory = keepInMemory;
        }

        public async Task<DecisionContent> LoadAsync(string key)
        {
            if (_keepInMemory && _cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var filePath = Path.Combine(_rootPath, key);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Decision file not found: {filePath}");
            }

            var content = await File.ReadAllTextAsync(filePath);
            var decision = System.Text.Json.JsonSerializer.Deserialize<DecisionContent>(content)
                ?? throw new InvalidOperationException($"Failed to deserialize decision from {filePath}");

            if (_keepInMemory)
            {
                _cache[key] = decision;
            }

            return decision;
        }
    }

    public class MemoryLoader : IDecisionLoader
    {
        private readonly Dictionary<string, DecisionContent> _decisions = new();

        public void Add(string key, DecisionContent decision)
        {
            _decisions[key] = decision;
        }

        public bool Remove(string key)
        {
            return _decisions.Remove(key);
        }

        public Task<DecisionContent> LoadAsync(string key)
        {
            if (_decisions.TryGetValue(key, out var decision))
            {
                return Task.FromResult(decision);
            }

            throw new KeyNotFoundException($"Decision not found: {key}");
        }
    }

    public delegate Task<DecisionContent> LoaderDelegate(string key);

    public class ClosureLoader : IDecisionLoader
    {
        private readonly LoaderDelegate _loader;

        public ClosureLoader(LoaderDelegate loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        public Task<DecisionContent> LoadAsync(string key)
        {
            return _loader(key);
        }
    }
}