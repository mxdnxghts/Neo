using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Application.Batch;



public interface IBatchProcessor
{
    IObservable<BatchProgress> Progress { get; }
    Task<IReadOnlyList<Result<Solution>>> ProcessAsync(
        IEnumerable<EquationSystem> systems,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellation = default);
}

public record BatchProgress(int Total, int Processed, int Succeeded, int Failed, TimeSpan Elapsed);