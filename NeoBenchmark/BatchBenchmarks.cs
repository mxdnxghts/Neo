using BenchmarkDotNet.Attributes;
using Neo.Application.Caching;
using Neo.Application.Solver.Equation;
using Neo.Application.Solver.Matrix;
using Neo.Application.Validators;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Matrix;
using Neo.Infrastructure.Parsing;

namespace NeoBenchmark.Batch;

/// <summary>
/// Benchmarks for batch processing performance.
/// </summary>
[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class BatchBenchmarks
{
    private EquationSolver _solver = null!;
    private List<string> _batch10 = null!;
    private List<string> _batch100 = null!;
    private List<string> _batch1000 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _solver = CreateEquationSolver();
        _batch10 = GenerateEquations(10);
        _batch100 = GenerateEquations(100);
        _batch1000 = GenerateEquations(1000);
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<IReadOnlyList<Solution>>> Batch_10() 
        => await _solver.SolveBatchAsync(_batch10, CancellationToken.None);

    [Benchmark]
    public async Task<Result<IReadOnlyList<Solution>>> Batch_100() 
        => await _solver.SolveBatchAsync(_batch100, CancellationToken.None);

    [Benchmark]
    public async Task<Result<IReadOnlyList<Solution>>> Batch_1000() 
        => await _solver.SolveBatchAsync(_batch1000, CancellationToken.None);

    private static EquationSolver CreateEquationSolver()
    {
        var parser = new EquationParser();
        var converter = new MatrixConverter();
        var matrixSolver = new MatrixSolver();
        var validator = new SolutionValidator();
        var cache = new NullEquationCache();
        var options = new SolvingOptions
        {
            EnableCaching = false,
            DefaultAlgorithm = SolvingAlgorithm.LU,
            MaxDegreeOfParallelism = -1
        };

        return new EquationSolver(parser, converter, matrixSolver, validator, cache, options);
    }

    private static List<string> GenerateEquations(int count)
    {
        var equations = new List<string>();
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            var size = random.Next(2, 4); // 2x2 or 3x3
            var terms = new List<string>();
            
            for (int j = 0; j < size; j++)
            {
                var coeff = random.Next(1, 10);
                terms.Add($"{coeff}x{j + 1}");
            }
            
            var constant = random.Next(10, 100);
            equations.Add($"{string.Join(" + ", terms)} = {constant}");
        }

        return equations;
    }
}
