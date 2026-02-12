using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Neo.Infrastructure.Integration;

public class PerformanceMonitor
{
    private readonly ConcurrentDictionary<string, PerformanceCounter> _counters = new();

    public void RecordOperation(string operationName, TimeSpan duration, bool success = true)
    {
        var counter = _counters.GetOrAdd(operationName, _ => new PerformanceCounter());
        counter.Record(duration, success);
    }

    public PerformanceStats GetStats(string operationName)
    {
        if (_counters.TryGetValue(operationName, out var counter))
            return counter.GetStats();

        return new PerformanceStats();
    }

    private class PerformanceCounter
    {
        private long _totalCount;
        private long _successCount;
        private long _totalDurationTicks;
        private long _minDurationTicks = long.MaxValue;
        private long _maxDurationTicks = long.MinValue;

        public void Record(TimeSpan duration, bool success)
        {
            Interlocked.Increment(ref _totalCount);
            if (success)
                Interlocked.Increment(ref _successCount);

            var ticks = duration.Ticks;
            Interlocked.Add(ref _totalDurationTicks, ticks);

            // Update min/max
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
}
public record PerformanceStats
{
    public long TotalOperations { get; set; }
    public long SuccessCount { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
}
