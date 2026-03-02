using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System;

namespace Neo.Application.Caching;

/// <summary>
/// Interface for caching equation systems and their solutions.
/// Used to avoid re-parsing and re-solving identical inputs.
/// </summary>
public interface IEquationCache
{
    /// <summary>
    /// Retrieves a cached equation system by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached system, or null if not found/expired.</returns>
    Result<EquationSystem?> GetSystem(string key);

    /// <summary>
    /// Stores an equation system in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="system">The equation system.</param>
    /// <param name="ttl">Optional time-to-live. Uses default if not specified.</param>
    void SetSystem(string key, EquationSystem system, TimeSpan? ttl = null);

    /// <summary>
    /// Retrieves a cached solution by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached solution, or null if not found/expired.</returns>
    Result<Solution?> GetSolution(string key);

    /// <summary>
    /// Stores a solution in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="solution">The solution.</param>
    /// <param name="ttl">Optional time-to-live. Uses default if not specified.</param>
    void SetSolution(string key, Solution solution, TimeSpan? ttl = null);
}