using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Neo.Infrastructure.Integration;

/// <summary>
/// Thread-safe performance monitor for tracking operation durations and success rates.
/// Uses <see cref="ConcurrentDictionary{TKey,TValue}"/> and <see cref="Interlocked"/> operations.
/// </summary>
public class PerformanceMonitor
{
    private readonly ConcurrentDictionary<string, PerformanceCounter> _counters = new();

    /// <summary>
    /// Records an operation with its duration and success status.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="duration">The operation duration.</param>
    /// <param name="success">Whether the operation succeeded.</param>
    public void RecordOperation(string operationName, TimeSpan duration, bool success = true)
    {
        var counter = _counters.GetOrAdd(operationName, _ => new PerformanceCounter());
        counter.Record(duration, success);
    }

    /// <summary>
    /// Gets aggregated statistics for an operation.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <returns>Performance statistics.</returns>
    public PerformanceStats GetStats(string operationName)
    {
        if (_counters.TryGetValue(operationName, out var counter))
            return counter.GetStats();

        return new PerformanceStats();
    }

    /// <summary>
    /// Starts a timed activity for performance monitoring.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <returns>An <see cref="Activity"/> that records duration when disposed.</returns>
    public Activity StartActivity(string operationName) => new(this, operationName);

    /// <summary>
    /// Thread-safe counter for a single operation type.
    /// </summary>
    private class PerformanceCounter
    {
        private long _totalCount;
        private long _successCount;
        private long _totalDurationTicks;
        private long _minDurationTicks = long.MaxValue;
        private long _maxDurationTicks = long.MinValue;

        /// <summary>
        /// Records a single operation result.
        /// </summary>
        /// <param name="duration">Operation duration.</param>
        /// <param name="success">Success status.</param>
        public void Record(TimeSpan duration, bool success)
        {
            Interlocked.Increment(ref _totalCount);
            if (success)
                Interlocked.Increment(ref _successCount);

            var ticks = duration.Ticks;
            Interlocked.Add(ref _totalDurationTicks, ticks);

            long currentMin;
            do
            {
                currentMin = Volatile.Read(ref _minDurationTicks);
            } while (ticks < currentMin &&
                     Interlocked.CompareExchange(ref _minDurationTicks, ticks, currentMin) != currentMin);

            long currentMax;
            do
            {
                currentMax = Volatile.Read(ref _maxDurationTicks);
            } while (ticks > currentMax &&
                     Interlocked.CompareExchange(ref _maxDurationTicks, ticks, currentMax) != currentMax);
        }

        /// <summary>
        /// Gets aggregated statistics for this counter.
        /// </summary>
        /// <returns>Performance statistics.</returns>
        public PerformanceStats GetStats()
        {
            var totalCount = Volatile.Read(ref _totalCount);
            var successCount = Volatile.Read(ref _successCount);
            var totalTicks = Volatile.Read(ref _totalDurationTicks);
            var minTicks = Volatile.Read(ref _minDurationTicks);
            var maxTicks = Volatile.Read(ref _maxDurationTicks);

            return new PerformanceStats
            {
                TotalOperations = totalCount,
                SuccessCount = successCount,
                SuccessRate = totalCount > 0 ? (double)successCount / totalCount : 0,
                AverageDuration = totalCount > 0 ?
                    TimeSpan.FromTicks(totalTicks / totalCount) : TimeSpan.Zero,
                MinDuration = minTicks != long.MaxValue ?
                    TimeSpan.FromTicks(minTicks) : TimeSpan.Zero,
                MaxDuration = maxTicks != long.MinValue ?
                    TimeSpan.FromTicks(maxTicks) : TimeSpan.Zero
            };
        }
    }

    /// <summary>
    /// Represents a timed activity that records its duration when disposed.
    /// </summary>
    public sealed class Activity : IDisposable
    {
        private readonly PerformanceMonitor _monitor;
        private readonly string _operationName;
        private readonly DateTime _startTime;
        private bool _disposed;
        private bool _success = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        /// <param name="monitor">The performance monitor.</param>
        /// <param name="operationName">The operation name.</param>
        public Activity(PerformanceMonitor monitor, string operationName)
        {
            _monitor = monitor;
            _operationName = operationName;
            _startTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the success status of the activity.
        /// </summary>
        /// <param name="success">The success status.</param>
        public void SetSuccess(bool success) => _success = success;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                var duration = DateTime.UtcNow - _startTime;
                _monitor.RecordOperation(_operationName, duration, _success);
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}

/// <summary>
/// Aggregated performance statistics for an operation.
/// </summary>
public record PerformanceStats
{
    /// <summary>
    /// Gets the total number of operations recorded.
    /// </summary>
    public long TotalOperations { get; set; }

    /// <summary>
    /// Gets the number of successful operations.
    /// </summary>
    public long SuccessCount { get; set; }

    /// <summary>
    /// Gets the success rate (0.0 to 1.0).
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Gets the average operation duration.
    /// </summary>
    public TimeSpan AverageDuration { get; set; }

    /// <summary>
    /// Gets the minimum operation duration.
    /// </summary>
    public TimeSpan MinDuration { get; set; }

    /// <summary>
    /// Gets the maximum operation duration.
    /// </summary>
    public TimeSpan MaxDuration { get; set; }
}
