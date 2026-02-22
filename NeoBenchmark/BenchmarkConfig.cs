using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace NeoBenchmark;

/// <summary>
/// Shared benchmark configuration for all Neo benchmarks.
/// </summary>
public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        // Job configuration
        AddJob(Job.Default
            .WithWarmupCount(3)
            .WithIterationCount(10)
            .WithInvocationCount(100)
            .WithUnrollFactor(10)
            .WithId("Net10"));

        // Diagnosers
        AddDiagnoser(MemoryDiagnoser.Default);

        // Ordering
        WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));
    }
}
