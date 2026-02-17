using Neo.Application.Solver;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;


namespace Neo.Application.Batch;

public sealed class ParallelBatchProcessor : IBatchProcessor
{
    private readonly IEquationSolver _solver;
    private readonly Subject<BatchProgress> _progressSubject = new();

    public IObservable<BatchProgress> Progress => _progressSubject.AsObservable();

    public ParallelBatchProcessor(IEquationSolver solver)
    {
        _solver = solver;
    }

    public async Task<IReadOnlyList<Result<Solution>>> ProcessAsync(
        IEnumerable<EquationSystem> systems,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellation = default)
    {
        var list = systems.ToList();
        var results = new Result<Solution>[list.Count];
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = cancellation
        };

        var stopwatch = Stopwatch.StartNew();
        int processed = 0, succeeded = 0, failed = 0;

        await Task.Run(() =>
        {
            Parallel.For(0, list.Count, options, i =>
            {
                cancellation.ThrowIfCancellationRequested();

                var result = _solver.Solve(list[i]);
                results[i] = result;

                Interlocked.Increment(ref processed);
                if (result.IsSuccess)
                    Interlocked.Increment(ref succeeded);
                else
                    Interlocked.Increment(ref failed);

                var progress = new BatchProgress(
                    Total: list.Count,
                    Processed: Volatile.Read(ref processed),
                    Succeeded: Volatile.Read(ref succeeded),
                    Failed: Volatile.Read(ref failed),
                    Elapsed: stopwatch.Elapsed
                );
                _progressSubject.OnNext(progress);
            });
        }, cancellation);

        stopwatch.Stop();
        _progressSubject.OnCompleted();
        return results;
    }
}