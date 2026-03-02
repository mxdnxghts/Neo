using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Neo.Application.Caching;
using Neo.Domain.Equation;
using Neo.Domain.Result;
using Neo.Domain.Solution;
using Neo.Infrastructure.Integration;
using Neo.Infrastructure.Telemetry;

namespace Neo.Application.Solver.Equation;

/// <summary>
/// Decorator for <see cref="IEquationSolver"/> that adds telemetry and performance monitoring.
/// Wraps a core solver instance and records metrics, traces, and errors for all operations.
/// Also handles caching to avoid redundant computations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TelemetryEquationSolverDecorator"/> class.
/// </remarks>
/// <param name="inner">The inner equation solver to decorate.</param>
/// <param name="telemetry">The telemetry service.</param>
/// <param name="performanceMonitor">Optional performance monitor.</param>
/// <param name="cache">Optional cache for storing solutions.</param>
/// <param name="options">Solving options including cache settings.</param>
public sealed class TelemetryEquationSolverDecorator(
    IEquationSolver inner,
    NeoTelemetryService telemetry,
    PerformanceMonitor? performanceMonitor = null,
    IEquationCache? cache = null,
    SolvingOptions? options = null) : EquationSolverDecorator(inner)
{
    private readonly IEquationSolver _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly NeoTelemetryService _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
    private readonly PerformanceMonitor _performanceMonitor = performanceMonitor ?? new PerformanceMonitor();
    private readonly IEquationCache _cache = cache ?? new NullEquationCache();
    private readonly SolvingOptions _options = options ?? new SolvingOptions();

    // ----------------------------------------------------------------------
    // Public API – synchronous
    // ----------------------------------------------------------------------


    /// <inheritdoc/>
    public override Result<Solution> Solve(string equationInput)
    {
        using var perfActivity = _performanceMonitor.StartActivity("SolveString");
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveString");
        var cacheStopwatch = Stopwatch.StartNew();

        // Check cache first
        if (_options.EnableCaching)
        {
            var key = ComputeHash(equationInput);
            var cached = _cache.GetSolution(key);
            cacheStopwatch.Stop();

            if (cached.IsSuccess && cached.Value != null)
            {
                _telemetry.RecordCacheHit(cacheStopwatch.Elapsed);
                telemetryActivity?.SetCacheTags(true, cacheStopwatch.Elapsed);
                var solution = cached.Value;
                var algorithm = solution.AlgorithmUsed?.ToString() ?? "Cache";
                _telemetry.RecordEquationSolved(
                    solution.OriginalSystem.VariableCount,
                    cacheStopwatch.Elapsed,
                    algorithm);
                telemetryActivity?.SetSolutionTags(algorithm, true, cacheStopwatch.Elapsed);
                perfActivity.SetSuccess(true);
                return Result<Solution>.Success(solution);
            }
            _telemetry.RecordCacheMiss(cacheStopwatch.Elapsed);
            telemetryActivity?.SetCacheTags(false, cacheStopwatch.Elapsed);
        }

        // Solve
        try
        {
            var result = _inner.Solve(equationInput);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                var solution = result.Value;
                var algorithm = solution.AlgorithmUsed?.ToString() ?? "Unknown";
                _telemetry.RecordEquationSolved(
                    solution.OriginalSystem.VariableCount,
                    stopwatch.Elapsed,
                    algorithm);
                telemetryActivity?.SetSolutionTags(algorithm, true, stopwatch.Elapsed);
                perfActivity.SetSuccess(true);

                // Cache the result
                if (_options.EnableCaching)
                {
                    var key = ComputeHash(equationInput);
                    _cache.SetSolution(key, solution, _options.CacheTtl);
                }
            }
            else
            {
                _telemetry.RecordError("SolveString", result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "");
                perfActivity.SetSuccess(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry.RecordError("SolveString", "EXCEPTION", ex.Message);
            telemetryActivity?.SetExceptionTags(ex);
            perfActivity.SetSuccess(false);
            throw;
        }
    }

    /// <inheritdoc/>
    public override Result<Solution> Solve(EquationSystem system)
    {
        using var perfActivity = _performanceMonitor.StartActivity("SolveSystem");
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveSystem");

        try
        {
            telemetryActivity?.SetEquationSystemTags(system.EquationCount, system.VariableCount);

            var result = _inner.Solve(system);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                var solution = result.Value;
                var algorithm = solution.AlgorithmUsed?.ToString() ?? "Unknown";
                _telemetry.RecordEquationSolved(
                    system.VariableCount,
                    stopwatch.Elapsed,
                    algorithm);
                telemetryActivity?.SetSolutionTags(algorithm, true, stopwatch.Elapsed);
                perfActivity.SetSuccess(true);
            }
            else
            {
                _telemetry.RecordError("SolveSystem", result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "");
                perfActivity.SetSuccess(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry.RecordError("SolveSystem", "EXCEPTION", ex.Message);
            telemetryActivity?.SetExceptionTags(ex);
            perfActivity.SetSuccess(false);
            throw;
        }
    }

    /// <inheritdoc/>
    public override Result<Solution> Solve(Matrix<double> coefficients, Vector<double> constants)
    {
        using var perfActivity = _performanceMonitor.StartActivity("SolveMatrix");
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveMatrix");

        try
        {
            telemetryActivity?.SetMatrixTags(coefficients);

            var result = _inner.Solve(coefficients, constants);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                var solution = result.Value;
                var algorithm = solution.AlgorithmUsed?.ToString() ?? "Unknown";
                _telemetry.RecordEquationSolved(
                    solution.OriginalSystem.VariableCount,
                    stopwatch.Elapsed,
                    algorithm);
                telemetryActivity?.SetSolutionTags(algorithm, true, stopwatch.Elapsed);
                perfActivity.SetSuccess(true);
            }
            else
            {
                _telemetry.RecordError("SolveMatrix", result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "");
                perfActivity.SetSuccess(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry.RecordError("SolveMatrix", "EXCEPTION", ex.Message);
            telemetryActivity?.SetExceptionTags(ex);
            perfActivity.SetSuccess(false);
            throw;
        }
    }

    // ----------------------------------------------------------------------
    // Public API – asynchronous (single)
    // ----------------------------------------------------------------------

    /// <inheritdoc/>
    public override async Task<Result<Solution>> SolveAsync(string equationInput, CancellationToken cancellationToken = default)
    {
        using var perfActivity = _performanceMonitor.StartActivity("SolveStringAsync");
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveStringAsync");

        try
        {
            var result = await _inner.SolveAsync(equationInput, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                var solution = result.Value;
                var algorithm = solution.AlgorithmUsed?.ToString() ?? "Unknown";
                _telemetry.RecordEquationSolved(
                    solution.OriginalSystem.VariableCount,
                    stopwatch.Elapsed,
                    algorithm);
                telemetryActivity?.SetSolutionTags(algorithm, true, stopwatch.Elapsed);
                perfActivity.SetSuccess(true);
            }
            else
            {
                _telemetry.RecordError("SolveStringAsync", result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "");
                perfActivity.SetSuccess(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry.RecordError("SolveStringAsync", "EXCEPTION", ex.Message);
            telemetryActivity?.SetExceptionTags(ex);
            perfActivity.SetSuccess(false);
            throw;
        }
    }

    /// <inheritdoc/>
    public override async Task<Result<Solution>> SolveAsync(EquationSystem system, CancellationToken cancellationToken = default)
    {
        using var perfActivity = _performanceMonitor.StartActivity("SolveSystemAsync");
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveSystemAsync");

        try
        {
            telemetryActivity?.SetEquationSystemTags(system.EquationCount, system.VariableCount);

            var result = await _inner.SolveAsync(system, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                var solution = result.Value;
                var algorithm = solution.AlgorithmUsed?.ToString() ?? "Unknown";
                _telemetry.RecordEquationSolved(
                    system.VariableCount,
                    stopwatch.Elapsed,
                    algorithm);
                telemetryActivity?.SetSolutionTags(algorithm, true, stopwatch.Elapsed);
                perfActivity.SetSuccess(true);
            }
            else
            {
                _telemetry.RecordError("SolveSystemAsync", result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "");
                perfActivity.SetSuccess(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry.RecordError("SolveSystemAsync", "EXCEPTION", ex.Message);
            telemetryActivity?.SetExceptionTags(ex);
            perfActivity.SetSuccess(false);
            throw;
        }
    }

    // ----------------------------------------------------------------------
    // Public API – batch and streaming
    // ----------------------------------------------------------------------

    /// <inheritdoc/>
    public override async Task<Result<IReadOnlyList<Solution>>> SolveBatchAsync(
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveBatchAsync");

        try
        {
            var inputList = inputs.ToList();
            telemetryActivity?.SetTag("neo.batch.count", inputList.Count);
            
            var result = await _inner.SolveBatchAsync(inputList, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _telemetry.RecordEquationSolved(
                    result.Value?.Count ?? 0,
                    stopwatch.Elapsed,
                    "Batch");
                telemetryActivity?.SetSolutionTags("Batch", true, stopwatch.Elapsed);
            }
            else
            {
                _telemetry.RecordError("SolveBatchAsync", result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "");
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry.RecordError("SolveBatchAsync", "EXCEPTION", ex.Message);
            telemetryActivity?.SetExceptionTags(ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<Solution> SolveStreamAsync(
        IAsyncEnumerable<string> equationStream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveStreamAsync");
        int count = 0;
        Solution? lastSolution = null;

        // Note: Cannot use try-catch with yield return, so we wrap the inner call instead
        await foreach (var input in equationStream.WithCancellation(cancellationToken))
        {
            var itemStopwatch = Stopwatch.StartNew();

            // Wrap inner solve to capture errors
            Result<Solution> result;
            try
            {
                result = _inner.Solve(input);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _telemetry.RecordError("SolveStreamAsync", "EXCEPTION", ex.Message);
                telemetryActivity?.SetExceptionTags(ex);
                telemetryActivity?.SetTag("neo.stream.count_before_failure", count);
                throw;
            }

            itemStopwatch.Stop();

            if (result.IsSuccess)
            {
                count++;
                lastSolution = result.Value;
                var algorithm = result.Value.AlgorithmUsed?.ToString() ?? "Stream";
                _telemetry.RecordEquationSolved(
                    result.Value.OriginalSystem.VariableCount,
                    itemStopwatch.Elapsed,
                    algorithm);
                yield return result.Value;
            }
        }

        stopwatch.Stop();
        telemetryActivity?.SetTag("neo.stream.count", count);
        telemetryActivity?.SetSolutionTags("Stream", true, stopwatch.Elapsed);
    }

    // ----------------------------------------------------------------------
    // Public API – with options override
    // ----------------------------------------------------------------------

    /// <inheritdoc/>
    public override Result<Solution> SolveWithOptions(string input, SolvingOptions options)
    {
        using var perfActivity = _performanceMonitor.StartActivity("SolveWithOptions");
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveWithOptions");

        try
        {
            var result = _inner.SolveWithOptions(input, options);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                var solution = result.Value;
                _telemetry.RecordEquationSolved(
                    solution.OriginalSystem.VariableCount,
                    stopwatch.Elapsed,
                    options.DefaultAlgorithm.ToString());
                telemetryActivity?.SetSolutionTags(options.DefaultAlgorithm.ToString(), true, stopwatch.Elapsed);
                perfActivity.SetSuccess(true);
            }
            else
            {
                _telemetry.RecordError("SolveWithOptions", result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "");
                perfActivity.SetSuccess(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry.RecordError("SolveWithOptions", "EXCEPTION", ex.Message);
            telemetryActivity?.SetExceptionTags(ex);
            perfActivity.SetSuccess(false);
            throw;
        }
    }

    /// <inheritdoc/>
    public override Result<Solution> SolveWithAlgorithm(string input, SolvingAlgorithm algorithm)
    {
        using var perfActivity = _performanceMonitor.StartActivity("SolveWithAlgorithm");
        var stopwatch = Stopwatch.StartNew();
        var telemetryActivity = _telemetry.StartSolveActivity("SolveWithAlgorithm");

        try
        {
            var result = _inner.SolveWithAlgorithm(input, algorithm);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                var solution = result.Value;
                _telemetry.RecordEquationSolved(
                    solution.OriginalSystem.VariableCount,
                    stopwatch.Elapsed,
                    algorithm.ToString());
                telemetryActivity?.SetSolutionTags(algorithm.ToString(), true, stopwatch.Elapsed);
                perfActivity.SetSuccess(true);
            }
            else
            {
                _telemetry.RecordError("SolveWithAlgorithm", result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "");
                perfActivity.SetSuccess(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry.RecordError("SolveWithAlgorithm", "EXCEPTION", ex.Message);
            telemetryActivity?.SetExceptionTags(ex);
            perfActivity.SetSuccess(false);
            throw;
        }
    }

    // ----------------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------------

    /// <summary>
    /// Computes a SHA-256 hash of the normalized input for cache keys.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>Base64-encoded hash.</returns>
    private static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input.Trim().ToLowerInvariant());
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
