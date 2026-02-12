using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Application;

public interface IEquationCache
{
    // Caching parsed systems
    Result<EquationSystem?> GetCachedSystem(string inputHash);
    void CacheSystem(string inputHash, EquationSystem system, TimeSpan? ttl = null);

    // Caching solutions
    Result<Solution?> GetCachedSolution(string systemHash);
    void CacheSolution(string systemHash, Solution solution, TimeSpan? ttl = null);

    // Statistics
    CacheStatistics GetStatistics();
    void Clear();
}

public interface IBatchProcessor
{
    // Parallel processing
    Task<IReadOnlyList<Result<Solution>>> ProcessBatchAsync(
        IEnumerable<EquationSystem> systems,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default);

    // Priority-based processing
    Task<IReadOnlyList<Result<Solution>>> ProcessWithPriorityAsync(
        IEnumerable<(EquationSystem System, int Priority)> systems,
        CancellationToken cancellationToken = default);

    // Progress reporting
    IObservable<BatchProgress> Progress { get; }
}

public record BatchProgress
{
    public int TotalItems { get; init; }
    public int ProcessedItems { get; init; }
    public int SuccessfulItems { get; init; }
    public int FailedItems { get; init; }
    public TimeSpan ElapsedTime { get; init; }
    public TimeSpan EstimatedRemainingTime { get; init; }
}