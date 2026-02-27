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

namespace NeoBenchmark.EndToEnd;

/// <summary>
/// End-to-end benchmarks measuring complete pipeline performance.
/// </summary>
[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class EndToEndBenchmarks
{
    private EquationSolver _solverWithCache = null!;
    private EquationSolver _solverNoCache = null!;
    private string _simpleEquation = null!;
    private string _complexEquation = null!;
    private List<string> _batchEquations = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleEquation = "2x + 3y = 8; x - y = 1";
        _complexEquation = GenerateComplexEquation(5);
        _batchEquations = GenerateEquations(100);

        _solverWithCache = CreateSolver(enableCache: true);
        _solverNoCache = CreateSolver(enableCache: false);

        // Warm up cache
        _solverWithCache.Solve(_simpleEquation);
        _solverWithCache.Solve(_complexEquation);
    }

    // Simple end-to-end scenarios
    [Benchmark(Baseline = true)]
    public Result<Solution> E2E_Simple_WithCache() 
        => _solverWithCache.Solve(_simpleEquation);

    [Benchmark]
    public Result<Solution> E2E_Simple_NoCache() 
        => _solverNoCache.Solve(_simpleEquation);

    // Complex end-to-end scenarios
    [Benchmark]
    public Result<Solution> E2E_Complex_WithCache() 
        => _solverWithCache.Solve(_complexEquation);

    [Benchmark]
    public Result<Solution> E2E_Complex_NoCache() 
        => _solverNoCache.Solve(_complexEquation);

    // Batch end-to-end scenarios
    [Benchmark]
    public async Task<Result<IReadOnlyList<Solution>>> E2E_Batch_WithCache() 
        => await _solverWithCache.SolveBatchAsync(_batchEquations, CancellationToken.None);

    [Benchmark]
    public async Task<Result<IReadOnlyList<Solution>>> E2E_Batch_NoCache() 
        => await _solverNoCache.SolveBatchAsync(_batchEquations, CancellationToken.None);

    // Repeated solve scenarios (cache benefit)
    [Benchmark]
    public Result<Solution> E2E_Repeated_WithCache() 
        => _solverWithCache.Solve(_simpleEquation);

    [Benchmark]
    public Result<Solution> E2E_Repeated_NoCache() 
        => _solverNoCache.Solve(_simpleEquation);

    private static EquationSolver CreateSolver(bool enableCache)
    {
        var parser = new EquationParser();
        var converter = new MatrixConverter();
        var matrixSolver = new MatrixSolver();
        var validator = new SolutionValidator();
        IEquationCache cache = enableCache
            ? new MemoryEquationCache()
            : new NullEquationCache();
        var options = new SolvingOptions
        {
            EnableCaching = enableCache,
            CacheTtl = TimeSpan.FromMinutes(30),
            DefaultAlgorithm = SolvingAlgorithm.LU
        };

        return new EquationSolver(parser, converter, matrixSolver, validator, cache, options);
    }

    private static string GenerateComplexEquation(int size)
    {
        var equations = new List<string>();
        for (int i = 0; i < size; i++)
        {
            var terms = new List<string>();
            for (int j = 0; j < size; j++)
            {
                var coeff = i * size + j + 1;
                terms.Add($"{coeff}x{j + 1}");
            }
            var constant = size * size + i + 1;
            equations.Add($"{string.Join(" + ", terms)} = {constant}");
        }
        return string.Join("; ", equations);
    }

    private static List<string> GenerateEquations(int count)
    {
        var equations = new List<string>();
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            var a = random.Next(1, 10);
            var b = random.Next(1, 10);
            var c = random.Next(10, 100);
            var d = random.Next(1, 5);
            var e = random.Next(1, 5);
            var f = random.Next(10, 100);
            equations.Add($"{a}x + {b}y = {c}; {d}x - {e}y = {f}");
        }

        return equations;
    }
}
