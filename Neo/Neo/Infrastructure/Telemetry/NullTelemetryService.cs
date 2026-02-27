using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Neo.Infrastructure.Telemetry;

/// <summary>
/// Null object implementation of <see cref="NeoTelemetryService"/>.
/// Used when telemetry is disabled or not configured.
/// </summary>
internal sealed class NullTelemetryService : NeoTelemetryService
{
    private static readonly NullTelemetryService _instance = new();

    private NullTelemetryService() 
        : base(new NullLogger<NeoTelemetryService>())
    {
    }

    public static NullTelemetryService Instance => _instance;

    public override Activity? StartSolveActivity(string operationName, string? equationHash = null)
    {
        return null;
    }

    public override void RecordEquationSolved(int variableCount, TimeSpan duration, string algorithm)
    {
        // No-op
    }

    public override void RecordError(string operation, string errorType, string? details = null)
    {
        // No-op
    }

    public override void RecordCacheHit(TimeSpan? lookupDuration = null)
    {
        // No-op
    }

    public override void RecordCacheMiss(TimeSpan? lookupDuration = null)
    {
        // No-op
    }

    public override ActivitySource ActivitySource { get; } = new ActivitySource("Neo.NullTelemetry");
    public override Meter Meter { get; } = new Meter("Neo.NullTelemetry");

    protected override void Dispose(bool disposing)
    {
        // Don't dispose the singleton instances
    }
}

/// <summary>
/// Null logger implementation for use with <see cref="NullTelemetryService"/>.
/// </summary>
internal sealed class NullLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        Microsoft.Extensions.Logging.EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // No-op
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
