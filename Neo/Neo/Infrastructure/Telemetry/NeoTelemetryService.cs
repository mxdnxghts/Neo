using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Neo.Infrastructure.Telemetry;

/// <summary>
/// Centralized telemetry service for the Neo equation solver.
/// Provides distributed tracing, metrics, and logging integration.
/// </summary>
public class NeoTelemetryService : IDisposable
{
    private readonly ActivitySource _activitySource;
    private readonly Meter _meter;
    private readonly ILogger<NeoTelemetryService> _logger;
    private readonly Counter<long> _equationsSolvedCounter;
    private readonly Counter<long> _errorsCounter;
    private readonly Histogram<double> _solveDurationHistogram;
    private readonly Histogram<int> _variableCountHistogram;
    private readonly Counter<long> _cacheHitsCounter;
    private readonly Counter<long> _cacheMissesCounter;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NeoTelemetryService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public NeoTelemetryService(ILogger<NeoTelemetryService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activitySource = new ActivitySource("Neo.EquationSolver", "1.0.0");
        _meter = new Meter("Neo.EquationSolver", "1.0.0");
        
        _equationsSolvedCounter = _meter.CreateCounter<long>(
            "neo.equations.solved.total",
            description: "Total number of equations solved");
        
        _errorsCounter = _meter.CreateCounter<long>(
            "neo.errors.total",
            description: "Total number of errors encountered");
        
        _solveDurationHistogram = _meter.CreateHistogram<double>(
            "neo.solve.duration",
            unit: "ms",
            description: "Time taken to solve equations");
        
        _variableCountHistogram = _meter.CreateHistogram<int>(
            "neo.equation.variables.count",
            description: "Number of variables in solved equations");
        
        _cacheHitsCounter = _meter.CreateCounter<long>(
            "neo.cache.hits.total",
            description: "Total number of cache hits");
        
        _cacheMissesCounter = _meter.CreateCounter<long>(
            "neo.cache.misses.total",
            description: "Total number of cache misses");
    }

    /// <summary>
    /// Starts a tracing activity for solving an equation.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="equationHash">Optional equation hash for correlation.</param>
    /// <returns>The created activity, or null if tracing is disabled.</returns>
    public virtual Activity? StartSolveActivity(string operationName, string? equationHash = null)
    {
        var activity = _activitySource.StartActivity(operationName, ActivityKind.Internal);
        
        if (activity != null && equationHash != null)
        {
            activity.SetTag("neo.equation.hash", equationHash);
        }
        
        return activity;
    }

    /// <summary>
    /// Records a successfully solved equation.
    /// </summary>
    /// <param name="variableCount">The number of variables in the equation.</param>
    /// <param name="duration">The time taken to solve.</param>
    /// <param name="algorithm">The algorithm used.</param>
    public virtual void RecordEquationSolved(int variableCount, TimeSpan duration, string algorithm)
    {
        var tags = new TagList
        {
            { "algorithm", algorithm },
            { "variableCount", variableCount }
        };
        
        _equationsSolvedCounter.Add(1, tags);
        _solveDurationHistogram.Record(duration.TotalMilliseconds, tags);
        _variableCountHistogram.Record(variableCount);
        
        _logger.LogDebug(
            "Equation solved with {VariableCount} variables using {Algorithm} in {DurationMs:F2}ms",
            variableCount, algorithm, duration.TotalMilliseconds);
    }

    /// <summary>
    /// Records an error during equation solving.
    /// </summary>
    /// <param name="operation">The operation that failed.</param>
    /// <param name="errorType">The type of error.</param>
    /// <param name="details">Optional error details.</param>
    public virtual void RecordError(string operation, string errorType, string? details = null)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "errorType", errorType }
        };
        
        _errorsCounter.Add(1, tags);
        
        _logger.LogError(
            "Error during {Operation}: {ErrorType}. Details: {Details}",
            operation, errorType, details ?? "N/A");
    }

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    /// <param name="lookupDuration">Optional cache lookup duration.</param>
    public virtual void RecordCacheHit(TimeSpan? lookupDuration = null)
    {
        _cacheHitsCounter.Add(1);
        _logger.LogDebug(
            "Cache HIT{LookupDuration}",
            lookupDuration.HasValue ? $" ({lookupDuration.Value.TotalMilliseconds:F2}ms)" : "");
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    /// <param name="lookupDuration">Optional cache lookup duration.</param>
    public virtual void RecordCacheMiss(TimeSpan? lookupDuration = null)
    {
        _cacheMissesCounter.Add(1);
        _logger.LogDebug(
            "Cache MISS{LookupDuration}",
            lookupDuration.HasValue ? $" ({lookupDuration.Value.TotalMilliseconds:F2}ms)" : "");
    }

    /// <summary>
    /// Gets the activity source for manual instrumentation.
    /// </summary>
    public virtual ActivitySource ActivitySource => _activitySource;

    /// <summary>
    /// Gets the meter for manual metrics recording.
    /// </summary>
    public virtual Meter Meter => _meter;

    /// <summary>
    /// Disposes of the telemetry resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _activitySource.Dispose();
                _meter.Dispose();
            }
            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
