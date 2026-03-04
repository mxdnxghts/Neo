using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System;

namespace Neo.Application.Caching;

/// <summary>
/// No-operation implementation of <see cref="IEquationCache"/>.
/// Used when caching is disabled or for testing purposes.
/// </summary>
public sealed class NullEquationCache : IEquationCache
{
    /// <summary>
    /// Always returns <c>null</c> as this cache never stores values.
    /// </summary>
    public Result<EquationSystem?> GetSystem(string key) =>
        Result<EquationSystem?>.Success(null);

    /// <summary>
    /// No-op: does not store the system.
    /// </summary>
    public void SetSystem(string key, EquationSystem system, TimeSpan? ttl = null)
    {
        // Intentionally empty - this cache does not store values
    }

    /// <summary>
    /// Always returns <c>null</c> as this cache never stores values.
    /// </summary>
    public Result<Solution?> GetSolution(string key) =>
        Result<Solution?>.Success(null);

    /// <summary>
    /// No-op: does not store the solution.
    /// </summary>
    public void SetSolution(string key, Solution solution, TimeSpan? ttl = null)
    {
        // Intentionally empty - this cache does not store values
    }

    /// <summary>
    /// No-op: nothing to clear.
    /// </summary>
    public void Clear()
    {
        // Intentionally empty - this cache does not store values
    }
}