using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Application.Caching;

public interface IEquationCache
{
    Result<EquationSystem?> GetSystem(string key);
    void SetSystem(string key, EquationSystem system, TimeSpan? ttl = null);
    Result<Solution?> GetSolution(string key);
    void SetSolution(string key, Solution solution, TimeSpan? ttl = null);
    void Clear();
}

public interface IBatchProcessor
{
    IObservable<BatchProgress> Progress { get; }
    Task<IReadOnlyList<Result<Solution>>> ProcessAsync(
        IEnumerable<EquationSystem> systems,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellation = default);
}

public record BatchProgress(int Total, int Processed, int Succeeded, int Failed, TimeSpan Elapsed);