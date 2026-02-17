using Neo.Domain.Equation;
using Neo.Domain.Solution;
using Neo.Domain.Result;
using System;
using System.Collections.Concurrent;

namespace Neo.Application.Caching;

public sealed class MemoryEquationCache : IEquationCache
{
    private readonly ConcurrentDictionary<string, CacheEntry<EquationSystem>> _systemCache = new();
    private readonly ConcurrentDictionary<string, CacheEntry<Solution>> _solutionCache = new();

    public Result<EquationSystem?> GetSystem(string key) =>
        _systemCache.TryGetValue(key, out var entry) && !entry.IsExpired
            ? Result<EquationSystem?>.Success(entry.Value)
            : Result<EquationSystem?>.Success(null);

    public void SetSystem(string key, EquationSystem system, TimeSpan? ttl = null)
    {
        var expires = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : DateTime.MaxValue;
        _systemCache[key] = new CacheEntry<EquationSystem>(system, expires);
    }

    public Result<Solution?> GetSolution(string key) =>
        _solutionCache.TryGetValue(key, out var entry) && !entry.IsExpired
            ? Result<Solution?>.Success(entry.Value)
            : Result<Solution?>.Success(null);

    public void SetSolution(string key, Solution solution, TimeSpan? ttl = null)
    {
        var expires = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : DateTime.MaxValue;
        _solutionCache[key] = new CacheEntry<Solution>(solution, expires);
    }

    public void Clear()
    {
        _systemCache.Clear();
        _solutionCache.Clear();
    }

    private record CacheEntry<T>(T Value, DateTime Expires)
    {
        public bool IsExpired => DateTime.UtcNow > Expires;
    }
}