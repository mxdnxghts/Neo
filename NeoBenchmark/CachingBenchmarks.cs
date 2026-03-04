using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Neo.Application.Caching;
using Neo.Application.Solver.Equation;
using Neo.Application.Solver.Matrix;
using Neo.Application.Validators;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Matrix;
using Neo.Infrastructure.Parsing;

namespace NeoBenchmark.Caching;

/// <summary>
/// Benchmarks for caching performance.
/// </summary>
[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class CachingBenchmarks
{
    private EquationSolver _solverWithCache = null!;
    private EquationSolver _solverWithDicMemoryCache = null!;
    private EquationSolver _solverNoCache = null!;
    private string _equation = null!;

    [GlobalSetup]
    public void Setup()
    {
        _equation = "2x + 3y = 8; x - y = 1";
        _solverWithCache = CreateSolver(Caching.Memory);
        _solverWithDicMemoryCache = CreateSolver(Caching.DicMemory);
        _solverNoCache = CreateSolver(Caching.Null);

        // Warm up cache
        _solverWithCache.Solve(_equation);
        _solverWithDicMemoryCache.Solve(_equation);
    }

    [Benchmark(Baseline = true)]
    public Result<Solution> Solve_WithCache_Hit() 
        => _solverWithCache.Solve(_equation);
        
    [Benchmark]
    public Result<Solution> Solve_WithDictionaryMemoryCache_Hit() 
        => _solverWithDicMemoryCache.Solve(_equation);

    [Benchmark]
    public Result<Solution> Solve_NoCache() 
        => _solverNoCache.Solve(_equation);

    [Benchmark]
    public Result<Solution> Solve_WithCache_Miss()
    {
        // Create unique equation each time to simulate cache miss
        var uniqueEquation = $"2x + 3y = {DateTime.UtcNow.Ticks % 1000}; x - y = 1";
        return _solverWithCache.Solve(uniqueEquation);
    }

    private static EquationSolver CreateSolver(Caching caching)
    {
        var parser = new EquationParser();
        var converter = new MatrixConverter();
        var matrixSolver = new MatrixSolver();
        var validator = new SolutionValidator();
        IEquationCache cache = caching switch
        {
            Caching.Memory => new MemoryEquationCache(new MemoryCache(new MemoryCacheOptions())),
            Caching.DicMemory => new InternalMemoryEquationCache(),
            Caching.Null => new NullEquationCache(),
            _ => throw new ArgumentOutOfRangeException(nameof(caching), caching, null),
        };

        var options = new SolvingOptions
        {
            EnableCaching = caching != Caching.Null,
            CacheTtl = TimeSpan.FromMinutes(30),
            DefaultAlgorithm = SolvingAlgorithm.LU
        };

        return new EquationSolver(parser, converter, matrixSolver, validator, cache, options);
    }
}

enum Caching
{
    Memory,
    DicMemory,
    Null,
}
