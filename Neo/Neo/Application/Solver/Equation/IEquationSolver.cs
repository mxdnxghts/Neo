using MathNet.Numerics.LinearAlgebra;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Application.Solver.Equation;

/// <summary>
/// Primary interface for solving linear equation systems.
/// Supports synchronous and asynchronous operations, batch processing, and streaming.
/// </summary>
public interface IEquationSolver
{
    /// <summary>
    /// Solves a system of equations from a string input.
    /// Example: "2x + 3y = 5; x - y = 1"
    /// </summary>
    /// <param name="equationInput">The equation string.</param>
    /// <returns>A <see cref="Result{T}"/> containing the <see cref="Solution"/> or error.</returns>
    Result<Solution> Solve(string equationInput);

    /// <summary>
    /// Solves an existing <see cref="EquationSystem"/>.
    /// </summary>
    /// <param name="system">The equation system to solve.</param>
    /// <returns>A <see cref="Result{T}"/> containing the <see cref="Solution"/> or error.</returns>
    Result<Solution> Solve(EquationSystem system);

    /// <summary>
    /// Solves a system from matrix representation.
    /// </summary>
    /// <param name="coefficients">The coefficient matrix A.</param>
    /// <param name="constants">The constants vector b (for Ax = b).</param>
    /// <returns>A <see cref="Result{T}"/> containing the <see cref="Solution"/> or error.</returns>
    Result<Solution> Solve(Matrix<double> coefficients, Vector<double> constants);

    /// <summary>
    /// Asynchronously solves a system from a string input.
    /// </summary>
    /// <param name="equationInput">The equation string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the <see cref="Result{T}"/> with <see cref="Solution"/> or error.</returns>
    Task<Result<Solution>> SolveAsync(string equationInput, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously solves an existing <see cref="EquationSystem"/>.
    /// </summary>
    /// <param name="system">The equation system to solve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the <see cref="Result{T}"/> with <see cref="Solution"/> or error.</returns>
    Task<Result<Solution>> SolveAsync(EquationSystem system, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves multiple equation systems in parallel.
    /// </summary>
    /// <param name="inputs">Collection of equation strings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing results for all inputs.</returns>
    Task<Result<IReadOnlyList<Solution>>> SolveBatchAsync(
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams solutions for an async sequence of equation inputs.
    /// Useful for large datasets or real-time processing.
    /// </summary>
    /// <param name="equationStream">Async stream of equation strings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of successful solutions.</returns>
    IAsyncEnumerable<Solution> SolveStreamAsync(
        IAsyncEnumerable<string> equationStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves with custom options overriding defaults.
    /// </summary>
    /// <param name="input">The equation string.</param>
    /// <param name="options">Custom solving options.</param>
    /// <returns>A <see cref="Result{T}"/> containing the <see cref="Solution"/> or error.</returns>
    Result<Solution> SolveWithOptions(string input, SolvingOptions options);

    /// <summary>
    /// Solves using a specific algorithm.
    /// </summary>
    /// <param name="input">The equation string.</param>
    /// <param name="algorithm">The solving algorithm to use.</param>
    /// <returns>A <see cref="Result{T}"/> containing the <see cref="Solution"/> or error.</returns>
    Result<Solution> SolveWithAlgorithm(string input, SolvingAlgorithm algorithm);
}

/// <summary>
/// Available algorithms for solving linear systems.
/// </summary>
public enum SolvingAlgorithm
{
    /// <summary>
    /// LU decomposition with partial pivoting. Fast for square matrices.
    /// </summary>
    LU,

    /// <summary>
    /// QR decomposition. Works for non-square matrices.
    /// </summary>
    QR,

    /// <summary>
    /// Cholesky decomposition. Fastest for symmetric positive-definite matrices.
    /// </summary>
    Cholesky,

    /// <summary>
    /// Singular Value Decomposition. Most stable, handles ill-conditioned matrices.
    /// </summary>
    SVD
}

/// <summary>
/// Configuration options for solving equations.
/// </summary>
public record SolvingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled.
    /// Default: true
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the time-to-live for cached solutions.
    /// Default: 30 minutes
    /// </summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the default solving algorithm.
    /// Default: LU
    /// </summary>
    public SolvingAlgorithm DefaultAlgorithm { get; set; } = SolvingAlgorithm.LU;

    /// <summary>
    /// Gets or sets the tolerance for solution validation (residual check).
    /// Default: 1e-10
    /// </summary>
    public double ValidationTolerance { get; set; } = 1e-10;

    /// <summary>
    /// Gets or sets the maximum degree of parallelism for batch operations.
    /// -1 means use system default.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = -1;
}