using BenchmarkDotNet.Attributes;
using MathNet.Numerics.LinearAlgebra;
using Neo.Application.Caching;
using Neo.Application.Solver.Equation;
using Neo.Application.Solver.Matrix;
using Neo.Application.Validators;
using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Matrix;
using Neo.Infrastructure.Parsing;

namespace NeoBenchmark.Solving;

/// <summary>
/// Benchmarks for equation solving performance.
/// </summary>
[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class SolvingBenchmarks
{
    private EquationSolver _solver = null!;
    private string _equation2x2 = null!;
    private string _equation3x3 = null!;
    private string _equation5x5 = null!;
    private string _equation10x10 = null!;
    private string _equationSPD = null!;

    [GlobalSetup]
    public void Setup()
    {
        _solver = CreateEquationSolver();
        _equation2x2 = GenerateEquationString(2);
        _equation3x3 = GenerateEquationString(3);
        _equation5x5 = GenerateEquationString(5);
        _equation10x10 = GenerateEquationString(10);
        _equationSPD = GenerateSymmetricPositiveDefiniteEquation(4);
    }

    // Size-based benchmarks with LU (default)
    [Benchmark(Baseline = true)]
    public Result<Solution> Solve_2x2_LU() 
        => _solver.Solve(_equation2x2);

    [Benchmark]
    public Result<Solution> Solve_3x3_LU() 
        => _solver.Solve(_equation3x3);

    [Benchmark]
    public Result<Solution> Solve_5x5_LU() 
        => _solver.Solve(_equation5x5);

    [Benchmark]
    public Result<Solution> Solve_10x10_LU() 
        => _solver.Solve(_equation10x10);

    // Algorithm comparison on 3x3
    [Benchmark]
    public Result<Solution> Solve_3x3_QR() 
        => _solver.SolveWithAlgorithm(_equation3x3, SolvingAlgorithm.QR);

    [Benchmark]
    public Result<Solution> Solve_3x3_Cholesky() 
        => _solver.SolveWithAlgorithm(_equation3x3, SolvingAlgorithm.Cholesky);

    [Benchmark]
    public Result<Solution> Solve_3x3_SVD() 
        => _solver.SolveWithAlgorithm(_equation3x3, SolvingAlgorithm.SVD);

    // Algorithm comparison on 5x5
    [Benchmark]
    public Result<Solution> Solve_5x5_QR() 
        => _solver.SolveWithAlgorithm(_equation5x5, SolvingAlgorithm.QR);

    [Benchmark]
    public Result<Solution> Solve_5x5_Cholesky() 
        => _solver.SolveWithAlgorithm(_equation5x5, SolvingAlgorithm.Cholesky);

    [Benchmark]
    public Result<Solution> Solve_5x5_SVD() 
        => _solver.SolveWithAlgorithm(_equation5x5, SolvingAlgorithm.SVD);

    // Special matrix types
    [Benchmark]
    public Result<Solution> Solve_SPD_Cholesky() 
        => _solver.SolveWithAlgorithm(_equationSPD, SolvingAlgorithm.Cholesky);

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
            DefaultAlgorithm = SolvingAlgorithm.LU
        };

        return new EquationSolver(parser, converter, matrixSolver, validator, cache, null, options);
    }

    private static string GenerateEquationString(int size)
    {
        var equations = new List<string>();
        for (int i = 0; i < size; i++)
        {
            var terms = new List<string>();
            for (int j = 0; j < size; j++)
            {
                // Create a diagonally dominant matrix for stability
                var coeff = i == j ? size + 1 : 1;
                terms.Add($"{coeff}x{j + 1}");
            }
            var constant = size * (size + 1) + i;
            equations.Add($"{string.Join(" + ", terms)} = {constant}");
        }
        return string.Join("; ", equations);
    }

    private static string GenerateSymmetricPositiveDefiniteEquation(int size)
    {
        // Create a simple SPD system: diagonal dominant symmetric matrix
        var equations = new List<string>();
        for (int i = 0; i < size; i++)
        {
            var terms = new List<string>();
            for (int j = 0; j < size; j++)
            {
                // Symmetric matrix with diagonal dominance
                var coeff = i == j ? size * 2 : 1;
                terms.Add($"{coeff}x{j + 1}");
            }
            var constant = size * size * 2;
            equations.Add($"{string.Join(" + ", terms)} = {constant}");
        }
        return string.Join("; ", equations);
    }
}
