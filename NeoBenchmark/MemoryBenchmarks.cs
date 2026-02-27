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

namespace NeoBenchmark.Memory;

/// <summary>
/// Benchmarks for memory allocation patterns.
/// </summary>
[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class MemoryBenchmarks
{
    private EquationSolver _solver = null!;
    private string _equation2x2 = null!;
    private string _equation5x5 = null!;
    private List<string> _batch100 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _solver = CreateEquationSolver();
        _equation2x2 = "2x + 3y = 5; x - y = 1";
        _equation5x5 = GenerateEquationString(5);
        _batch100 = GenerateEquations(100);
    }

    [Benchmark(Baseline = true)]
    public Result<Solution> Memory_SingleSolve_2x2() 
        => _solver.Solve(_equation2x2);

    [Benchmark]
    public Result<Solution> Memory_SingleSolve_5x5() 
        => _solver.Solve(_equation5x5);

    [Benchmark]
    public async Task<Result<IReadOnlyList<Solution>>> Memory_BatchSolve() 
        => await _solver.SolveBatchAsync(_batch100, CancellationToken.None);

    [Benchmark]
    public Result<Solution> Memory_WithCaching() 
        => _solver.Solve(_equation2x2);

    private static EquationSolver CreateEquationSolver()
    {
        var parser = new EquationParser();
        var converter = new MatrixConverter();
        var matrixSolver = new MatrixSolver();
        var validator = new SolutionValidator();
        var cache = new MemoryEquationCache();
        var options = new SolvingOptions
        {
            EnableCaching = true,
            DefaultAlgorithm = SolvingAlgorithm.LU
        };

        return new EquationSolver(parser, converter, matrixSolver, validator, cache, options);
    }

    private static string GenerateEquationString(int size)
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
        for (int i = 0; i < count; i++)
        {
            equations.Add($"2x + 3y = {i + 5}; x - y = 1");
        }
        return equations;
    }
}
