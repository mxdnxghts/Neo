using MathNet.Numerics.LinearAlgebra;
using Neo.Application.Caching;
using Neo.Application.Solver.Matrix;
using Neo.Application.Validators;
using Neo.Domain.Equation;
using Neo.Domain.Equation.Variables;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Matrix;
using Neo.Infrastructure.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Application.Solver.Equation;

/// <summary>
/// Orchestrator for solving linear equation systems.
/// Handles parsing, matrix conversion, algorithm selection, solving, validation, and caching.
/// </summary>
public sealed class EquationSolver : IEquationSolver
{
    private readonly IEquationParser _parser;
    private readonly IMatrixConverter _converter;
    private readonly IMatrixSolver _matrixSolver;
    private readonly ISolutionValidator _validator;
    private readonly IEquationCache _cache;
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly SolvingOptions _defaultOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquationSolver"/> class.
    /// </summary>
    /// <param name="parser">The equation parser.</param>
    /// <param name="converter">The matrix converter.</param>
    /// <param name="matrixSolver">The matrix solver.</param>
    /// <param name="validator">The solution validator.</param>
    /// <param name="cache">Optional cache. Uses <see cref="NullEquationCache"/> if not provided.</param>
    /// <param name="performanceMonitor">Optional performance monitor.</param>
    /// <param name="defaultOptions">Default solving options.</param>
    public EquationSolver(
        IEquationParser parser,
        IMatrixConverter converter,
        IMatrixSolver matrixSolver,
        ISolutionValidator validator,
        IEquationCache? cache = null,
        PerformanceMonitor? performanceMonitor = null,
        SolvingOptions? defaultOptions = null)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _matrixSolver = matrixSolver ?? throw new ArgumentNullException(nameof(matrixSolver));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _cache = cache ?? new NullEquationCache();
        _performanceMonitor = performanceMonitor ?? new PerformanceMonitor();
        _defaultOptions = defaultOptions ?? new SolvingOptions();
    }

    // ----------------------------------------------------------------------
    // Public API – synchronous
    // ----------------------------------------------------------------------

    /// <inheritdoc/>
    public Result<Solution> Solve(string equationInput)
        => SolveInternal(equationInput, _defaultOptions);

    /// <inheritdoc/>
    public Result<Solution> Solve(EquationSystem system)
        => SolveInternal(system, _defaultOptions);

    /// <inheritdoc/>
    public Result<Solution> Solve(Matrix<double> coefficients, Vector<double> constants)
    {
        var systemResult = BuildSystemFromMatrix(coefficients, constants);
        if (systemResult.IsFailure)
            return Result<Solution>.Failure(systemResult.Error!);

        return SolveInternal(systemResult.Value!, _defaultOptions);
    }

    // ----------------------------------------------------------------------
    // Public API – asynchronous (single)
    // ----------------------------------------------------------------------

    /// <inheritdoc/>
    public Task<Result<Solution>> SolveAsync(string equationInput, CancellationToken cancellationToken = default)
        => Task.Run(() => Solve(equationInput), cancellationToken);

    /// <inheritdoc/>
    public Task<Result<Solution>> SolveAsync(EquationSystem system, CancellationToken cancellationToken = default)
        => Task.Run(() => Solve(system), cancellationToken);

    // ----------------------------------------------------------------------
    // Public API – batch and streaming
    // ----------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<Solution>>> SolveBatchAsync(
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var inputList = inputs.ToList();
        if (inputList.Count == 0)
            return Result<IReadOnlyList<Solution>>.Success(Array.Empty<Solution>());

        var solutions = new Solution[inputList.Count];
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _defaultOptions.MaxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        try
        {
            await Task.Run(() =>
            {
                Parallel.For(0, inputList.Count, options, i =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var result = Solve(inputList[i]);
                    if (result.IsSuccess)
                        solutions[i] = result.Value!;
                });
            }, cancellationToken);

            return Result<IReadOnlyList<Solution>>.Success(solutions.Where(s => s != null).ToList());
        }
        catch (OperationCanceledException)
        {
            return Result<IReadOnlyList<Solution>>.Failure(
                new Error("Batch solving cancelled", "CANCELLED"));
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<Solution> SolveStreamAsync(
        IAsyncEnumerable<string> equationStream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var input in equationStream.WithCancellation(cancellationToken))
        {
            var result = Solve(input);
            if (result.IsSuccess)
                yield return result.Value!;
        }
    }

    // ----------------------------------------------------------------------
    // Public API – with options override
    // ----------------------------------------------------------------------

    /// <inheritdoc/>
    public Result<Solution> SolveWithOptions(string input, SolvingOptions options)
        => SolveInternal(input, options);

    /// <inheritdoc/>
    public Result<Solution> SolveWithAlgorithm(string input, SolvingAlgorithm algorithm)
    {
        var options = _defaultOptions with { DefaultAlgorithm = algorithm };
        return SolveInternal(input, options);
    }

    // ----------------------------------------------------------------------
    // Internal solving logic
    // ----------------------------------------------------------------------

    private Result<Solution> SolveInternal(string input, SolvingOptions options)
    {
        using var activity = _performanceMonitor.StartActivity("SolveString");
        if (options.EnableCaching)
        {
            var key = ComputeHash(input);
            var cached = _cache.GetSolution(key);
            if (cached.IsSuccess && cached.Value != null)
            {
                activity.SetSuccess(true);
                return Result<Solution>.Success(cached.Value);
            }
        }

        var parseResult = _parser.Parse(input);
        if (parseResult.IsFailure)
        {
            activity.SetSuccess(false);
            return Result<Solution>.Failure(parseResult.Error!);
        }

        var system = parseResult.Value!;
        var solutionResult = SolveInternal(system, options);

        if (solutionResult.IsSuccess && options.EnableCaching)
        {
            var key = ComputeHash(input);
            _cache.SetSolution(key, solutionResult.Value!, options.CacheTtl);
        }

        activity.SetSuccess(solutionResult.IsSuccess);
        return solutionResult;
    }

    private Result<Solution> SolveInternal(EquationSystem system, SolvingOptions options)
    {
        using var activity = _performanceMonitor.StartActivity("SolveSystem");
        var matrixResult = _converter.ToMathNetMatrix(system);
        if (matrixResult.IsFailure)
        {
            activity.SetSuccess(false);
            return Result<Solution>.Failure(matrixResult.Error!);
        }

        var vectorResult = _converter.ToMathNetVector(system);
        if (vectorResult.IsFailure)
        {
            activity.SetSuccess(false);
            return Result<Solution>.Failure(vectorResult.Error!);
        }

        var a = matrixResult.Value;
        var b = vectorResult.Value;

        var status = _validator.DetermineStatus(system, a, b);
        if (status != SolutionStatus.Success)
        {
            activity.SetSuccess(false);
            return status switch
            {
                SolutionStatus.NoSolution => Result<Solution>.Failure(
                    new Error("System has no solution", "NO_SOLUTION")),
                SolutionStatus.InfiniteSolutions => Result<Solution>.Failure(
                    new Error("System has infinitely many solutions", "INFINITE_SOLUTIONS")),
                _ => Result<Solution>.Failure(new Error("Invalid system", "INVALID_SYSTEM"))
            };
        }

        var algorithm = SelectAlgorithm(a, options);
        var solveResult = _matrixSolver.Solve(a, b, algorithm);
        if (solveResult.IsFailure)
        {
            activity.SetSuccess(false);
            return Result<Solution>.Failure(solveResult.Error!);
        }

        var x = solveResult.Value;

        var values = system.Variables
            .Select((v, i) => (v, x[i]))
            .ToDictionary(t => t.v, t => t.Item2);

        var solution = Solution.Success(system, values);

        var validation = _validator.Validate(system, solution, options.ValidationTolerance);
        if (validation.IsFailure)
        {
            activity.SetSuccess(false);
            return Result<Solution>.Failure(validation.Error!);
        }

        activity.SetSuccess(true);
        return Result<Solution>.Success(solution);
    }

    // ----------------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------------

    /// <summary>
    /// Selects the optimal solving algorithm based on matrix properties and options.
    /// </summary>
    /// <param name="a">The coefficient matrix.</param>
    /// <param name="options">The solving options.</param>
    /// <returns>The selected algorithm.</returns>
    private SolvingAlgorithm SelectAlgorithm(Matrix<double> a, SolvingOptions options)
    {
        if (a.RowCount != a.ColumnCount)
            return SolvingAlgorithm.QR;

        if (a.IsSymmetric() && IsPositiveDefinite(a))
            return SolvingAlgorithm.Cholesky;

        return options.DefaultAlgorithm;
    }

    /// <summary>
    /// Checks if a symmetric matrix is positive definite using Cholesky decomposition.
    /// </summary>
    /// <param name="matrix">The symmetric matrix to check.</param>
    /// <returns><c>true</c> if positive definite; otherwise, <c>false</c>.</returns>
    private static bool IsPositiveDefinite(Matrix<double> matrix)
    {
        try
        {
            var chol = matrix.Cholesky();
            return chol.Determinant > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Computes a SHA-256 hash of the normalized input for cache keys.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>Base64-encoded hash.</returns>
    private static string ComputeHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input.Trim().ToLowerInvariant());
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Builds an <see cref="EquationSystem"/> from matrix representation.
    /// </summary>
    /// <param name="coefficients">The coefficient matrix.</param>
    /// <param name="constants">The constants vector.</param>
    /// <returns>The equation system.</returns>
    private Result<EquationSystem> BuildSystemFromMatrix(Matrix<double> coefficients, Vector<double> constants)
    {
        if (coefficients.RowCount != constants.Count)
            return Result<EquationSystem>.Failure(
                new Error("Row count of coefficient matrix must match length of constant vector", "DIMENSION_MISMATCH"));

        int rows = coefficients.RowCount;
        int cols = coefficients.ColumnCount;

        var variables = Enumerable.Range(1, cols)
            .Select(i => Variable.Create($"x{i}"))
            .ToList();

        var equations = new List<LinearEquation>();

        for (int i = 0; i < rows; i++)
        {
            var coeffDict = new Dictionary<Variable, double>();
            for (int j = 0; j < cols; j++)
            {
                coeffDict[variables[j]] = coefficients[i, j];
            }
            equations.Add(LinearEquation.FromDictionary(coeffDict, constants[i]));
        }

        return Result<EquationSystem>.Success(new EquationSystem(equations));
    }
}
