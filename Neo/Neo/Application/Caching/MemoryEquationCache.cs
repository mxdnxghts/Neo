using Microsoft.Extensions.Caching.Memory;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System;
using System.Collections.Concurrent;

namespace Neo.Application.Caching;

/// <summary>
/// In-memory implementation of <see cref="IEquationCache"/> with TTL-based expiration.
/// Thread-safe using <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed class MemoryEquationCache : IEquationCache
{
    private readonly IMemoryCache _memoryCache;
    public MemoryEquationCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    /// <inheritdoc/>
    public Result<EquationSystem?> GetSystem(string key)
    {
        return _memoryCache.TryGetValue(key, out CacheEntry<EquationSystem> entry) && !entry.IsExpired
            ? Result<EquationSystem>.Success(entry.Value)
            : Result<EquationSystem>.Success(null);
    }

    /// <inheritdoc/>
    public void SetSystem(string key, EquationSystem system, TimeSpan? ttl = null)
    {
        var expires = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : DateTime.MaxValue;
        _memoryCache.Set(key, new CacheEntry<EquationSystem>(system, expires), expires);
    }

    /// <inheritdoc/>
    public Result<Solution?> GetSolution(string key)
    {
        return _memoryCache.TryGetValue(key, out CacheEntry<Solution> entry) && !entry.IsExpired
            ? Result<Solution>.Success(entry.Value)
            : Result<Solution>.Success(null);
    }

    /// <inheritdoc/>
    public void SetSolution(string key, Solution solution, TimeSpan? ttl = null)
    {
        var expires = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : DateTime.MaxValue;
        _memoryCache.Set(key, new CacheEntry<Solution>(solution, expires), expires);
    }

    /// <summary>
    /// Represents a cache entry with value and expiration time.
    /// </summary>
    /// <typeparam name="T">The type of cached value.</typeparam>
    private record CacheEntry<T>(T Value, DateTime Expires)
    {
        /// <summary>
        /// Gets a value indicating whether the entry has expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > Expires;
    }
}